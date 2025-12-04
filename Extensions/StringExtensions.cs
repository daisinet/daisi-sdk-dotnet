using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Daisi.SDK.Extensions
{
    public static class StringExtensions
    {
        public static string ToShortNumber(this int number)
        {
            if (number > 1000000)
                return Math.Floor(number / 1000000d).ToString("0.##") + "M";
            else if (number > 1000)
                return Math.Floor(number / 1000d).ToString("0.##") + "K";
            else
                return number.ToString();
        }

        public static string? CleanupAssistantResponse(this string response)
        {
            return response?.Replace("User:", "").Replace("Assistant:", "").Replace("Bot:", "").Replace("Daisi:","");
        }
        public static string RandomNumber(int length = 10)
        {
            return Random(length, true, false);
        }
        public static string Random(int length = 10, bool includeNumbers = true, bool includeLetters = true)
        {
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var numbers = "0123456789";
            string chars = "";
            if (includeNumbers) chars += numbers;
            if (includeLetters) chars += letters;
            string result = "";
            for (int i = 0; i < length; i++)
            {
                var z = System.Random.Shared.Next(chars.Length);
                result += chars[z];
            }
            return result;
        }
        public static string ToUrlString(this string value)
        {
            string pattern = @"[^a-zA-Z0-9]";

            return Regex.Replace(value, pattern, "-");
        }

        public static string ToSnakeCase(this string camelCaseString)
        {
            return string
                .Concat(
                    camelCaseString.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())
                )
                .ToLower();
        }

        public static string RemoveBase64Strings(this string content)
        {
            string pattern = @"(<img[^>]+src=\""data:.*?>)";

            return Regex.Replace(content, pattern, "");
        }

        public static string MaxLength(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return str.Substring(0, Math.Min(str.Length, maxLength));
        }

        public static int GetDeterministicHashCode(this string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];

                    if (i == str.Length - 1)
                    {
                        break;
                    }

                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        public static string CapitalizeFirstLetter(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return $"{str[0].ToString().ToUpper()}{str.Substring(1)}";
        }
    }
}
