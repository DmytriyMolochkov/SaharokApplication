using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsProjectServer
{
    [Serializable]
    public class SectionNameTemplate
    {
        public string Template { get; set; }
        public char Separator { get; set; }
    }
}
