using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CryptoShool.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<UserProgress> Progress { get; set; } = new List<UserProgress>();
    }
} 