using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliHelper
{
    internal static class StringUtils
    {
        /// <summary>
        /// Reads a string until a specified unescaped character is found.
        /// </summary>
        public static string ReadUntilChars(this string str, params char[] ch)
        {
            return str.ReadUntilChars(out int a, ch);
        }

        /// <summary>
        /// Reads a string until a specified unescaped character is found.
        /// </summary>
        public static string ReadUntilChars(this string str, out int totalLength, params char[] ch)
        {
            string ret = "";
            char quotes = '\0';

            totalLength = 0;

            for (int i = 0; i < str.Length; i++)
            {
                char currentChar = str[i];
                char lastChar = i > 0 ? str[i - 1] : '\0';

                if (currentChar == '"' || currentChar == '\'')
                {
                    totalLength++;

                    if (quotes == '\0')
                    {
                        quotes = currentChar;
                    }
                    else
                    {
                        quotes = '\0';
                        break;
                    }

                    continue;
                }

                if (ch.Contains(currentChar) && lastChar != '\\' && quotes == '\0')
                {
                    break;
                }

                ret += currentChar;
                totalLength++;
            }

            return ret;
        }
    }
}
