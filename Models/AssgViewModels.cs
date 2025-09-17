using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Brain_Mint.Models
{
    public class AssgViewModels
    {
        public Assignment Assignment { get; set; }
        public bool HasSubmitted { get; set; }
        public AssignmentSubmission Submission { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysUntilDue { get; set; }

    }
}