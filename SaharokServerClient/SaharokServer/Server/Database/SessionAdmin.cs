using SaharokServer.Encryption;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public SessionAdmin(Admin admin, User user)
        {
            Admin = admin;
            User = user;
            TimeOn = DateTime.Now;
            IsOnline = true;
            ServerNumber = ServerObject.ServerNumber;
            IsSuccessfulLogin = false;
        }

        public void Login(ref Admin admin)
        {
            IsSuccessfulLogin = true;
            admin.LastHWID = admin.NowHWID;
            admin.LastIP = admin.NowIP;
            admin.LastNamePC = admin.NowNamePC;
            admin.LastSuccessfulLogin = DateTime.Now;
            if (ServerObject.ServerNumber == 1)
            {
                admin.IsOnlineServer1 = true;
            }
            else if (ServerObject.ServerNumber == 2)
            {
                admin.IsOnlineServer2 = true;
            }
        }

        public void Disconnect(ref Admin admin)
        {
            TimeOff = DateTime.Now;
            ConnectionTime = string.Format("{0:HH:mm:ss}", new DateTime().AddTicks(TimeOff.Ticks - TimeOn.Ticks));
            IsOnline = false;
            foreach (var s in admin.SessionsAdmin)
            {
                if (s.ServerNumber == ServerObject.ServerNumber && s.IsOnline)
                {
                    return;
                }
            }
            if (ServerObject.ServerNumber == 1)
            {
                admin.IsOnlineServer1 = false;
            }
            else if (ServerObject.ServerNumber == 2)
            {
                admin.IsOnlineServer2 = false;
            }
        }
    }
}
