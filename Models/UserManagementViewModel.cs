using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Brain_Mint.Models;

namespace Brain_Mint.Models
{
    public class UserManagementViewModel
    {
        public List<User> Teachers { get; set; }
        public List<User> Students { get; set; }
        public string SearchTerm { get; set; }
        public string SelectedRole { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }

        public UserManagementViewModel()
        {
            Teachers = new List<User>();
            Students = new List<User>();
            SearchTerm = "";
            SelectedRole = "All";
        }
    }
}