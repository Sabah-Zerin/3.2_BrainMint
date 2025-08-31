using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;

namespace Brain_Mint.Models
{
    public class BrainMintDbContext : DbContext
    {
        public BrainMintDbContext() : base("name=BrainMintDB")
        {
        }

        public DbSet<User> Users { get; set; }
        // Add other DbSets as needed (Quiz, Question, etc.)

        // You can add this method to configure your model
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}