using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CliHelper;
using System.Linq;

namespace PiConsoleTest
{
    [TestClass]
    public class OptionParserTest
    {
        [TestMethod]
        public void SingleSimpleShortOption()
        {
            Option[] options = new Option[]
            {
                new Option("a", null, "TestA", "Nothing", false)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "-a";

            var result = parser.ParseAll(testLine);

            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().Option == options[0]);
        }

        [TestMethod]
        public void SingleSimpleLongOption()
        {
            Option[] options = new Option[]
            {
                new Option("a", "longoption", "TestA", "Nothing", false)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "--longoption";

            var result = parser.ParseAll(testLine);

            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().Option == options[0]);
        }

        [TestMethod]
        public void SingleShortOption()
        {
            Option[] options = new Option[]
            {
                new Option("a", null, "TestA", "Nothing", true)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "-a 'test'";

            var result = parser.ParseAll(testLine);

            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().Option == options[0]);
            Assert.AreEqual("test", result.First().Argument);
        }

        [TestMethod]
        public void SingleLongOption()
        {
            Option[] options = new Option[]
            {
                new Option("a", "longoption", "TestA", "Nothing", true)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "--longoption 'test'";

            var result = parser.ParseAll(testLine);

            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().Option == options[0]);
            Assert.AreEqual("test", result.First().Argument);
        }


        [TestMethod]
        public void MultipleSimpleShortOptions()
        {
            Option[] options = new Option[]
            {
                new Option("a", null, "TestA", "Nothing"),
                new Option("b", null, "TestB", "Nothing")
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "-a -b";

            var result = parser.ParseAll(testLine).ToArray();

            Assert.IsTrue(result.Count() == 2);
            Assert.AreEqual(options[0], result[0].Option);
            Assert.AreEqual(options[1], result[1].Option);
        }

        [TestMethod]
        public void MultipleSimpleLongOptions()
        {
            Option[] options = new Option[]
            {
                new Option("a", "longoptiona", "TestA", "Nothing"),
                new Option("b", "longoptionb", "TestB", "Nothing")
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "--longoptiona --longoptionb";

            var result = parser.ParseAll(testLine).ToArray();

            Assert.IsTrue(result.Count() == 2);
            Assert.AreEqual(options[0], result[0].Option);
            Assert.AreEqual(options[1], result[1].Option);
        }

        [TestMethod]
        public void MultipleShortOptions()
        {
            Option[] options = new Option[]
            {
                new Option("a", null, "TestA", "Nothing", true),
                new Option("b", null, "TestB", "Nothing", true)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "-a 'testa' -b testb";

            var result = parser.ParseAll(testLine).ToArray();

            Assert.IsTrue(result.Count() == 2);

            Assert.AreEqual(options[0], result[0].Option);
            Assert.AreEqual(options[1], result[1].Option);

            Assert.AreEqual("testa", result[0].Argument);
            Assert.AreEqual("testb", result[1].Argument);
        }

        [TestMethod]
        public void MultipleLongOptions()
        {
            Option[] options = new Option[]
            {
                new Option("a", "longa", "TestA", "Nothing", true),
                new Option("b", "longb", "TestB", "Nothing", true)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "--longa 'testa' --longb testb";

            var result = parser.ParseAll(testLine).ToArray();

            Assert.IsTrue(result.Count() == 2);

            Assert.AreEqual(options[0], result[0].Option);
            Assert.AreEqual(options[1], result[1].Option);

            Assert.AreEqual("testa", result[0].Argument);
            Assert.AreEqual("testb", result[1].Argument);
        }


        [TestMethod]
        public void OptionWithoutArgumentShouldFail()
        {
            Option[] options = new Option[]
            {
                new Option("a", null, "TestA", "Nothing", true)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "-a";

            Assert.ThrowsException<ParserException>(() => parser.ParseAll(testLine).ToList());
        }

        [TestMethod]
        public void OptionWithArgumentShouldNotThrow()
        {
            Option[] options = new Option[]
            {
                new Option("a", null, "TestA", "Nothing", false),
                new Option("b", null, "TestB", "Nothing", false)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "-a argument -b";

            parser.ParseAll(testLine).ToList();
        }

        [TestMethod]
        public void OptionWithArgumentAtEndShouldNotThrow()
        {
            Option[] options = new Option[]
            {
                new Option("a", null, "TestA", "Nothing", false)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "-a argument";

            parser.ParseAll(testLine).ToList();
        }


        [TestMethod]
        public void MultipleOptionShouldThrow()
        {
            Option[] options = new Option[]
            {
                new Option("a", null, "TestA", "Nothing", false, multipleTimes:false)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));

            string testLine = "-a -a";
            
            Assert.ThrowsException<ParserException>(() => parser.ParseAll(testLine).ToList());
        }

        [TestMethod]
        public void ArgumentBetweenOptionsShouldThrow()
        {
            Option[] options = new Option[]
            {
                new Option("a", null, "TestA", "Nothing", false)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options)
            {
                AllowArgumentsBetweenOptions = false
            });

            string testLine = "-a hola -a";

            Assert.ThrowsException<ParserException>(() => parser.ParseAll(testLine).ToList());
        }

        [TestMethod]
        public void DuplicateOptionDefinitionShouldThrow()
        {
            Option optA =  new Option("a", null, "a", "");
            Option optAA = new Option("a", "oi", "rth", "");
            Option optB =  new Option("b", "dup", "b", "");
            Option optC =  new Option("c", "dup", "c", "");
            Option optD =  new Option("d", null, "dup", "");
            Option optE =  new Option("e", null, "dup", "");

            Assert.ThrowsException<OptionException>(() => Create(optA, optAA));
            Assert.ThrowsException<OptionException>(() => Create(optB, optC));
            Assert.ThrowsException<OptionException>(() => Create(optD, optE));

            void Create(params Option[] opts)
            {
                new OptionParser(new OptionParser.Configuration(opts));
            }
        }
    }
}
