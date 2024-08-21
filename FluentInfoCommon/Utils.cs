using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInfoCommon
{
    public class Utils
    {
        private Utils() { }

        private static readonly string[] newLineSeparator = ["\r\n", "\r", "\n"];

        public static string[] SplitToLines(string value, StringSplitOptions options = StringSplitOptions.None)
        {
            return value.Split(newLineSeparator, options);
        }
    }
}
