using System;
using System.Collections.Generic;
using System.Text;

namespace SaharokServer.Server.Database.Views
{
    public class ViewUser
    {
        public int ID { get; set; }
        public string HWID { get; set; }
        public string NamePC { get; set; }
        public string NameUser { get; set; } // задаётся вручную
        public string FirstIP { get; set; }
        public string LastIP { get; set; }
        public DateTime FirstConnection { get; set; }
        public DateTime LastConnection { get; set; }
        public string Comment { get; set; } // задаётся вручную
        public bool IsOnlineServer1 { get; set; }
        public bool IsOnlineServer2 { get; set; }
        public bool AllowUsing { get; set; } // изменяется вручную
    }
}
