using System.Threading.Tasks;
using System.Collections.Generic;
using CryptoShool.Web.Models;

namespace CryptoShool.Web.Services
{
    public interface ICryptoService
    {
        Task<string> Encrypt(string algorithm, string text, string key);
        Task<string> Decrypt(string algorithm, string text, string key);
        Task<bool> CheckAnswer(string algorithm, string text, string key, string answer);
        Task<List<UserProgress>> GetUserProgress(string userId);
        Task UpdateProgress(string userId, string algorithm, int score);
    }
} 