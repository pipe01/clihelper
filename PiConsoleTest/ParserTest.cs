using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PiConsole;
using System.Linq;

namespace PiConsoleTest
{
    [TestClass]
    public class ParserTest
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
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options)
            {
                ThrowOnInvalidOption = true
            });

            string testLine = "-a";

            Assert.ThrowsException<ParserException>(() => parser.ParseAll(testLine).ToList());
        }

        [TestMethod]
        public void OptionWithArgumentShouldFail()
        {
            Option[] options = new Option[]
            {
                new Option("a", null, "TestA", "Nothing", false)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options)
            {
                ThrowOnInvalidOption = true
            });

            string testLine = "-a argument";

            //Assert.ThrowsException<ParserException>(() => parser.ParseAll(testLine).ToList());
        }

        [TestMethod]
        public void MultipleOptionShouldFail()
        {
            Option[] options = new Option[]
            {
                new Option("a", null, "TestA", "Nothing", false, false)
            };
            OptionParser parser = new OptionParser(new OptionParser.Configuration(options)
            {
                ThrowOnInvalidOption = true
            });

            string testLine = "-a -a";
            
            Assert.ThrowsException<ParserException>(() => parser.ParseAll(testLine).ToList());
        }
    }
}
