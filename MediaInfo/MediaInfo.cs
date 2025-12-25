using System.Runtime.InteropServices;

namespace MediaInfoLib;

public partial class MediaInfo
{
    private readonly IntPtr _handle = MediaInfo_New();

    [LibraryImport("MediaInfo.dll")]
    private static partial IntPtr MediaInfo_New();

    [LibraryImport("MediaInfo.dll")]
    private static partial void MediaInfo_Delete(IntPtr handle);

    [LibraryImport("MediaInfo.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr MediaInfo_Option(IntPtr handle, string parameter, string value);

    [LibraryImport("MediaInfo.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr MediaInfo_Open(IntPtr handle, string filename);

    [LibraryImport("MediaInfo.dll")]
    private static partial IntPtr MediaInfo_Inform(IntPtr handle, IntPtr reserved);

    public string? Option(string parameter, string value)
    {
        return Marshal.PtrToStringUni(MediaInfo_Option(_handle, parameter, value));
    }

    // Returns true if opened successfully
    public bool Open(string filename)
    {
        return MediaInfo_Open(_handle, filename) != 0;
    }

    public string? Inform()
    {
        return Marshal.PtrToStringUni(MediaInfo_Inform(_handle, 0));
    }

    ~MediaInfo()
    {
        MediaInfo_Delete(_handle);
    }
}