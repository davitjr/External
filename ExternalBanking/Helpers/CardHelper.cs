using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ExternalBanking.Helpers
{
    public static class CardHelper
    {
        public static string MaskCardNumber(string cardNumber)
        {
            StringBuilder sb = new StringBuilder(cardNumber);
            sb.Remove(4, cardNumber.Length - 8).Insert(4, new string('*', 8));
            return Regex.Replace(sb.ToString(), ".{4}", "$0 ").Trim();
        }
    }
}
