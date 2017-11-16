using System;
using System.Collections.Generic;
using System.Text;

namespace PiConsole
{
    /// <summary>
    /// This property represents an option in an <see cref="ArgumentStructure"/> object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionAttribute : Attribute
    {
        public string OptionName { get; private set; }
        public Option OptionDefinition { get; private set; }

        /// <summary>
        /// Option name as specified when creating the options. This will search the option from the options list
        /// when parsing
        /// </summary>
        public OptionAttribute(string optionName)
        {
            this.OptionName = optionName;
        }

        /// <summary>
        /// Creates a new option definition and binds it to this property.
        /// </summary>
        /// <param name="shortOpt">Short option. E.g., "e".</param>
        /// <param name="longOpt">Long option. E.g., "extract". Can be null.</param>
        /// <param name="name">Name. E.g., "extract"</param>
        /// <param name="usage">Option usage. E.g., "Extracts a file"</param>
        /// <param name="hasArgument">Does this option have arguments?</param>
        /// <param name="multipleTimes">Can this option appear more than once?</param>
        public OptionAttribute(string shortOpt, string longOpt, string name, string usage, bool hasArgument = false,
            bool multipleTimes = true) : this(name)
        {
            this.OptionDefinition = new Option(shortOpt, longOpt, name, usage, hasArgument, multipleTimes);
        }
    }
}
