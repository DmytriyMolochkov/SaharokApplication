using SaharokServer.Encryption;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace SaharokServer.Server.Database
{
    [Table("session_admin")]
    public class SessionAdmin
    {
        [Key] public int ID { get; set; }
        public int UserID { get; set; }
        public int AdminID { get; set; }
        [Required] public DateTime TimeOn { get; set; }
        public DateTime TimeOff { get; set; }
        public string ConnectionTime { get; set; }
        public int ServerNumber { get; set; }
        public bool IsOnline { get; set; }
        public bool IsSuccessfulLogin { get; set; }

        [InverseProperty("SessionsAdmin")]
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [InverseProperty("SessionsAdmin")]
        [ForeignKey("AdminID")]
        public virtual Admin Admin { get; set; }

        public virtual ErrorAdmin ErrorAdmin { get; set; }
        //public virtual ICollection<RequestResponse> RequestResponse { get; set; }

        public SessionAdmin()
        {

        }

        public void Start(Admin admin, User user, int serverNumber)
        {
            Admin = admin;
            User = user;
            TimeOn = DateTime.Now;
            IsOnline = true;
            ServerNumber = serverNumber;
            IsSuccessfulLogin = false;
        }

        public void Login(Admin admin)
        {
            IsSuccessfulLogin = true;
            admin.LastHWID = admin.NowHWID;
            admin.LastIP = admin.NowIP;
            admin.LastNamePC = admin.NowNamePC;
            admin.LastSuccessfulLogin = DateTime.Now;
            if (ServerNumber % 2 > 0)
                admin.IsOnlineServer1 = true;
            else
                admin.IsOnlineServer2 = true;
        }

        public void Disconnect()
        {
            TimeOff = DateTime.Now;
            ConnectionTime = string.Format("{0:HH:mm:ss}", new DateTime().AddTicks(TimeOff.Ticks - TimeOn.Ticks));
            IsOnline = false;
            foreach (var s in Admin.SessionsAdmin)
            {
                if (s.ServerNumber == ServerNumber && s.IsOnline)
                {
                    return;
                }
            }
            if (ServerNumber % 2 > 0)
                Admin.IsOnlineServer1 = false;
            else
                Admin.IsOnlineServer2 = false;
        }
    }
}
