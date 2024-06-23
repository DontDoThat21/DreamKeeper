using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamKeeper.Data
{
    public class ByteArrayMediaElement : MediaElement
    {
        public static readonly BindableProperty AudioDataProperty = BindableProperty.Create(
            nameof(AudioData), typeof(byte[]), typeof(ByteArrayMediaElement), default(byte[]), propertyChanged: OnAudioDataChanged);

        public byte[] AudioData
        {
            get => (byte[])GetValue(AudioDataProperty);
            set => SetValue(AudioDataProperty, value);
        }

        private static void OnAudioDataChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ByteArrayMediaElement)bindable;
            control.UpdateMediaSource();
        }

        private void UpdateMediaSource()
        {
            if (AudioData != null && AudioData.Length > 0)
            {
                var tempFilePath = Path.Combine(FileSystem.CacheDirectory, "tempRecording.m4a");
                // Write byte array to a temporary file
                File.WriteAllBytes(tempFilePath, AudioData);
                // Set the source to the temporary file
                this.Source = MediaSource.FromFile(tempFilePath);
            }
        }
    }
}
