using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Brain_Mint.Models
{
    // Simple assignment class for backward compatibility (remove the other Assignment classes)
    public class SimpleAssignment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
    }
}