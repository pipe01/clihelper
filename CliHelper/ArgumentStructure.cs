using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.ObjectModel;

namespace CliHelper
{
    public abstract class ArgumentStructure
    {
        public struct ArgumentDefinition
        {
            public string Name;
            public bool Optional;

            public ArgumentDefinition(string name, bool optional)
            {
                this.Name = name;
                this.Optional = optional;
            }
        }

        public class Configuration
        {
            /// <summary>
            /// The list of arguments that don't belong to an option. E.g., "extract.exe --optionWithoutArgs [one of these arguments] [another one]".
            /// </summary>
            public IList<ArgumentDefinition> Arguments { get; set; } = new List<ArgumentDefinition>();
        }

        #region Static
        private static bool ContainsKey<TKey, TValue>(List<KeyValuePair<TKey, TValue>> arr, TKey key)
        {
            return arr.Any(o => o.Key.Equals(key));
        }

        /// <summary>
        /// Sets an object's properties. <para/>
        /// If any value in the list is null and the property is boolean,
        /// it will be set to false. If any property is boolean but the value isn't null, it will
        /// be set to true. If any property is string and the value is null, an exception will be thrown.
        /// If any property's value isn't string or boolean, an exception will be thrown.
        /// </summary>
        /// <param name="obj">The object to modify.</param>
        /// <param name="fields">The properties.</param>
        /// <exception cref="ArgumentException"></exception>
        private static void ApplyReflection(object obj, List<KeyValuePair<string, string>> fields)
        {
            Contract.Requires(obj != null);
            Contract.Requires(fields != null);

            Type type = obj.GetType();

            foreach (var prop in type.GetProperties())
            {
                //Try to get OptionAttribute
                var optAttribute = prop.GetCustomAttribute<OptionAttribute>();
                
                //If it isn't found, skip
                //TODO Maybe require all properties in the class to be Options
                if (optAttribute == null)
                    continue;

                //The property is read-only if the setter function doesn't exist or it's private
                bool isReadOnly = prop.SetMethod == null || !prop.SetMethod.IsPublic;

                //All option properties must be writeable, throw
                if (isReadOnly)
                    throw new ArgumentException("All option properties must be writeable! Make sure the setter exists and is public.");

                //Get the option name and property type
                string optionName = optAttribute.OptionName;
                Type fieldType = prop.PropertyType;

                //If the property's type is bool, we set it to whether the option is set or not
                if (fieldType == typeof(bool))
                {
                    prop.SetValue(obj, ContainsKey(fields, optionName));
                }
                //If it isn't bool, check if the property's option name is set
                else if (!ContainsKey(fields, optionName))
                {
                    //throw new ArgumentException($"Option {optionName} not found!", nameof(fields));

                    //If it isn't, set the property to null
                    prop.SetValue(obj, null);
                }
                else
                {
                    //Make sure the property's type is string
                    if (fieldType != typeof(string))
                    {
                        //If it isn't throw
                        throw new ArgumentException($"Property '{prop.Name}' must be of type String or Boolean!");
                    }

                    //It's all correct, set the property value

                    string value = fields
                        .Where(o => o.Key == optionName)
                        .SingleOrDefault().Value;

                    prop.SetValue(obj, value);
                }
            }
        }

        /// <summary>
        /// Loops through all of <typeparamref name="T"/>'s properties and extracts their options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strict">If any property without an option definition is found and this is
        /// false, an exception will be thrown.</param>
        /// <exception cref="ArgumentException"></exception>
        private static IEnumerable<Option> GetOptionDefinitions<T>(bool strict = false) where T : ArgumentStructure
        {
            Type objType = typeof(T);
            PropertyInfo[] properties = objType.GetProperties();

            foreach (var prop in properties)
            {
                //Skip the Arguments property
                if (prop.Name == nameof(Arguments) || prop.Name == nameof(CommandConfiguration))
                    continue;

                //Try to get OptionAttribute
                OptionAttribute optAttribute = prop.GetCustomAttribute<OptionAttribute>();

                if (optAttribute == null)
                    throw new ArgumentException("Property without option found.");

                //Check if it contains an option definition
                if (optAttribute.OptionDefinition == null)
                    throw new ArgumentException("Option property without definition found.");

                //It does contain one, return it
                yield return optAttribute.OptionDefinition;
            }
        }

        /// <summary>
        /// Joins a string array into a single string. If any string in the array contains spaces, it will be
        /// escaped using quotes.
        /// </summary>
        /// <param name="arr">Array containing the strings to be joined.</param>
        /// <returns></returns>
        private static string Join(string[] arr)
        {
            string line = "";

            for (int i = 0; i < arr.Length; i++)
            {
                string item = arr[i];

                if (item.Contains(" "))
                {
                    item = $"\"{item}\"";
                }

                line += item + " ";
            }

            return line.TrimEnd();
        }
        
        #region Parse
        /// <summary>
        /// Get option definitions from <typeparamref name="T"/> and parse arguments.
        /// </summary>
        /// <typeparam name="T"><see cref="Arguments"/> class containing option definitions.</typeparam>
        /// <param name="args">String array containing the arguments.</param>
        public static T Parse<T>(string[] args) where T : ArgumentStructure
        {
            //Get option definitions from the Arguments class
            Option[] options = GetOptionDefinitions<T>(strict: true).ToArray();

            return Parse<T>(args, options);
        }

        /// <summary>
        /// Parse arguments with explicit options..
        /// </summary>
        /// <typeparam name="T"><see cref="Arguments"/> derived class.</typeparam>
        /// <param name="args">String array containing the arguments.</param>
        /// <param name="options">Option array.</param>
        public static T Parse<T>(string[] args, Option[] options) where T : ArgumentStructure
        {
            //Join the arguments array
            string line = Join(args);

            return Parse<T>(line, options);
        }


        /// <summary>
        /// Get option definitions from <typeparamref name="T"/> and parse arguments.
        /// </summary>
        /// <typeparam name="T"><see cref="Arguments"/> class containing option definitions.</typeparam>
        /// <param name="line">Arguments line.</param>
        public static T Parse<T>(string line) where T : ArgumentStructure
        {
            //Get option definitions from the Arguments class
            Option[] options = GetOptionDefinitions<T>(strict: true).ToArray();

            return Parse<T>(line, options);
        }

        /// <summary>
        /// Parse arguments with explicit options.
        /// </summary>
        /// <typeparam name="T"><see cref="Arguments"/> derived class.</typeparam>
        /// <param name="line">String line containing all the arguments.</param>
        /// <param name="options">Option array.</param>
        /// <exception cref="ArgumentException"></exception>
        public static T Parse<T>(string line, Option[] options) where T : ArgumentStructure
        {
            Contract.Requires(options != null);
            Contract.Requires(line != null);

            //Parse the line and get the values
            List<KeyValuePair<string, string>> reflectionValues = Parse(line, options)
                .Select(o => new KeyValuePair<string, string>(o.Key?.Name, o.Value))
                .ToList();

            //Create an argument object instance 
            T argsInstance = Activator.CreateInstance(typeof(T)) as T;

            //Get the parsed values that have a key. Those that don't will be command arguments.
            var filteredValues = reflectionValues
                .Where(o => o.Key != null)
                .ToList();

            //Apply fields to the Arguments instance
            ApplyReflection(argsInstance, filteredValues);


            //Get command arguments
            var arguments = reflectionValues
                .Where(o => o.Key == null)
                .Select(o => o.Value);

            //Too many given arguments
            if (arguments.Count() > argsInstance.CommandConfiguration.Arguments.Count)
                throw new ArgumentException("Too many arguments.");

            //Create a queue with the argument definitions
            Queue<ArgumentDefinition> argumentQueue =
                new Queue<ArgumentDefinition>(argsInstance.CommandConfiguration.Arguments);
            
            Dictionary<string, string> finalArguments = new Dictionary<string, string>();

            //Loop through the user-given arguments
            foreach (var item in arguments)
            {
                //Pop a definition from the queue
                ArgumentDefinition def = argumentQueue.Dequeue();

                //If the argument name is duplicated, throw
                if (finalArguments.ContainsKey(def.Name))
                    throw new ArgumentException($"Duplicate argument name '{def.Name}'");

                //Set the argument value in the dictionary
                finalArguments[def.Name] = item;
            }

            //Loop through the remaining unset argument definitions
            foreach (var item in argumentQueue)
            {
                //If any definition is not optional, throw
                if (!item.Optional)
                    throw new ArgumentException($"Missing argument '{item.Name}'");
            }

            //Set the arguments in the object instance
            argsInstance.Arguments = new ReadOnlyDictionary<string, string>(finalArguments);
            
            return argsInstance;
        }


        /// <summary>
        /// Parse arguments. The values will be returned as a (<see cref="Option"/>, string) dictionary.
        /// </summary>
        /// <param name="args">String array containing the arguments.</param>
        /// <param name="options">Option array.</param>
        public static List<KeyValuePair<Option, string>> Parse(string[] args, Option[] options)
        {
            //Join the arguments array
            string line = Join(args);

            return Parse(line, options);
        }

        /// <summary>
        /// Parse arguments. The values will be returned as a (<see cref="Option"/>, string) KeyValuePair list.
        /// </summary>
        /// <param name="line">Arguments line.</param>
        /// <param name="options">Option array.</param>
        public static List<KeyValuePair<Option, string>> Parse(string line, Option[] options)
        {
            Contract.Requires(options != null);
            Contract.Requires(line != null);

            OptionParser parser = new OptionParser(new OptionParser.Configuration(options));
            var values = parser.ParseAll(line).ToList();

            List<KeyValuePair<Option, string>> ret = new List<KeyValuePair<Option, string>>();

            foreach (var item in values)
            {
                ret.Add(new KeyValuePair<Option, string>(item.Option, item.Argument));
            }

            return ret;
        }
        #endregion
        
        /// <summary>
        /// Get a command's usage.
        /// </summary>
        /// <typeparam name="T">Command argument structure.</typeparam>
        /// <param name="programName">The program's command line name. If null, the first line is skipped.</param>
        /// <param name="spacing">The spacing between options and usage.</param>
        /// <param name="multipleMarker">Append (+) to those commands that can be passed more than once.</param>
        public static string GetCommandUsage<T>(string programName = null, int spacing = 4,
            bool multipleMarker = true) where T : ArgumentStructure
        {
            StringBuilder builder = new StringBuilder();

            if (programName != null)
                builder.AppendLine(programName + " [options]"); //TODO Get arguments and append their names

            builder.AppendLine("Usage:");

            var options = GetOptionDefinitions<T>();
            
            int maxLineLength = options
                .Select(o => GetLine(o).Length)
                .Max() + spacing;

            foreach (var option in options)
            {
                string line = GetLine(option);

                int length = line.Length;
                int spaces = maxLineLength - length;

                string space = new string(' ', spaces);

                line += space;
                line += option.Usage;

                if (option.CanAppearMultipleTimes && multipleMarker)
                    line += " (+)";

                builder.AppendLine(line);
            }

            return builder.ToString();

            string GetLine(Option option)
            {
                string line = $"  -{option.ShortOption}";

                if (option.LongOption != null)
                    line += $", --{option.LongOption}";

                return line;
            }
        }
        #endregion

        /// <summary>
        /// This command's configuration.
        /// </summary>
        public virtual Configuration CommandConfiguration { get; } = new Configuration();

        /// <summary>
        /// The passed command arguments
        /// </summary>
        public IReadOnlyDictionary<string, string> Arguments { get; private set; }
            = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    }
}
