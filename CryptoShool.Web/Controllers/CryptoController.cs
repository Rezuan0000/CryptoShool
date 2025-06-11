using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CryptoShool.Web.Services;
using CryptoShool.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace CryptoShool.Web.Controllers
{
    public class CryptoController : Controller
    {
        private readonly ICryptoService _cryptoService;
        private readonly ICipherService _cipherService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CryptoController(
            ICryptoService cryptoService,
            ICipherService cipherService,
            UserManager<ApplicationUser> userManager)
        {
            _cryptoService = cryptoService;
            _cipherService = cipherService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Caesar()
        {
            return View();
        }

        public IActionResult Vigenere()
        {
            return View();
        }

        public IActionResult XOR()
        {
            return View();
        }

        public IActionResult Hill()
        {
            return View();
        }

        public IActionResult RSA()
        {
            return View();
        }

        public IActionResult Playfair()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Encrypt(string algorithm, string text, string key)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key))
            {
                return Json(new { success = false, message = "Текст и ключ не могут быть пустыми" });
            }

            var result = await _cryptoService.Encrypt(algorithm, text, key);
            return Json(new { success = true, result = result });
        }

        [HttpPost]
        public async Task<IActionResult> Decrypt(string algorithm, string text, string key)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key))
            {
                return Json(new { success = false, message = "Текст и ключ не могут быть пустыми" });
            }

            var result = await _cryptoService.Decrypt(algorithm, text, key);
            return Json(new { success = true, result = result });
        }

        [HttpPost]
        public async Task<IActionResult> CheckAnswer(string algorithm, string text, string key, string answer, string operation = "encrypt")
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(answer))
            {
                return Json(new { success = false, message = "Текст, ключ и ответ не могут быть пустыми" });
            }

            string result;
            if (operation.ToLower() == "decrypt")
            {
                // Для расшифровки сравниваем ответ с исходным текстом
                result = await _cryptoService.Decrypt(algorithm, text, key);
                var isCorrect = result.Equals(answer, StringComparison.OrdinalIgnoreCase);
                
                if (isCorrect)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        // Получаем текущий прогресс
                        var progress = await _cryptoService.GetUserProgress(user.Id);
                        var currentProgress = progress.FirstOrDefault(p => p.Algorithm == algorithm);
                        var currentScore = currentProgress?.Score ?? 0;
                        
                        // Увеличиваем прогресс на 20% для расшифровки
                        await _cryptoService.UpdateProgress(user.Id, algorithm, currentScore + 20);
                    }
                }
                
                return Json(new { success = true, result = isCorrect });
            }
            else
            {
                // Для шифрования сравниваем ответ с зашифрованным текстом
                result = await _cryptoService.Encrypt(algorithm, text, key);
                var isCorrect = result.Equals(answer, StringComparison.OrdinalIgnoreCase);
                
                if (isCorrect)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        // Получаем текущий прогресс
                        var progress = await _cryptoService.GetUserProgress(user.Id);
                        var currentProgress = progress.FirstOrDefault(p => p.Algorithm == algorithm);
                        var currentScore = currentProgress?.Score ?? 0;
                        
                        // Увеличиваем прогресс на 20% для шифрования
                        await _cryptoService.UpdateProgress(user.Id, algorithm, currentScore + 20);
                    }
                }
                
                return Json(new { success = true, result = isCorrect });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProgress()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new List<object>());
            }

            var progress = await _cryptoService.GetUserProgress(user.Id);
            var result = progress.Select(p => new
            {
                algorithmName = p.Algorithm,
                score = p.Score,
                isCompleted = p.Score >= 100
            }).ToList();

            return Json(result);
        }

        private int[,] ParseHillKey(string key)
        {
            var numbers = key.Split(',')
                .Select(n => int.Parse(n.Trim()))
                .ToArray();

            if (numbers.Length != 4)
            {
                throw new ArgumentException("Ключ для шифра Хилла должен содержать 4 числа, разделенных запятыми");
            }

            return new int[,]
            {
                { numbers[0], numbers[1] },
                { numbers[2], numbers[3] }
            };
        }

        private (int n, int e) ParseRSAKey(string key)
        {
            var parts = key.Split(',');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Ключ для RSA должен быть в формате 'n=число,e=число'");
            }

            var n = int.Parse(parts[0].Split('=')[1]);
            var e = int.Parse(parts[1].Split('=')[1]);

            return (n, e);
        }
    }
} 