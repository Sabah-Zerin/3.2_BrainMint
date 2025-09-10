using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brain_Mint.Models
{
    [Table("LoginLogs")]
    public class LoginLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [StringLength(20)]
        public string UserRole { get; set; }

        [Required]
        public DateTime LoginTime { get; set; }

        [StringLength(45)]
        public string IpAddress { get; set; }

        [StringLength(500)]
        public string UserAgent { get; set; }

        public DateTime? LogoutTime { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}