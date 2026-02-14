using System.Buffers.Binary;
using System.Text;
using DreamKeeper.Data.Data;
using Whisper.net;
using Whisper.net.Ggml;

namespace DreamKeeper.Services
{
    /// <summary>
    /// Transcribes WAV audio to text using Whisper.net (on-device, free).
    /// Downloads the ggml-tiny model on first use.
    /// </summary>
    public sealed class WhisperTranscriptionService : ITranscriptionService, IDisposable
    {
        private static readonly string ModelDirectory = Path.Combine(FileSystem.AppDataDirectory, "whisper");
        private static readonly string ModelPath = Path.Combine(ModelDirectory, "ggml-tiny.bin");

        private readonly SemaphoreSlim _initLock = new(1, 1);
        private WhisperFactory? _factory;
        private bool _disposed;

        /// <summary>
        /// Transcribes WAV audio bytes to text using the Whisper tiny model.
        /// </summary>
        public async Task<string> TranscribeAsync(byte[] audioData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(audioData);

            if (audioData.Length == 0)
                return string.Empty;

            await EnsureModelLoadedAsync(cancellationToken).ConfigureAwait(false);

            if (_factory is null)
                return string.Empty;

            var tempWavPath = Path.Combine(FileSystem.CacheDirectory, $"transcribe_{Guid.NewGuid()}.wav");
            try
            {
                var resampledData = ResampleWavTo16KhzMono(audioData);
                await File.WriteAllBytesAsync(tempWavPath, resampledData, cancellationToken).ConfigureAwait(false);

                await using var processor = _factory.CreateBuilder()
                    .WithLanguage("en")
                    .Build();

                await using var fileStream = File.OpenRead(tempWavPath);

                var resultBuilder = new StringBuilder();

                await foreach (var segment in processor.ProcessAsync(fileStream, cancellationToken).ConfigureAwait(false))
                {
                    resultBuilder.Append(segment.Text);
                }

                return resultBuilder.ToString().Trim();
            }
            finally
            {
                try { File.Delete(tempWavPath); } catch { /* best effort cleanup */ }
            }
        }

        private async Task EnsureModelLoadedAsync(CancellationToken cancellationToken)
        {
            if (_factory is not null)
                return;

            await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_factory is not null)
                    return;

                if (!File.Exists(ModelPath))
                {
                    Directory.CreateDirectory(ModelDirectory);

                    // Download the tiny model (~75 MB) on first use
                    using var httpClient = new HttpClient();
                    var downloader = new WhisperGgmlDownloader(httpClient);
                    using var modelStream = await downloader.GetGgmlModelAsync(
                        GgmlType.Tiny, cancellationToken: cancellationToken).ConfigureAwait(false);

                    await using var fileStream = File.Create(ModelPath);
                    await modelStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
                }

                _factory = WhisperFactory.FromPath(ModelPath);
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <summary>
        /// Resamples a WAV byte array to 16KHz mono PCM16, the format Whisper.net requires.
        /// If the audio is already 16KHz mono, it is returned as-is.
        /// </summary>
        private static byte[] ResampleWavTo16KhzMono(byte[] wavData)
        {
            const int targetSampleRate = 16_000;
            const int targetChannels = 1;
            const int targetBitsPerSample = 16;

            if (wavData.Length < 44)
                return wavData; // too small to be a valid WAV

            // Validate RIFF/WAVE header
            if (Encoding.ASCII.GetString(wavData, 0, 4) != "RIFF" ||
                Encoding.ASCII.GetString(wavData, 8, 4) != "WAVE")
            {
                throw new InvalidOperationException("Audio data is not in WAV format.");
            }

            // Walk sub-chunks to find "fmt " and "data"
            int srcChannels = 0, srcSampleRate = 0, srcBitsPerSample = 0;
            int dataOffset = 0, dataSize = 0;
            bool foundFmt = false;

            int offset = 12; // past "RIFF" + size + "WAVE"
            while (offset + 8 <= wavData.Length)
            {
                var chunkId = Encoding.ASCII.GetString(wavData, offset, 4);
                int chunkSize = BinaryPrimitives.ReadInt32LittleEndian(wavData.AsSpan(offset + 4, 4));

                if (chunkId == "fmt " && chunkSize >= 16)
                {
                    int fmtStart = offset + 8;
                    srcChannels = BinaryPrimitives.ReadInt16LittleEndian(wavData.AsSpan(fmtStart + 2, 2));
                    srcSampleRate = BinaryPrimitives.ReadInt32LittleEndian(wavData.AsSpan(fmtStart + 4, 4));
                    srcBitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(wavData.AsSpan(fmtStart + 14, 2));
                    foundFmt = true;
                }
                else if (chunkId == "data")
                {
                    dataOffset = offset + 8;
                    dataSize = Math.Min(chunkSize, wavData.Length - dataOffset);
                }

                // Both found — stop scanning
                if (foundFmt && dataSize > 0)
                    break;

                offset += 8 + chunkSize;
                // WAV chunks are word-aligned
                if (chunkSize % 2 != 0)
                    offset++;
            }

            if (!foundFmt || dataSize == 0)
                throw new InvalidOperationException("Could not locate fmt/data chunks in WAV.");

            if (srcSampleRate == targetSampleRate && srcChannels == targetChannels && srcBitsPerSample == targetBitsPerSample)
                return wavData; // already in the correct format

            // Guard against invalid or unsupported WAV headers
            if (srcChannels <= 0 || srcSampleRate <= 0 || srcBitsPerSample is not (16 or 24 or 32))
                throw new InvalidOperationException(
                    $"Unsupported WAV format: {srcChannels} channels, {srcSampleRate} Hz, {srcBitsPerSample}-bit. " +
                    "Expected PCM WAV with 16, 24, or 32 bits per sample.");

            int srcBytesPerSample = srcBitsPerSample / 8;
            int srcFrameSize = srcChannels * srcBytesPerSample;
            int srcFrameCount = dataSize / srcFrameSize;

            // Decode source PCM samples to float mono
            var srcMono = new float[srcFrameCount];
            for (int i = 0; i < srcFrameCount; i++)
            {
                int frameStart = dataOffset + i * srcFrameSize;
                float sum = 0;
                for (int ch = 0; ch < srcChannels; ch++)
                {
                    int sampleOffset = frameStart + ch * srcBytesPerSample;
                    float sample = srcBitsPerSample switch
                    {
                        16 => BinaryPrimitives.ReadInt16LittleEndian(wavData.AsSpan(sampleOffset, 2)) / 32768f,
                        24 => (wavData[sampleOffset] | (wavData[sampleOffset + 1] << 8) | ((sbyte)wavData[sampleOffset + 2] << 16)) / 8388608f,
                        32 => BinaryPrimitives.ReadInt32LittleEndian(wavData.AsSpan(sampleOffset, 4)) / 2147483648f,
                        _ => 0f
                    };
                    sum += sample;
                }
                srcMono[i] = sum / srcChannels;
            }

            // Resample using linear interpolation
            double ratio = (double)srcSampleRate / targetSampleRate;
            int dstFrameCount = (int)(srcFrameCount / ratio);
            var dstSamples = new short[dstFrameCount];

            for (int i = 0; i < dstFrameCount; i++)
            {
                double srcIndex = i * ratio;
                int idx0 = (int)srcIndex;
                int idx1 = Math.Min(idx0 + 1, srcFrameCount - 1);
                float frac = (float)(srcIndex - idx0);
                float value = srcMono[idx0] + (srcMono[idx1] - srcMono[idx0]) * frac;
                dstSamples[i] = (short)Math.Clamp(value * 32767f, short.MinValue, short.MaxValue);
            }

            // Build the output WAV
            int dstDataSize = dstFrameCount * targetChannels * (targetBitsPerSample / 8);
            using var output = new MemoryStream(44 + dstDataSize);
            using var writer = new BinaryWriter(output);

            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dstDataSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16); // sub-chunk size
            writer.Write((short)1); // PCM
            writer.Write((short)targetChannels);
            writer.Write(targetSampleRate);
            writer.Write(targetSampleRate * targetChannels * (targetBitsPerSample / 8)); // byte rate
            writer.Write((short)(targetChannels * (targetBitsPerSample / 8))); // block align
            writer.Write((short)targetBitsPerSample);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dstDataSize);

            foreach (short sample in dstSamples)
            {
                writer.Write(sample);
            }

            return output.ToArray();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _factory?.Dispose();
            _initLock.Dispose();
        }
    }
}
