using UnityEngine;

public static class StringNormalizer
{
    /// <summary>
    /// Нормализует строку:
    /// - убирает ведущие нули
    /// - удаляет все символы, кроме цифр и латинских букв
    /// - переводит латинские буквы в верхний регистр
    /// </summary>
    public static string Normalize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        string result = "";
        bool leadingZeroPhase = true;

        foreach (char c in input)
        {
            bool isDigit = c >= '0' && c <= '9';
            bool isLatinLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');

            if (!isDigit && !isLatinLetter)
                continue;

            if (leadingZeroPhase)
            {
                if (c == '0')
                    continue;

                leadingZeroPhase = false;
            }

            result += isLatinLetter ? char.ToUpper(c) : c;
        }

        return result;
    }
}
