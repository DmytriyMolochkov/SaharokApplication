using System;
using System.Collections.Generic;
using System.Text;

namespace SaharokServer.Server.Database.Views
{
    public class ViewRequestResponse
    {
        public int ID { get; set; }
        public int SessionID { get; set; }
        public string NamePC { get; set; }
        public string NameUser { get; set; } // задаётся вручную
        public string NameProject { get; set; }
        public string CodeProject { get; set; }
        public string PathProject { get; set; }
        public DateTime Time { get; set; }
        public string ClientRequest { get; set; }
        public string ServerResponse { get; set; }
        public int ServerNumber { get; set; }
    }
}
