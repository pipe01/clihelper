using CliHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    class Program
    {
        class MyArgs : ArgumentStructure
        {
            [Option("h", "help", "help", "Shows help", false, multipleTimes: false)]
            public bool ShowHelp { get; set; }

            [Option("e", "extract", "extract", "Extracts a file", true)]
            public string ExtractFile { get; set; }

            public override Configuration CommandConfiguration => new Configuration
            {
                Arguments = new[]
                {
                    new ArgumentDefinition("file1", false),
                    new ArgumentDefinition("file2", true)
                }
            };
        }

        static void Main(string[] args)
        {
            /*OptionParser parser = new OptionParser(new OptionParser.Configuration
            {
                AllowArgumentsBetweenOptions = true,
                OptionDefinitions = new Option[]
                {
                    new Option("h", "help", "Help", "Shows help", false, false),
                    new Option("o", "output", "Output", "Show output", false),
                    new Option("e", "extract", "Extract", "Extracts a file", true)
                }
            });

            var result = parser.ParseAll("-h 'argumento lol' -e hola").ToList();*/
            
            var margs = ArgumentStructure.Parse<MyArgs>("-h argumento lol");

            if (margs.ShowHelp)
                Console.WriteLine(ArgumentStructure.GetCommandUsage<MyArgs>());

            //var res = Lexer.Parse("-e lol xd --hola").ToList();

            Console.WriteLine("Done");
            Console.ReadLine();


        }
    }
}
