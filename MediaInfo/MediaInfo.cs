using System.Runtime.InteropServices;

namespace MediaInfoLib;

public partial class MediaInfo
{
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

    private readonly IntPtr handle = MediaInfo_New();

    public string? Option(string parameter, string value)
    {
        return Marshal.PtrToStringUni(MediaInfo_Option(handle, parameter, value));
    }

    // Returns true if opened successfully
    public bool Open(string filename)
    {
        return MediaInfo_Open(handle, filename) != 0;
    }

    public string? Inform()
    {
        return Marshal.PtrToStringUni(MediaInfo_Inform(handle, 0));
    }

    ~MediaInfo()
    {
        MediaInfo_Delete(handle);
    }
}
