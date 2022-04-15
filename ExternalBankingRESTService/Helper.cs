namespace ExternalBankingRESTService
{
    public static class Helper
    {
        public static bool HasAnsiCharacters(string str)
        {
            bool hasAnsi = false;
            for (int i = 0; i <= str.Length - 1; i++)
            {
                int charCode = (int)str[i];
                if (charCode >= 178 && charCode <= 253 && charCode != 187)
                {
                    hasAnsi = true;
                }
            }

            return hasAnsi;
        }

        public static string ConvertUnicodeToAnsi(string str)
        {
            string result = "";
            if (str == null)
            {
                return result;
            }

            foreach (char c in str)
            {
                int charCode = (int)c;
                char asciiChar;
                if (charCode >= 1329 && charCode <= 1329 + 37)
                {
                    asciiChar = (char)((charCode - 1240) * 2);
                }
                else if (charCode >= 1329 + 48 && charCode <= 1329 + 48 + 37)
                {
                    asciiChar = (char)((charCode - 1288) * 2 + 1);
                }
                else if (char.IsNumber(c))
                {
                    asciiChar = c;
                }
                else if (charCode == 1415) // և
                {
                    asciiChar = (char)168;
                }
                else if (charCode == 1371) // Շեշտ
                {
                    asciiChar = (char)176;
                }
                else if (charCode == 1372) // Բացականչական
                {
                    asciiChar = (char)175;
                }
                else if (charCode == 1374) // Հարցական
                {
                    asciiChar = (char)177;
                }
                else if (charCode == 1373) // Բութ
                {
                    asciiChar = (char)170;
                }
                else if (charCode == 58) // Վերջակետ
                {
                    asciiChar = (char)163;
                }
                else if (charCode == 32) // Բացատ
                {
                    asciiChar = (char)32;
                }
                else if (c == ')')
                {
                    asciiChar = (char)164;
                }
                else if (c == '(')
                {
                    asciiChar = (char)165;
                }
                else
                {
                    asciiChar = c;
                }

                result += asciiChar;
            }

            return result;
        }
    }
}