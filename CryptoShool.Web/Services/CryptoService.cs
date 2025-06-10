using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoShool.Web.Data;
using CryptoShool.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoShool.Web.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICipherService _cipherService;

        public CryptoService(ApplicationDbContext context, ICipherService cipherService)
        {
            _context = context;
            _cipherService = cipherService;
        }

        public async Task<bool> CheckAnswer(string algorithm, string text, string key, string answer)
        {
            answer = NormalizeString(answer);
            var result = await Encrypt(algorithm, text, key);
            return result == answer;
        }

        public async Task<List<UserProgress>> GetUserProgress(string userId)
        {
            return await _context.UserProgress
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task UpdateProgress(string userId, string algorithm, int score)
        {
            var progress = await _context.UserProgress
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Algorithm == algorithm);

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserId = userId,
                    Algorithm = algorithm,
                    Score = score
                };
                _context.UserProgress.Add(progress);
            }
            else
            {
                progress.Score = Math.Max(progress.Score, score);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string> Encrypt(string algorithm, string text, string key)
        {
            text = NormalizeString(text);
            key = NormalizeString(key);

            switch (algorithm.ToLower())
            {
                case "caesar":
                    if (!int.TryParse(key, out int shift))
                    {
                        throw new ArgumentException("Для шифра Цезаря ключ должен быть числом");
                    }
                    return CaesarEncrypt(text, shift);

                case "vigenere":
                    return VigenereEncrypt(text, key);

                case "playfair":
                    return PlayfairEncrypt(text, key);

                case "hill":
                    var keyMatrix = ParseHillKey(key);
                    return HillEncrypt(text, keyMatrix);

                case "rsa":
                    var rsaKey = ParseRSAKey(key);
                    return RSAEncrypt(text, rsaKey);

                case "xor":
                    return XOREncrypt(text, key);

                default:
                    throw new ArgumentException("Неизвестный алгоритм");
            }
        }

        public async Task<string> Decrypt(string algorithm, string text, string key)
        {
            text = NormalizeString(text);
            key = NormalizeString(key);

            switch (algorithm.ToLower())
            {
                case "caesar":
                    if (!int.TryParse(key, out int shift))
                    {
                        throw new ArgumentException("Для шифра Цезаря ключ должен быть числом");
                    }
                    return CaesarDecrypt(text, shift);

                case "vigenere":
                    return VigenereDecrypt(text, key);

                case "playfair":
                    return PlayfairDecrypt(text, key);

                case "hill":
                    var keyMatrix = ParseHillKey(key);
                    return HillDecrypt(text, keyMatrix);

                case "rsa":
                    var rsaKey = ParseRSAKey(key);
                    return RSADecrypt(text, rsaKey);

                case "xor":
                    return XORDecrypt(text, key);

                default:
                    throw new ArgumentException("Неизвестный алгоритм");
            }
        }

        private string NormalizeString(string input)
        {
            return input.Trim().ToUpper();
        }

        private string CaesarEncrypt(string text, int shift)
        {
            const string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
            string result = "";
            foreach (char c in text)
            {
                if (alphabet.Contains(c))
                {
                    int index = (alphabet.IndexOf(c) + shift) % alphabet.Length;
                    if (index < 0) index += alphabet.Length;
                    result += alphabet[index];
                }
                else
                {
                    result += c;
                }
            }
            return result;
        }

        private string CaesarDecrypt(string text, int shift)
        {
            return CaesarEncrypt(text, -shift);
        }

        private string VigenereEncrypt(string text, string key)
        {
            const string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
            string result = "";
            int keyIndex = 0;
            foreach (char c in text)
            {
                if (alphabet.Contains(c))
                {
                    int textIndex = alphabet.IndexOf(c);
                    int keyChar = alphabet.IndexOf(key[keyIndex % key.Length]);
                    int newIndex = (textIndex + keyChar) % alphabet.Length;
                    result += alphabet[newIndex];
                    keyIndex++;
                }
                else
                {
                    result += c;
                }
            }
            return result;
        }

        private string VigenereDecrypt(string text, string key)
        {
            const string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
            string result = "";
            int keyIndex = 0;
            foreach (char c in text)
            {
                if (alphabet.Contains(c))
                {
                    int textIndex = alphabet.IndexOf(c);
                    int keyChar = alphabet.IndexOf(key[keyIndex % key.Length]);
                    int newIndex = (textIndex - keyChar + alphabet.Length) % alphabet.Length;
                    result += alphabet[newIndex];
                    keyIndex++;
                }
                else
                {
                    result += c;
                }
            }
            return result;
        }

        private string PlayfairEncrypt(string text, string key)
        {
            const string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ .,";
            string result = "";
            char[,] matrix = CreatePlayfairMatrix(key);

            for (int i = 0; i < text.Length; i += 2)
            {
                if (i + 1 >= text.Length)
                {
                    text += ' ';
                }

                char a = text[i];
                char b = text[i + 1];

                if (a == b)
                {
                    b = ' ';
                    i--;
                }

                int[] posA = FindPosition(matrix, a);
                int[] posB = FindPosition(matrix, b);

                if (posA[0] == -1 || posB[0] == -1)
                {
                    result += a;
                    result += b;
                    continue;
                }

                if (posA[0] == posB[0])
                {
                    result += matrix[posA[0], (posA[1] + 1) % 6];
                    result += matrix[posB[0], (posB[1] + 1) % 6];
                }
                else if (posA[1] == posB[1])
                {
                    result += matrix[(posA[0] + 1) % 6, posA[1]];
                    result += matrix[(posB[0] + 1) % 6, posB[1]];
                }
                else
                {
                    result += matrix[posA[0], posB[1]];
                    result += matrix[posB[0], posA[1]];
                }
            }

            return result;
        }

        private string PlayfairDecrypt(string text, string key)
        {
            const string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ .,";
            string result = "";
            char[,] matrix = CreatePlayfairMatrix(key);

            for (int i = 0; i < text.Length; i += 2)
            {
                if (i + 1 >= text.Length)
                {
                    result += text[i];
                    break;
                }

                char a = text[i];
                char b = text[i + 1];

                int[] posA = FindPosition(matrix, a);
                int[] posB = FindPosition(matrix, b);

                if (posA[0] == -1 || posB[0] == -1)
                {
                    result += a;
                    result += b;
                    continue;
                }

                if (posA[0] == posB[0])
                {
                    result += matrix[posA[0], (posA[1] + 5) % 6];
                    result += matrix[posB[0], (posB[1] + 5) % 6];
                }
                else if (posA[1] == posB[1])
                {
                    result += matrix[(posA[0] + 5) % 6, posA[1]];
                    result += matrix[(posB[0] + 5) % 6, posB[1]];
                }
                else
                {
                    result += matrix[posA[0], posB[1]];
                    result += matrix[posB[0], posA[1]];
                }
            }

            return result;
        }

        private char[,] CreatePlayfairMatrix(string key)
        {
            const string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ .,";
            char[,] matrix = new char[6, 6];
            string keyString = key + alphabet;
            keyString = new string(keyString.Distinct().ToArray());
            keyString = keyString.Replace("Ё", "Е").Replace("Й", "И").Replace("Ъ", "Ь");

            int index = 0;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (index < keyString.Length)
                    {
                        matrix[i, j] = keyString[index++];
                    }
                }
            }

            return matrix;
        }

        private int[] FindPosition(char[,] matrix, char c)
        {
            c = char.ToUpper(c);
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (matrix[i, j] == c)
                    {
                        return new int[] { i, j };
                    }
                }
            }
            return new int[] { -1, -1 };
        }

        private int[,] ParseHillKey(string key)
        {
            var numbers = key.Split(',').Select(int.Parse).ToArray();
            if (numbers.Length != 4)
            {
                throw new ArgumentException("Для шифра Хилла нужно 4 числа для матрицы 2x2");
            }

            return new int[,]
            {
                { numbers[0], numbers[1] },
                { numbers[2], numbers[3] }
            };
        }

        private string HillEncrypt(string text, int[,] key)
        {
            const string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
            string result = "";

            for (int i = 0; i < text.Length; i += 2)
            {
                if (i + 1 >= text.Length)
                {
                    result += text[i];
                    break;
                }

                int a = alphabet.IndexOf(char.ToUpper(text[i]));
                int b = alphabet.IndexOf(char.ToUpper(text[i + 1]));

                if (a == -1 || b == -1)
                {
                    result += text[i];
                    result += text[i + 1];
                    continue;
                }

                int c = (key[0, 0] * a + key[0, 1] * b) % alphabet.Length;
                int d = (key[1, 0] * a + key[1, 1] * b) % alphabet.Length;

                if (c < 0) c += alphabet.Length;
                if (d < 0) d += alphabet.Length;

                result += alphabet[c];
                result += alphabet[d];
            }

            return result;
        }

        private string HillDecrypt(string text, int[,] key)
        {
            const string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
            string result = "";

            int det = key[0, 0] * key[1, 1] - key[0, 1] * key[1, 0];
            if (det < 0) det += alphabet.Length;
            det = det % alphabet.Length;

            int detInv = ModInverse(det, alphabet.Length);

            int[,] invKey = new int[,]
            {
                { (key[1, 1] * detInv) % alphabet.Length, (-key[0, 1] * detInv) % alphabet.Length },
                { (-key[1, 0] * detInv) % alphabet.Length, (key[0, 0] * detInv) % alphabet.Length }
            };

            // Нормализация элементов обратной матрицы
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (invKey[i, j] < 0)
                    {
                        invKey[i, j] += alphabet.Length;
                    }
                }
            }

            for (int i = 0; i < text.Length; i += 2)
            {
                if (i + 1 >= text.Length)
                {
                    result += text[i];
                    break;
                }

                int a = alphabet.IndexOf(char.ToUpper(text[i]));
                int b = alphabet.IndexOf(char.ToUpper(text[i + 1]));

                if (a == -1 || b == -1)
                {
                    result += text[i];
                    result += text[i + 1];
                    continue;
                }

                int c = (invKey[0, 0] * a + invKey[0, 1] * b) % alphabet.Length;
                int d = (invKey[1, 0] * a + invKey[1, 1] * b) % alphabet.Length;

                if (c < 0) c += alphabet.Length;
                if (d < 0) d += alphabet.Length;

                result += alphabet[c];
                result += alphabet[d];
            }

            return result;
        }

        private int ModInverse(int a, int m)
        {
            a = a % m;
            if (a < 0) a += m;

            for (int x = 1; x < m; x++)
            {
                if ((a * x) % m == 1)
                {
                    return x;
                }
            }
            throw new ArgumentException("Обратный элемент не существует");
        }

        private (int e, int n, int d) ParseRSAKey(string key)
        {
            var numbers = key.Split(',').Select(int.Parse).ToArray();
            if (numbers.Length != 3)
            {
                throw new ArgumentException("Для RSA нужно 3 числа: e, n, d");
            }
            return (numbers[0], numbers[1], numbers[2]);
        }

        private string RSAEncrypt(string text, (int e, int n, int d) key)
        {
            string result = "";
            foreach (char c in text)
            {
                int m = c - 'А';
                int encrypted = ModPow(m, key.e, key.n);
                result += (char)(encrypted + 'А');
            }
            return result;
        }

        private string RSADecrypt(string text, (int e, int n, int d) key)
        {
            string result = "";
            foreach (char c in text)
            {
                int encrypted = c - 'А';
                int m = ModPow(encrypted, key.d, key.n);
                result += (char)(m + 'А');
            }
            return result;
        }

        private int ModPow(int base_, int exponent, int modulus)
        {
            if (modulus == 1) return 0;
            int result = 1;
            base_ = base_ % modulus;
            while (exponent > 0)
            {
                if ((exponent & 1) == 1)
                {
                    result = (result * base_) % modulus;
                }
                base_ = (base_ * base_) % modulus;
                exponent >>= 1;
            }
            return result;
        }

        private string XOREncrypt(string text, string key)
        {
            string result = "";
            for (int i = 0; i < text.Length; i++)
            {
                result += (char)(text[i] ^ key[i % key.Length]);
            }
            return result;
        }

        private string XORDecrypt(string text, string key)
        {
            return XOREncrypt(text, key);
        }
    }
} 