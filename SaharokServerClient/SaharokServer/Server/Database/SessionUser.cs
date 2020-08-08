using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SaharokServer.Server.Database
{
    [Table("session_user")]
    public class SessionUser
    {
        [Key] public int ID { get; set; }
        public int UserID { get; set; }
        [Required] public DateTime TimeOn { get; set; }
        public DateTime TimeOff { get; set; }
        public string ConnectionTime { get; set; }
        public int ServerNumber { get; set; }
        public bool IsOnline { get; set; }

        [InverseProperty("SessionsUser")]
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public virtual ErrorUser ErrorUser { get; set; }
        public virtual ICollection<RequestResponse> RequestResponse { get; set; }

        public SessionUser()
        {

        }

        public SessionUser(User user)
        {
            User = user;
            TimeOn = user.LastConnection;
            IsOnline = true;
            ServerNumber = ServerObject.ServerNumber;
        }

        public void Disconnect(ref User user)
        {
            TimeOff = DateTime.Now;
            ConnectionTime = string.Format("{0:HH:mm:ss}", new DateTime().AddTicks(TimeOff.Ticks - TimeOn.Ticks));
            IsOnline = false;
            foreach (var s in user.SessionsUser)
            {
                if (s.ServerNumber == ServerObject.ServerNumber && s.IsOnline)
                {
                    return;
                }
            }
            if (ServerObject.ServerNumber == 1)
            {
                user.IsOnlineServer1 = false;
            }
            else if (ServerObject.ServerNumber == 2)
            {
                user.IsOnlineServer2 = false;
            }
        }
    }
}
