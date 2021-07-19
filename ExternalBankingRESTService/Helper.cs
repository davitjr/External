using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalBankingRESTService
{
    public static class Helper
    {
        public static string ConvertAnsiToUnicode(string str)
        {

            if (str == null)
            {
                return str;
            }

            if (!HasAnsiCharacters(str))
            {
                return str;
            }

            string result = "";
            int strLen = str.Length;
            for (int i = 0; i <= str.Length - 1; i++)
            {
                int charCode = (int)str[i];
                char uChar;
                if (charCode >= 178 && charCode <= 253)
                {
                    if (charCode % 2 == 0)
                    {
                        uChar = (char)((charCode - 178) / 2 + 1329);
                    }
                    else
                    {
                        uChar = (char)((charCode - 179) / 2 + 1377);
                    }
                }
                else
                {
                    if (charCode >= 32 && charCode <= 126)
                    {
                        uChar = str[i];
                    }
                    else
                    {
                        switch (charCode)
                        {
                            case 162:
                                uChar = (char)1415;
                                break;
                            case 168:
                                uChar = (char)1415;
                                break;
                            case 176:
                                uChar = (char)1371;
                                break;
                            case 175:
                                uChar = (char)1372;
                                break;
                            case 177:
                                uChar = (char)1374;
                                break;
                            case 170:
                                uChar = (char)1373;
                                break;
                            case 173:
                                uChar = '-';
                                break;
                            case 163:
                                uChar = (char)1417;
                                break;
                            case 169:
                                uChar = '.';
                                break;
                            case 166:
                                uChar = '»'; //187
                                break;
                            case 167:
                                uChar = '«'; //171
                                break;
                            case 164:
                                uChar = ')';
                                break;
                            case 165:
                                uChar = '(';
                                break;
                            case 46:
                                uChar = '.';
                                break;
                            default:
                                uChar = str[i];
                                break;
                        }
                    }
                }
                result += uChar;
            }
            return result;
        }

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