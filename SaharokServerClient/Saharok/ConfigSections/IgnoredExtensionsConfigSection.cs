using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Saharok
{
    class IgnoredExtensionsConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("value", DefaultValue = "", IsRequired = true)]
        public string value
        {
            get => Convert.ToString(this["value"]);
            set => this["value"] = value;
        }
    }
}
