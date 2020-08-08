using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SaharokServer.Server.Database
{
    [Table("error_response")]
    public class ErrorResponse
    {

        [Key] public int ID { get; set; }
        [Required] public int RequestResponseID { get; set; }
        [ForeignKey("RequestResponseID")] public virtual RequestResponse RequestResponse { get; set; }

        public ErrorResponse()
        {
        }
        public ErrorResponse(RequestResponse requestResponse)
        {
            RequestResponse = requestResponse;
        }
    }
}
