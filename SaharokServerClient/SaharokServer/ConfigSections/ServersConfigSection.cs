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

namespace SaharokServer
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
        [ConfigurationProperty("ServerNumber", IsKey = true, IsRequired = true)]
        public int ServerNumber
        {
            get { return (Convert.ToInt32((base["ServerNumber"]))); }
            set { base["ServerNumber"] = value; }
        }

        [ConfigurationProperty("UserPort", IsKey = false, IsRequired = true)]
        public int UserPort
        {
            get { return (Convert.ToInt32((base["UserPort"]))); }
            set { base["UserPort"] = value; }
        }

        [ConfigurationProperty("AdminPort", IsKey = false, IsRequired = true)]
        public int AdminPort
        {
            get { return (Convert.ToInt32((base["AdminPort"]))); }
            set { base["AdminPort"] = value; }
        }

        [ConfigurationProperty("AllowUsingNewClient", IsKey = false, IsRequired = true)]
        public bool AllowUsingNewClient
        {
            get { return (Convert.ToBoolean((base["AllowUsingNewClient"]))); }
            set { base["AllowUsingNewClient"] = value; }
        }


        [ConfigurationProperty("DBname",  IsKey = false, IsRequired = true)]
        public string DBname
        {
            get { return ((string)(base["DBname"])); }
            set { base["DBname"] = value; }
        }
    }
}
