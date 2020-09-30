using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LiveChat_IlirG.Models
{
    public partial class Group
    {
        [Key]
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }
}
