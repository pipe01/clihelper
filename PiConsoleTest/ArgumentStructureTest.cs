using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PiConsole;

namespace PiConsoleTest
{
    [TestClass]
    public class ArgumentStructureTest
    {
        class TestClass1 : ArgumentStructure
        {
            [Option("testString")]
            public string TestString { get; set; }

            [Option("testBool")]
            public bool TestBool { get; set; }
        }

        class TestClass2 : ArgumentStructure
        {
            [Option("a", "testString", "testString", "erhuithi", true)]
            public string TestString { get; set; }

            [Option("b", "testBool", "testBool", "erogirtjh")]
            public bool TestBool { get; set; }
        }


        [TestMethod]
        public void ReflectionStringExplicitOptions()
        {
            string testString = "--testString 'hola' --testBool";

            TestClass1 parsed = ArgumentStructure.Parse<TestClass1>(testString, new Option[]
            {
                new Option("a", "testString", "testString", "hoijo", true),
                new Option("b", "testBool", "testBool", "edhroirtjho")
            });

            Assert.AreEqual("hola", parsed.TestString);
            Assert.IsTrue(parsed.TestBool);
        }

        [TestMethod]
        public void ReflectionStringImplicitOptions()
        {
            string testString = "--testString 'hola' --testBool";

            TestClass2 parsed = ArgumentStructure.Parse<TestClass2>(testString);

            Assert.AreEqual("hola", parsed.TestString);
            Assert.IsTrue(parsed.TestBool);
        }


        [TestMethod]
        public void ReflectionArrayExplicitOptions()
        {
            string[] testArray = "--testString 'hola' --testBool".Split(' ');

            TestClass1 parsed = ArgumentStructure.Parse<TestClass1>(testArray, new Option[]
            {
                new Option("a", "testString", "testString", "hoijo", true),
                new Option("b", "testBool", "testBool", "edhroirtjho")
            });

            Assert.AreEqual("hola", parsed.TestString);
            Assert.IsTrue(parsed.TestBool);
        }

        [TestMethod]
        public void ReflectionArrayImplicitOptions()
        {
            string[] testArray = "--testString 'hola' --testBool".Split(' ');

            TestClass2 parsed = ArgumentStructure.Parse<TestClass2>(testArray);

            Assert.AreEqual("hola", parsed.TestString);
            Assert.IsTrue(parsed.TestBool);
        }

        
        [TestMethod]
        public void DictionaryString()
        {
            string testString = "--testString 'hola' --testBool";

            var options = new Option[]
            {
                new Option("a", "testString", "testString", "hoijo", true),
                new Option("b", "testBool", "testBool", "edhroirtjho")
            };

            var parsed = ArgumentStructure.Parse(testString, options);

            //Assert.AreEqual("hola", parsed[options[0]]);
            //Assert.IsTrue(parsed.ContainsKey(options[1]));
        }

        [TestMethod]
        public void DictionaryArray()
        {
            string[] testArray = "--testString 'hola' --testBool".Split(' ');

            var options = new Option[]
            {
                new Option("a", "testString", "testString", "hoijo", true),
                new Option("b", "testBool", "testBool", "edhroirtjho")
            };

            var parsed = ArgumentStructure.Parse(testArray, options);

            //Assert.AreEqual("hola", parsed[options[0]]);
            //Assert.IsTrue(parsed.ContainsKey(options[1]));
        }

        //TODO Add tests for command arguments
    }
}
