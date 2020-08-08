using System;
using System.Collections.Generic;
using System.Text;

namespace SaharokServer.Server.Database.Views
{
    public class ViewSessionAdmin
    {
        public int ID { get; set; }
        public string Login { get; set; }
        public string NamePC { get; set; }
        public string NameUser { get; set; } // задаётся вручную
        public DateTime TimeOn { get; set; }
        public DateTime TimeOff { get; set; }
        public string ConnectionTime { get; set; }
        public int ServerNumber { get; set; }
        public bool IsOnline { get; set; }
        public bool IsSuccessfulLogin { get; set; }
    }
}
