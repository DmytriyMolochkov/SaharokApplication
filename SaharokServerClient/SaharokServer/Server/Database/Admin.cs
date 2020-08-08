using SaharokServer.Encryption;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;
using System.Text;

namespace SaharokServer.Server.Database
{
    [Table("admin")]
    public class Admin
    {
        [Key] public int ID { get; set; }
        [MaxLength(30)] public string Login { get; set; } // задаётся вручную
        [MaxLength(30)] public string Password { get; set; } // задаётся вручную
        [MaxLength(15)] public string LastIP { get; set; }
        public DateTime LastSuccessfulLogin { get; set; }
        public string LastNamePC { get; set; }
        [MaxLength(40)] public string LastHWID { get; set; }
        public string NowIP { get; set; }
        public string NowNamePC { get; set; }
        public string NowHWID { get; set; }
        public bool IsOnlineServer1 { get; set; }
        public bool IsOnlineServer2 { get; set; }

        public virtual ICollection<SessionAdmin> SessionsAdmin { get; set; }


        public Admin()
        {

        }

        public Admin(TcpClient tcpClient, NetworkStream nStream, MyAes myAes)
        {
            if (nStream != null)
                nStream.Flush();
            myAes.GenerateParameters();
            myAes.ExportParameters(nStream);
            string[] infoAdmin = (string[])myAes.DecryptFromStream(nStream);
            NowHWID = infoAdmin[0];
            NowNamePC = infoAdmin[1];
            NowIP = Convert.ToString(((System.Net.IPEndPoint)tcpClient.Client.RemoteEndPoint).Address);
        }
    }
}
