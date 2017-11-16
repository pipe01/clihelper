using PiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    class Program
    {
        class MyArgs : Arguments
        {
            [Option("h", "help", "help", "Shows help", false, false)]
            public bool ShowHelp { get; set; }

            [Option("e", "extract", "extract", "Extracts a file", true)]
            public string ExtractFile { get; set; }
        }

        static void Main(string[] args)
        {
            OptionParser parser = new OptionParser(new OptionParser.Configuration
            {
                ThrowOnInvalidOption = true,
                ValidOptionDefinitions = new Option[]
                {
                    new Option("h", "help", "Help", "Shows help", false, false),
                    new Option("o", "output", "Output", "Show output", false),
                    new Option("e", "extract", "Extract", "Extracts a file", true)
                }
            });

            var result = parser.ParseAll("-e \"asdf lol\" hola").ToList();

            /*Arguments.ThrowOnOptionError = true;
            try
            {
                var margs = Arguments.Parse<MyArgs>("-h hoi");
            }
            catch (ParserException ez)
            {
                Console.WriteLine("Error while parsing: " + ez.Message);
            }*/
            
            Console.WriteLine("Done");
            Console.ReadLine();


        }
    }
}
