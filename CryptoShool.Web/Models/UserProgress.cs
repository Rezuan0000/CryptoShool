using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoShool.Web.Models
{
    public class UserProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Algorithm { get; set; }

        public int Score { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
} 