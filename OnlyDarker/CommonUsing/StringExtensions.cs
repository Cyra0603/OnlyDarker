using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.CommonUsing
{
    public static class StringExtensions
    {
        public static string SetLineBreaks(this string stringToBreak, int lineLength)
        {
            ArgumentNullException.ThrowIfNull(stringToBreak);
            if (lineLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(lineLength), "Line length should be more than 0.");

            int length = stringToBreak.Length;
            int resultLength = length + (length / lineLength);
            Span<char> result = stackalloc char[resultLength];
            int index = 0;
            int resultIndex = 0;
            foreach (char c in stringToBreak)
            {
                result[resultIndex++] = c;
                index++;
                if (index >= lineLength)
                {
                    result[resultIndex++] = '\n';
                    index = 0;
                }
            }

            return new string(result[..resultIndex]);
        }
        public static string SetLineBreaks(this string stringToBreak, int lineLength, out int lines)
        {
            ArgumentNullException.ThrowIfNull(stringToBreak);
            if (lineLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(lineLength), "Line length should be more than 0.");

            lines = 1;
            int length = stringToBreak.Length;
            int resultLength = length + (length / lineLength);
            Span<char> result = stackalloc char[resultLength];
            int index = 0;
            int resultIndex = 0;
            foreach (char c in stringToBreak)
            {
                result[resultIndex++] = c;
                index++;
                if (index >= lineLength)
                {
                    result[resultIndex++] = '\n';
                    index = 0;
                    lines++;
                }
            }
            return new string(result[..resultIndex]);
        }
    }
}
