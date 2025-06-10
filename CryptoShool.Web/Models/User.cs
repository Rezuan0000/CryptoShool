using System.ComponentModel.DataAnnotations;

namespace CryptoShool.Web.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите имя пользователя")]
        [Display(Name = "Имя пользователя")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        public List<UserProgress> Progress { get; set; } = new List<UserProgress>();
    }
} 