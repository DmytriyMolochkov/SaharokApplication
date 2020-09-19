using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Xml;
using System.Windows;
using System.Configuration;

namespace SaharokAdmin
{

    public class ServersConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("Servers")]
        public ServersCollection Servers
        {
            get { return ((ServersCollection)(base["Servers"])); }
            set { base["Servers"] = value; }
        }
    }

    [ConfigurationCollection(typeof(ServerElement), AddItemName = "Server")]
    public class ServersCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServerElement)(element)).ServerNumber;
        }

        public ServerElement this[int idx]
        {
            get { return (ServerElement)BaseGet(idx); }
        }
    }

    public class ServerElement : ConfigurationElement
    {

        [ConfigurationProperty("IP", IsKey = false, IsRequired = true)]
        public string IP
        {
            get { return ((string)(base["IP"])); }
            set { base["IP"] = value; }
        }

        [ConfigurationProperty("ServerNumber", IsKey = true, IsRequired = true)]
        public int ServerNumber
        {
            get { return (Convert.ToInt32((base["ServerNumber"]))); }
            set { base["ServerNumber"] = value; }
        }

        [ConfigurationProperty("AdminPort", IsKey = false, IsRequired = true)]
        public int AdminPort
        {
            get { return (Convert.ToInt32((base["AdminPort"]))); }
            set { base["AdminPort"] = value; }
        }
    }
}
