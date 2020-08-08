using System;
using System.Collections.Generic;
using System.Text;

namespace SaharokServer.Server.Database.Views
{
    public class ViewErrorAdmin
    {
        public int ID { get; set; }
        public string Login { get; set; }
        public int SessionID { get; set; }
        public string NamePC { get; set; }
        public string NameUser { get; set; } // задаётся вручную
        public DateTime Time { get; set; }
        public string ErrorMessage { get; set; }
        public int ServerNumber { get; set; }
    }
}
