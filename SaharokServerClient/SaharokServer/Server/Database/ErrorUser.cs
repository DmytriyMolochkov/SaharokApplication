using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SaharokServer.Server.Database
{
    [Table("error_user")]
    public class ErrorUser
    {
        [Key] public int ID { get; set; }
        public int SessionID { get; set; }
        [Required] public DateTime Time { get; set; }
        public string ErrorMessage { get; set; }
        [ForeignKey("SessionID")] public virtual SessionUser SessionUser { get; set; }

        public ErrorUser()
        {

        }

        public ErrorUser(SessionUser session, Exception ex)
        {
            SessionUser = session;
            Time = DateTime.Now;
            ErrorMessage = ex.Message;
        }
    }
}
