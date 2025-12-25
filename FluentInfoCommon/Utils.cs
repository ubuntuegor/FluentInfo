namespace FluentInfoCommon;

public static class Utils
{
    private static readonly string[] NewLineSeparator = ["\r\n", "\r", "\n"];

    public static string[] SplitToLines(string value, StringSplitOptions options = StringSplitOptions.None)
    {
        return value.Split(NewLineSeparator, options);
    }
}