using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace CryptoShool.Web.Services
{
    public interface ICipherService
    {
        string CaesarEncrypt(string text, int shift);
        string CaesarDecrypt(string text, int shift);
        string VigenereEncrypt(string text, string key);
        string VigenereDecrypt(string text, string key);
        string XOREncrypt(string text, string key);
        string XORDecrypt(string text, string key);
        string HillEncrypt(string text, int[,] key);
        string HillDecrypt(string text, int[,] key);
        string RSAEncrypt(string text, (int n, int e) publicKey);
        string RSADecrypt(string text, (int n, int d) privateKey);
        string PlayfairEncrypt(string text, string key);
        string PlayfairDecrypt(string text, string key);
    }

    public class CipherService : ICipherService
    {
        public string CaesarEncrypt(string text, int shift)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    char offset = char.IsUpper(c) ? 'А' : 'а';
                    result.Append((char)(((c - offset + shift) % 32) + offset));
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        public string CaesarDecrypt(string text, int shift)
        {
            return CaesarEncrypt(text, 32 - (shift % 32));
        }

        public string VigenereEncrypt(string text, string key)
        {
            StringBuilder result = new StringBuilder();
            key = key.ToUpper();
            int keyIndex = 0;

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    char offset = char.IsUpper(c) ? 'А' : 'а';
                    int keyChar = key[keyIndex % key.Length] - 'А';
                    result.Append((char)(((c - offset + keyChar) % 32) + offset));
                    keyIndex++;
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        public string VigenereDecrypt(string text, string key)
        {
            StringBuilder result = new StringBuilder();
            key = key.ToUpper();
            int keyIndex = 0;

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    char offset = char.IsUpper(c) ? 'А' : 'а';
                    int keyChar = key[keyIndex % key.Length] - 'А';
                    result.Append((char)(((c - offset - keyChar + 32) % 32) + offset));
                    keyIndex++;
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        public string XOREncrypt(string text, string key)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    char offset = char.IsUpper(text[i]) ? 'А' : 'а';
                    int keyChar = key[i % key.Length] - 'А';
                    int textChar = text[i] - offset;
                    result.Append((char)(((textChar ^ keyChar) % 32) + offset));
                }
                else
                {
                    result.Append(text[i]);
                }
            }
            return result.ToString();
        }

        public string XORDecrypt(string text, string key)
        {
            return XOREncrypt(text, key); // XOR is symmetric
        }

        public string HillEncrypt(string text, int[,] key)
        {
            if (key.GetLength(0) != 2 || key.GetLength(1) != 2)
                throw new ArgumentException("Ключ должен быть матрицей 2x2");

            StringBuilder result = new StringBuilder();
            text = text.ToUpper().Replace(" ", "");

            // Добавляем 'X' в конец, если длина текста нечетная
            if (text.Length % 2 != 0)
                text += "X";

            for (int i = 0; i < text.Length; i += 2)
            {
                int[] block = new int[2];
                block[0] = text[i] - 'А';
                block[1] = text[i + 1] - 'А';

                int[] encrypted = new int[2];
                encrypted[0] = (key[0, 0] * block[0] + key[0, 1] * block[1]) % 32;
                encrypted[1] = (key[1, 0] * block[0] + key[1, 1] * block[1]) % 32;

                result.Append((char)(encrypted[0] + 'А'));
                result.Append((char)(encrypted[1] + 'А'));
            }

            return result.ToString();
        }

        public string HillDecrypt(string text, int[,] key)
        {
            if (key.GetLength(0) != 2 || key.GetLength(1) != 2)
                throw new ArgumentException("Ключ должен быть матрицей 2x2");

            // Вычисляем определитель
            int det = (key[0, 0] * key[1, 1] - key[0, 1] * key[1, 0]) % 32;
            if (det < 0) det += 32;

            // Находим обратный элемент для определителя
            int detInv = 0;
            for (int i = 0; i < 32; i++)
            {
                if ((det * i) % 32 == 1)
                {
                    detInv = i;
                    break;
                }
            }

            // Вычисляем обратную матрицу
            int[,] invKey = new int[2, 2];
            invKey[0, 0] = (key[1, 1] * detInv) % 32;
            invKey[0, 1] = (-key[0, 1] * detInv) % 32;
            invKey[1, 0] = (-key[1, 0] * detInv) % 32;
            invKey[1, 1] = (key[0, 0] * detInv) % 32;

            if (invKey[0, 1] < 0) invKey[0, 1] += 32;
            if (invKey[1, 0] < 0) invKey[1, 0] += 32;

            StringBuilder result = new StringBuilder();
            text = text.ToUpper().Replace(" ", "");

            for (int i = 0; i < text.Length; i += 2)
            {
                int[] block = new int[2];
                block[0] = text[i] - 'А';
                block[1] = text[i + 1] - 'А';

                int[] decrypted = new int[2];
                decrypted[0] = (invKey[0, 0] * block[0] + invKey[0, 1] * block[1]) % 32;
                decrypted[1] = (invKey[1, 0] * block[0] + invKey[1, 1] * block[1]) % 32;

                result.Append((char)(decrypted[0] + 'А'));
                result.Append((char)(decrypted[1] + 'А'));
            }

            return result.ToString();
        }

        public string RSAEncrypt(string text, (int n, int e) publicKey)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in text)
            {
                int m = c;
                int c_encrypted = ModPow(m, publicKey.e, publicKey.n);
                result.Append(c_encrypted.ToString() + " ");
            }
            return result.ToString().Trim();
        }

        public string RSADecrypt(string text, (int n, int d) privateKey)
        {
            StringBuilder result = new StringBuilder();
            string[] numbers = text.Split(' ');
            foreach (string num in numbers)
            {
                if (int.TryParse(num, out int c))
                {
                    int m = ModPow(c, privateKey.d, privateKey.n);
                    result.Append((char)m);
                }
            }
            return result.ToString();
        }

        private int ModPow(int base_, int exponent, int modulus)
        {
            if (modulus == 1) return 0;
            int result = 1;
            base_ = base_ % modulus;
            while (exponent > 0)
            {
                if ((exponent & 1) == 1)
                    result = (result * base_) % modulus;
                base_ = (base_ * base_) % modulus;
                exponent >>= 1;
            }
            return result;
        }

        public string PlayfairEncrypt(string text, string key)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Текст и ключ не могут быть пустыми");
            }

            text = text.ToUpper();
            key = key.ToUpper();

            var table = CreatePlayfairTable(key);
            var result = new StringBuilder();
            var pairs = new List<string>();

            // Разбиваем текст на пары
            for (int i = 0; i < text.Length; i += 2)
            {
                if (i + 1 < text.Length)
                {
                    if (text[i] == text[i + 1])
                    {
                        pairs.Add(text[i].ToString() + '/');
                        i--; // Отступаем назад, чтобы обработать вторую букву в следующей итерации
                    }
                    else
                    {
                        pairs.Add(text.Substring(i, 2));
                    }
                }
                else
                {
                    pairs.Add(text[i].ToString() + '/');
                }
            }

            // Шифруем каждую пару
            foreach (var pair in pairs)
            {
                try
                {
                    var pos1 = FindPosition(table, pair[0]);
                    var pos2 = FindPosition(table, pair[1]);

                    if (pos1.row == pos2.row)
                    {
                        // Если буквы в одной строке
                        result.Append(table[pos1.row, (pos1.col + 1) % 6]);
                        result.Append(table[pos2.row, (pos2.col + 1) % 6]);
                    }
                    else if (pos1.col == pos2.col)
                    {
                        // Если буквы в одном столбце
                        result.Append(table[(pos1.row + 1) % 6, pos1.col]);
                        result.Append(table[(pos2.row + 1) % 6, pos2.col]);
                    }
                    else
                    {
                        // Если буквы образуют прямоугольник
                        result.Append(table[pos1.row, pos2.col]);
                        result.Append(table[pos2.row, pos1.col]);
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Ошибка при шифровании пары '{pair}': {ex.Message}");
                }
            }

            return result.ToString();
        }

        public string PlayfairDecrypt(string text, string key)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Текст и ключ не могут быть пустыми");
            }

            text = text.ToUpper();
            key = key.ToUpper();

            var table = CreatePlayfairTable(key);
            var result = new StringBuilder();
            var pairs = new List<string>();

            // Разбиваем текст на пары
            for (int i = 0; i < text.Length; i += 2)
            {
                if (i + 1 < text.Length)
                {
                    pairs.Add(text.Substring(i, 2));
                }
            }

            // Расшифровываем каждую пару
            foreach (var pair in pairs)
            {
                try
                {
                    var pos1 = FindPosition(table, pair[0]);
                    var pos2 = FindPosition(table, pair[1]);

                    if (pos1.row == pos2.row)
                    {
                        // Если буквы в одной строке
                        result.Append(table[pos1.row, (pos1.col + 5) % 6]);
                        result.Append(table[pos2.row, (pos2.col + 5) % 6]);
                    }
                    else if (pos1.col == pos2.col)
                    {
                        // Если буквы в одном столбце
                        result.Append(table[(pos1.row + 5) % 6, pos1.col]);
                        result.Append(table[(pos2.row + 5) % 6, pos2.col]);
                    }
                    else
                    {
                        // Если буквы образуют прямоугольник
                        result.Append(table[pos1.row, pos2.col]);
                        result.Append(table[pos2.row, pos1.col]);
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Ошибка при расшифровании пары '{pair}': {ex.Message}");
                }
            }

            // Удаляем добавленные символ "/"
            return result.ToString().Replace("/", "");
        }

        private char[,] CreatePlayfairTable(string key)
        {
            var table = new char[6, 6];
            var used = new HashSet<char>();
            var alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ., ";
            int row = 0, col = 0;

            // Добавляем ключ в таблицу
            foreach (var c in key)
            {
                if (!used.Contains(c))
                {
                    table[row, col] = c;
                    used.Add(c);
                    col++;
                    if (col == 6)
                    {
                        col = 0;
                        row++;
                    }
                }
            }

            // Добавляем оставшиеся буквы алфавита
            foreach (var c in alphabet)
            {
                if (!used.Contains(c))
                {
                    table[row, col] = c;
                    used.Add(c);
                    col++;
                    if (col == 6)
                    {
                        col = 0;
                        row++;
                    }
                }
            }

            return table;
        }

        private (int row, int col) FindPosition(char[,] table, char c)
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (table[i, j] == c)
                    {
                        return (i, j);
                    }
                }
            }
            throw new ArgumentException($"Символ '{c}' не найдена в таблице Плейфера");
        }
    }
} 