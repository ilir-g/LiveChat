using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LiveChat_IlirG.Models
{
    public partial class User 
    {
        [Key]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Image { get; set; }
        [Required]
        public string SecurityCode { get; set; }
        [NotMapped]
        public ChatMessage ChatMessages { get; set; }

    }
  
    public partial class UserResponse
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Image { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        
        
    }

    public partial class UserContacts
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string ContactUserId { get; set; }
    }

}
