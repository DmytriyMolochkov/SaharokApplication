using System;
using System.Collections.Generic;
using System.Text;

namespace SaharokServer.Server.Database.Views
{
    public class ViewErrorServerObject
    {
        public int ID { get; set; }
        public DateTime Time { get; set; }
        public string ErrorMessage { get; set; }
        public int ServerNumber { get; set; }
    }
}
