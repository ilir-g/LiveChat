using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveChat_IlirG.Models
{
    public class LiveChatContext : DbContext
    {
        public LiveChatContext(DbContextOptions<LiveChatContext> options)
            : base(options)
        {
        }
        

        public DbSet<User> Users { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<UserContacts> UserContacts { get; set; }
    }
}

