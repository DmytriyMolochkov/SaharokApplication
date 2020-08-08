using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SaharokServer.Server.Database
{
    [Table("request_response")]
    public class RequestResponse
    {
        [Key] public int ID { get; set; }
        [Required] public int SessionID { get; set; }
        public string NameProject { get; set; }
        public string CodeProject { get; set; }
        public string PathProject { get; set; }
        [Required] public DateTime Time { get; set; }
        public string ClientRequest { get; set; }
        public string ServerResponse { get; set; }
        [InverseProperty("RequestResponse")]
        [ForeignKey("SessionID")]
        public virtual SessionUser SessionUser { get; set; }
        public virtual BannedResponse BannedResponse { get; set; }
        public virtual ErrorResponse ErrorResponse { get; set; }

        public RequestResponse()
        {

        }

        public RequestResponse(SessionUser session)
        {
            SessionUser = session;
            Time = DateTime.Now;
        }
    }
}
