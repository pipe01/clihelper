using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiConsole
{
    internal interface Token
    {

    }

    internal struct OptionToken : Token
    {
        public string Option;

        public OptionToken(string option)
        {
            this.Option = option;
        }
    }

    internal struct OptionArgumentToken : Token
    {
        public OptionToken OptionToken;
        public string Text;

        public OptionArgumentToken(OptionToken option, string text)
        {
            this.OptionToken = option;
            this.Text = text;
        }
    }

    internal struct ArgumentToken : Token
    {
        public string Text;

        public ArgumentToken(string text)
        {
            this.Text = text;
        }
    }
}
