using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliHelper
{
    internal static class Lexer
    {
        public static IEnumerable<Token> Parse(string line)
        {
            line = line.Trim();

            for (int i = 0; i < line.Length; i++)
            {
                char currentChar = line[i];
                char nextChar = '\0';

                //Get the next char, if possible
                if (i < line.Length - 1)
                    nextChar = line[i + 1];
                
                //If we are starting an option
                if (currentChar == '-')
                {
                    //Check if it's a long option (--xyz)
                    bool longOption = nextChar == '-';

                    //Get the option string without the dashes
                    int afterOptionIndex = i + 1 + (longOption ? 1 : 0);
                    string optionText = line.Substring(afterOptionIndex).ReadUntilChars(' ');

                    //Skip to after the option
                    i = afterOptionIndex + optionText.Length;

                    //Create and return token
                    OptionToken token = new OptionToken(optionText, longOption);
                    
                    yield return token;
                }
                //If we are starting a string
                else
                {
                    //Get the complete argument string
                    string text = line.Substring(i).ReadUntilChars(out int length, ' ');

                    //Skip index
                    i += length;

                    //Return a generic argument
                    yield return new ArgumentToken(text);
                }
            }
        }
    }
}
