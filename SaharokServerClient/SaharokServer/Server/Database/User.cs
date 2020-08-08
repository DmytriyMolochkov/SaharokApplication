using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using SaharokServer.Encryption;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaharokServer.Server.Database
{
    [Table("user")]
    public class User
    {
        [Key] public int ID { get; set; }
        [Required] [MaxLength(40)] public string HWID { get; set; }
        [Required] public string NamePC { get; set; }
        [MaxLength(30)] public string NameUser { get; set; } // задаётся вручную
        [Required] [MaxLength(15)] public string FirstIP { get; set; }
        [MaxLength(15)] public string LastIP { get; set; }
        [Required] public DateTime FirstConnection { get; set; }
        public DateTime LastConnection { get; set; }
        public string Comment { get; set; } // задаётся вручную
        public bool IsOnlineServer1 { get; set; }
        public bool IsOnlineServer2 { get; set; }
        [Required] public bool AllowUsing { get; set; } // изменяется вручную

        public virtual ICollection<SessionUser> SessionsUser { get; set; }
        public virtual ICollection<SessionAdmin> SessionsAdmin { get; set; }

        public User()
        {

        }

        public User(TcpClient tcpClient, NetworkStream nStream)
        {
            if (nStream != null)
                nStream.Flush();
            MyAes myAes = new MyAes();
            myAes.GenerateParameters();
            myAes.ExportParameters(nStream);
            string[] infoClient = (string[])myAes.DecryptFromStream(nStream);
            LastConnection = DateTime.Now;
            HWID = infoClient[0];
            NamePC = infoClient[1];
            LastIP = Convert.ToString(((System.Net.IPEndPoint)tcpClient.Client.RemoteEndPoint).Address);
            AllowUsing = true;
        }
    }
}
