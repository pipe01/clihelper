﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace CliHelper
{
    /// <summary>
    /// Represents an option in the command line arguments
    /// </summary>
    public class Option
    {
        /// <summary>
        /// Represents the short option without the dash. E.g., "e" for -e.
        /// </summary>
        public string ShortOption { get; set; }

        /// <summary>
        /// Represents the long option without the dash. E.g., "extract" for --extract. Can be null.
        /// </summary>
        public string LongOption { get; set; }

        /// <summary>
        /// The option's name. E.g., "extract"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The option's usage. E.g., "Extracts a file.". Can be null
        /// </summary>
        public string Usage { get; set; }

        /// <summary>
        /// True if the option has to have an argument. E.g., for "-e 'file.txt'" this would be true,
        /// for "-h", this would be false.
        /// </summary>
        public bool HasArgument { get; set; }

        /// <summary>
        /// The option's argument name. Can be null.
        /// </summary>
        public string ArgumentName { get; set; }

        /// <summary>
        /// Whether this option can appear multiple times.
        /// </summary>
        public bool CanAppearMultipleTimes { get; set; }

        /// <summary>
        /// Initializes a new <see cref="Option"/> instance.
        /// </summary>
        public Option() { }

        /// <summary>
        /// Initializes a new <see cref="Option"/> instance.
        /// </summary>
        /// <param name="shortOpt">Short option. E.g., "e".</param>
        /// <param name="longOpt">Long option. E.g., "extract". Can be null.</param>
        /// <param name="name">Name. E.g., "extractFile".</param>
        /// <param name="usage">Option usage. E.g., "Extracts a file".</param>
        /// <param name="hasArgument">Does this option have arguments?</param>
        /// <param name="argName">Argument name. E.g., "filename".</param>
        /// <param name="multipleTimes">Can this option appear more than once?</param>
        /// <exception cref="ArgumentException"></exception>
        public Option(string shortOpt, string longOpt, string name, string usage,
            bool hasArgument = false, string argName = null, bool multipleTimes = true)
        {
            CheckEmpty(shortOpt, "Short option", nameof(shortOpt));
            CheckEmpty(name, "Option name", nameof(name));

            //If the option has an argument, require an argument name
            if (CheckEmpty(argName, "Argument name", nameof(argName), false) && hasArgument)
                throw new ArgumentException("Argument name required.", nameof(argName));

            this.ShortOption = shortOpt;
            this.LongOption = longOpt;
            this.Name = name;
            this.Usage = usage;
            this.HasArgument = hasArgument;
            this.ArgumentName = argName;
            this.CanAppearMultipleTimes = multipleTimes;
        }

        /// <summary>
        /// Checks if a string is null or empty.
        /// If the string is empty, returns false.
        /// If the string is empty and <paramref name="throwException"/> is true, throws an exception.
        /// </summary>
        private bool CheckEmpty(string str, string name, string varName, bool throwException = true)
        {
            bool check = string.IsNullOrWhiteSpace(str);

            if (check && throwException)
                throw new ArgumentException(name + " must not be empty.", varName);

            return !check;
        }
    }
}
