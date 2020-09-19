using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saharok
{
    [Serializable]
    public class SectionNameTemplate
    {
        public string Template { get; set; }
        public char Separator { get; set; }
        public SectionNameTemplate(SectionNameTemplateConfigSection nameTemplateConfigSection)
        {
            Template = nameTemplateConfigSection.Template;
            Separator = nameTemplateConfigSection.Separator;
        }
    }

    public class SectionNameTemplateConfigSection :
    ConfigurationSection
    {
        [ConfigurationProperty("Template", DefaultValue = "|раздел документации|", IsRequired = true)]
        [StringValidator(InvalidCharacters = @"<>*?/:\", MinLength = 1)]
        public string Template
        {
            get => Convert.ToString(this["Template"]);
            set => this["Template"] = value;
        }

        [ConfigurationProperty("Separator", DefaultValue = ' ', IsRequired = true)]
        public char Separator
        {
            get => Convert.ToChar(this["Separator"]);
            set => this["Separator"] = value;
        }
    }
}
