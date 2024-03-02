using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInfo
{
    internal class Utils
    {
        private Utils() { }

        public static string[] SplitToLines(string value, StringSplitOptions options = StringSplitOptions.None)
        {
            return value.Split(new[] { "\r\n", "\r", "\n" }, options);
        }
    }
}
