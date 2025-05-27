using System;

namespace DreamKeeper.Services
{
    public static class AudioFileHelper
    {
        /// <summary>
        /// Gets the appropriate audio file extension based on the current device platform
        /// </summary>
        /// <returns>The file extension including the dot (e.g., ".m4a")</returns>
        public static string GetAudioFileExtension()
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return ".m4a";
            }
            else if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                return ".mp4";
            }
            else
            {
                return ".mp3"; // Default fallback format for other platforms
            }
        }

        /// <summary>
        /// Gets the appropriate MIME type for the current platform's audio format
        /// </summary>
        /// <returns>The MIME type string</returns>
        public static string GetAudioMimeType()
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return "audio/mp4a-latm";
            }
            else if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                return "audio/mp4";
            }
            else
            {
                return "audio/mpeg";
            }
        }

        /// <summary>
        /// Creates a unique filename for temporary audio storage
        /// </summary>
        /// <returns>A unique filename with appropriate extension</returns>
        public static string GetUniqueAudioFilename()
        {
            return $"recording_{Guid.NewGuid()}{GetAudioFileExtension()}";
        }
    }
}