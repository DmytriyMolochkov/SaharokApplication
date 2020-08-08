using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SaharokServer.Server.Database
{
    [Table("error_server_object")]
    public class ErrorServerObject
    {
        [Key] public int ID { get; set; }
        [Required] public DateTime Time { get; set; }
        public string ErrorMessage { get; set; }
        public int ServerNumber { get; set; }

        public ErrorServerObject()
        {

        }

        public ErrorServerObject(Exception ex)
        {
            Time = DateTime.Now;
            ErrorMessage = ex.Message;
            ServerNumber = ServerObject.ServerNumber;
        }
    }
}
