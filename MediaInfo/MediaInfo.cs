using System;
using System.Runtime.InteropServices;

namespace MediaInfoLib
{
    public class MediaInfo
    {
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_New();
        [DllImport("MediaInfo.dll")]
        private static extern void MediaInfo_Delete(IntPtr handle);
        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr MediaInfo_Option(IntPtr handle, string parameter, string value);
        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr MediaInfo_Open(IntPtr handle, string filename);
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Inform(IntPtr handle, IntPtr reserved);

        private readonly IntPtr handle;

        public MediaInfo()
        {
            handle = MediaInfo_New();
        }

        public string? Option(string parameter, string value)
        {
            return Marshal.PtrToStringUni(MediaInfo_Option(handle, parameter, value));
        }

        // Returns true if opened successfully
        public bool Open(string filename)
        {
            return (int)MediaInfo_Open(handle, filename) == 1;
        }

        public string? Inform()
        {
            return Marshal.PtrToStringUni(MediaInfo_Inform(handle, (IntPtr)0));
        }

        ~MediaInfo ()
        {
            MediaInfo_Delete(handle);
        }
    }
}