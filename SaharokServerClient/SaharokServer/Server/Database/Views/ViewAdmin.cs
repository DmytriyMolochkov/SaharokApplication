using System;
using System.Collections.Generic;
using System.Text;

namespace SaharokServer.Server.Database.Views
{
    public class ViewAdmin
    {
        public int ID { get; set; }
        public string Login { get; set; } // задаётся вручную
        public string Password { get; set; } // задаётся вручную
        public string LastIP { get; set; }
        public DateTime LastSuccessfulLogin { get; set; }
        public string LastNamePC { get; set; }
        public string LastHWID { get; set; }
        public bool IsOnlineServer1 { get; set; }
        public bool IsOnlineServer2 { get; set; }
    }
}
