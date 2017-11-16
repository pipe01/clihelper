using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("PiConsoleTest")]
[assembly: InternalsVisibleTo("Testing")]

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

            List<Token> tokens = Lexer.Parse(line.Substring(start)).ToList();
            OptionToken? lastOption = (OptionToken?)tokens
                .Where(o => o is OptionToken)
                .LastOrDefault();

            for (int i = 0; i < tokens.Count; i++)
            {
                Token currentToken = tokens[i];
                Token nextToken = null;

                //Try to get next token
                if (i < tokens.Count - 1)
                    nextToken = tokens[i + 1];

                //We are on an option token
                if (currentToken is OptionToken optionToken)
                {
                    //Check if there is an argument
                    ArgumentToken? argToken =
                        nextToken is ArgumentToken ? (ArgumentToken?)nextToken : null;

                    //If there is an argument token behind, skip it
                    if (argToken != null)
                        i++;

                    Option optionDefinition = null;

                    //Get the option definition
                    if (optionToken.Long)
                    {
                        optionDefinition = _Configuration.ValidOptionDefinitions
                            .Where(o => o.LongOption.Equals(optionToken.Option))
                            .SingleOrDefault();
                    }
                    else
                    {
                        optionDefinition = _Configuration.ValidOptionDefinitions
                            .Where(o => o.ShortOption.Equals(optionToken.Option))
                            .SingleOrDefault();
                    }


                    //If the token isn't on the provided definitions list, throw
                    if (optionDefinition == null)
                        throw new ParserException($"Token '{optionToken.Option}' not found");


                    bool isLastOption = lastOption.HasValue && optionToken.Equals(lastOption.Value);
                    bool hasArgumentToken = argToken != null;
                    bool reqsArgument = optionDefinition.HasArgument;
                    

                    //This option has already been specified but it can only appear once, throw
                    if (appearances.Contains(optionDefinition) && !optionDefinition.CanAppearMultipleTimes)
                        throw new ParserException($"Token '{optionToken.Option}' can only appear once.");

                    //We have an argument but we don't require it, and this isn't the last option, throw
                    if ((!reqsArgument && hasArgumentToken) && !isLastOption)
                        throw new ParserException($"Token '{optionToken.Option}' can't have an argument.");

                    //We don't have an argument but we require it, throw
                    if (!hasArgumentToken && reqsArgument)
                        throw new ParserException($"Token '{optionToken.Option}' must have an argument.");


                    //Add option to the appearances list
                    appearances.Add(optionDefinition);

                    yield return new OptionValue(optionDefinition, argToken?.Text);
                }
                else if (currentToken is ArgumentToken argToken)
                {
                    yield return new OptionValue(null, argToken.Text);
                }
            }
        }
    }
}
