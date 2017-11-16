﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Linq;
using System.Reflection;

namespace PiConsole
{
    public abstract class Arguments
    {
        private static bool ContainsKey<TKey, TValue>(List<KeyValuePair<TKey, TValue>> arr, TKey key)
        {
            return arr.Any(o => o.Key.Equals(key));
        }

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

        private static IEnumerable<Option> GetOptionDefinitions<T>() where T : Arguments
        {
            Type objType = typeof(T);
            PropertyInfo[] properties = objType.GetProperties();

            foreach (var prop in properties)
            {
                //Try to get OptionAttribute
                OptionAttribute optAttribute = prop.GetCustomAttribute<OptionAttribute>();

                if (optAttribute == null)
                    continue;

                //Check if it contains an option definition
                if (optAttribute.OptionDefinition == null)
                    continue;

                //It does contain one, return it
                yield return optAttribute.OptionDefinition;
            }
        }

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


        /// <summary>
        /// Get option definitions from <typeparamref name="T"/> and parse arguments.
        /// </summary>
        /// <typeparam name="T"><see cref="Arguments"/> class containing option definitions.</typeparam>
        /// <param name="args">String array containing the arguments.</param>
        public static T Parse<T>(string[] args) where T : Arguments
        {
            //Get option definitions from the Arguments class
            Option[] options = GetOptionDefinitions<T>().ToArray();

            return Parse<T>(args, options);
        }

        /// <summary>
        /// Parse arguments.
        /// </summary>
        /// <typeparam name="T"><see cref="Arguments"/> derived class.</typeparam>
        /// <param name="args">String array containing the arguments.</param>
        /// <param name="options">Option array.</param>
        public static T Parse<T>(string[] args, Option[] options) where T : Arguments
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
        public static T Parse<T>(string line) where T : Arguments
        {
            //Get option definitions from the Arguments class
            Option[] options = GetOptionDefinitions<T>().ToArray();

            return Parse<T>(line, options);
        }

        /// <summary>
        /// Parse arguments.
        /// </summary>
        /// <typeparam name="T"><see cref="Arguments"/> derived class.</typeparam>
        /// <param name="line">String line containing all the arguments.</param>
        /// <param name="options">Option array.</param>
        public static T Parse<T>(string line, Option[] options) where T : Arguments
        {
            Contract.Requires(options != null);
            Contract.Requires(line != null);

            List<KeyValuePair<string, string>> reflectionValues = Parse(line, options)
                .Select(o => new KeyValuePair<string, string>(o.Key.Name, o.Value))
                .ToList();

            T argsInstance = Activator.CreateInstance(typeof(T)) as T;

            ApplyReflection(argsInstance, reflectionValues);

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
    }
}
