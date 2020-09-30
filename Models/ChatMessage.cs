using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LiveChat_IlirG.Models
{
    public partial class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string RecipientId { get; set; }
        public string Message { get; set; }
        public string Image { get; set; }
        public DateTime Date { get; set; }
    }
   
}
