using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliHelper
{
    internal interface Token
    {

    }

    internal struct OptionToken : Token
    {
        public string Option;
        public bool Long;
        public Guid ID;

        public OptionToken(string option, bool longOption)
        {
            this.Option = option;
            this.Long = longOption;
            this.ID = Guid.NewGuid();
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
