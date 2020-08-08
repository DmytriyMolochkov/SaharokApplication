using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SaharokServer.Server.Database
{
    [Table("error_admin")]
    public class ErrorAdmin
    {
        [Key] public int ID { get; set; }
        public int SessionID { get; set; }
        [Required] public DateTime Time { get; set; }
        public string ErrorMessage { get; set; }
        [ForeignKey("SessionID")] public virtual SessionAdmin SessionAdmin { get; set; }

        public ErrorAdmin()
        {

        }

        public ErrorAdmin(SessionAdmin session, Exception ex)
        {
            SessionAdmin = session;
            Time = DateTime.Now;
            ErrorMessage = ex.Message;
        }
    }
}
