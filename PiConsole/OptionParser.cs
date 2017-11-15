using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("PiConsoleTest")]

namespace PiConsole
{
    public class ParserException : Exception
    {
        public ParserException(string message)
            : base(message)
        {
            
        }
    }
    
    internal class OptionParser
    {
        /// <summary>
        /// Parser configuration.
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// Whether to throw an exception when an invalid option is found. Defaults to false.
            /// </summary>
            public bool ThrowOnInvalidOption { get; set; } = false;

            /// <summary>
            /// List of the valid option definitions.
            /// </summary>
            public IEnumerable<Option> ValidOptionDefinitions { get; set; } = new List<Option>();

            public Configuration() { }
            public Configuration(IEnumerable<Option> optionDefinitions)
            {
                this.ValidOptionDefinitions = optionDefinitions;
            }
        }

        /// <summary>
        /// Pair of option definition and argument
        /// </summary>
        public struct OptionValue
        {
            public Option Option;
            public string Argument;

            public static OptionValue Empty => new OptionValue(null, null);

            public OptionValue(Option Option, string Argument)
            {
                this.Option = Option;
                this.Argument = Argument;
            }
        }

        private Configuration _Configuration;

        public OptionParser(Configuration config)
        {
            _Configuration = config;
        }

        /// <summary>
        /// Parse the first option from the line string.
        /// </summary>
        /// <param name="line">String containing the options.</param>
        /// <param name="start">Char index at which to start. Defaults to 0.</param>
        public IEnumerable<OptionValue> ParseAll(string line, int start = 0)
        {
            line = line.Substring(start);

            //Options that have already been seen will go here.
            List<Option> appearances = new List<Option>();

            for (int i = 0; i < line.Length; i++)
            {
                char currentChar = line[i];
                char nextChar = i < line.Length - 1 ? line[i + 1] : '\0';

                //Extract option chunk

                //We are starting an option
                if (currentChar == '-')
                {
                    //Check if it's a long or short option
                    bool longOpt = nextChar == '-';

                    //Get the full option definition
                    string optionDef = ReadUntilChar(line.Substring(i), ' ');

                    //Skip option characters
                    i += optionDef.Length;

                    //Strip the dashes out
                    optionDef = optionDef.TrimStart('-');

                    //Get the option argument, if any
                    string optionArgs = GetOptionArgument(ReadUntilChar(line.Substring(i), '-'), out int argLength);


                    //Find the option in the option definitions
                    Option optionMatch = null;
                    if (longOpt)
                    {
                        optionMatch = _Configuration.ValidOptionDefinitions
                            .FirstOrDefault(o => o.LongOption.Equals(optionDef));
                    }
                    else
                    {
                        optionMatch = _Configuration.ValidOptionDefinitions
                            .FirstOrDefault(o => o.ShortOption.Equals(optionDef));
                    }

                    //Check if we found an option
                    if (optionMatch == null)
                    {
                        //The option isn't on the valid option collection, throw brick

                        if (_Configuration.ThrowOnInvalidOption)
                            throw new ParserException($"Can't find option '{optionDef}'.");
                        else
                            continue;
                    }
                    else if ((optionMatch.HasArgument && optionArgs == null) || (!optionMatch.HasArgument && optionArgs != null))
                    {
                        //If we require arguments and we don't have them, or if we don't require arguments and
                        //we do have them, the option is invalid

                        string errorMsg = $"Argument mismatch at option '{optionDef}', column index {i}: ";

                        if (optionMatch.HasArgument)
                            errorMsg += "argument required.";
                        else
                            errorMsg += "argument not required.";

                        if (_Configuration.ThrowOnInvalidOption)
                            throw new ParserException(errorMsg);
                        else
                            continue;
                    }
                    else if (appearances.Contains(optionMatch) && !optionMatch.CanAppearMultipleTimes)
                    {
                        //This option can only appear once, but it has appeared more than once

                        if (_Configuration.ThrowOnInvalidOption)
                            throw new ParserException($"Argument '{optionDef}' can only appear once.");
                        else
                            continue;
                    }

                    i += argLength;

                    //Add option to seen option list
                    appearances.Add(optionMatch);

                    //We have successfully checked the option
                    //Create and return it

                    OptionValue opt = new OptionValue(optionMatch, optionArgs);

                    yield return opt;
                }
            }
        }
        
        /// <summary>
        /// Tries to get the option's argument. If none, returns null.
        /// </summary>
        /// <param name="totalLength">The absolute length on <paramref name="str"/> of the argument.</param>
        private string GetOptionArgument(string str, out int totalLength)
        {
            str = str.Trim();

            //There is no argument, return null.
            if (str.Length == 0)
            {
                totalLength = -1;
                return null;
            }

            string ret = "";

            char[] stringDelimiters = new[] { '"', '\'' };
            char currentDelimiter = '\0';
            int length = -1;

            for (int i = 0; i < str.Length; i++)
            {
                char currentChar = str[i];
                
                bool isDelimiter = stringDelimiters.Contains(currentChar);

                //Check if we are on a string delimiter
                if (isDelimiter && currentDelimiter == '\0')
                {
                   //We are starting a string, set the current delimiter
                   currentDelimiter = currentChar;
                }
                else if (isDelimiter && currentDelimiter != '\0' && currentChar == currentDelimiter)
                {
                    //We are on a string and the current char is the string delimiter we are currently using
                    //End the string

                    length = i + 1;
                    break;
                }
                else
                {
                    //It's a regular character, add it to the argument
                    ret += currentChar;
                }
            }

            totalLength = length;
            return ret;
        }

        /// <summary>
        /// Reads a string until a certain unescaped character is found.
        /// </summary>
        private string ReadUntilChar(string str, char ch)
        {
            string ret = "";

            for (int i = 0; i < str.Length; i++)
            {
                char currentChar = str[i];
                char lastChar = i > 0 ? str[i - 1] : '\0';

                if (currentChar == ch && lastChar != '\\')
                {
                    break;
                }

                ret += currentChar;
            }

            return ret;
        }
    }
}
