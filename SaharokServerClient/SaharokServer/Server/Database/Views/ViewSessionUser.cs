using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SaharokServer.Server.Database.Views
{
    public class ViewSessionUser
    {
        public int SessionID { get; set; }
        public string NamePC { get; set; }
        public string NameUser { get; set; } // задаётся вручную
        public DateTime TimeOn { get; set; }
        public DateTime TimeOff { get; set; }
        public string ConnectionTime { get; set; }
        public int ServerNumber { get; set; }
        public bool IsOnline { get; set; }
    }
}
