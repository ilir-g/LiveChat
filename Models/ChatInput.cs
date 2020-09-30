using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LiveChat_IlirG.Models
{
    public partial class ChatInput
    {
        /// <summary>
        /// UserId who is sending the message
        /// </summary>
        [Required]
        public string UserId { get; set; }
        /// <summary>
        /// RecipientId, who is getting the message
        /// </summary>
        [Required]
        public string RecipientId { get; set; }
        /// <summary>
        /// SecurityCode, for the user who is sending the message
        /// </summary>
        [Required]
        public string SecurityCode { get; set; }
        /// <summary>
        /// Message, is the content you are sending to the user
        /// </summary>
        public string Message { get; set; }

    }
}
