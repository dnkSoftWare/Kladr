using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KladrService.Loaders
{
  public  static class ExtClasses
    {
        public static string GetPCode(this string value, int typ)
        {
            int[,] typAr = new int[,] {
         {1,2},
         {3,3},
         {6,3},
         {9,3},
         {12,4},
         {16,4}
        };
            return value.Substring(typAr[typ - 1, 0] - 1, typAr[typ - 1, 1]);
        }

        public static string Convert(this string value, Encoding src, Encoding trg)
        {
            Decoder dec = src.GetDecoder();
            byte[] ba = trg.GetBytes(value);
            int len = dec.GetCharCount(ba, 0, ba.Length);
            char[] ca = new char[len];
            dec.GetChars(ba, 0, ba.Length, ca, 0);
            return new string(ca);
        }
        public static string ConvertFrom866(this string value)
        {
            Encoding src = Encoding.GetEncoding(866);
            Encoding trg = Encoding.Default;
            Decoder dec = src.GetDecoder();
            byte[] ba = trg.GetBytes(value);
            int len = dec.GetCharCount(ba, 0, ba.Length);
            char[] ca = new char[len];
            dec.GetChars(ba, 0, ba.Length, ca, 0);
            return new string(ca);
        }

        public static bool Is51or99CODE(this string value)
        {
            return (value.EndsWith("51") || value.EndsWith("99"));
        }

        public static int ActualCode(this string value)
        {
            return int.Parse(value.Substring(value.Length - 2, 2));
        }

        public static byte TypeCode(this string value)
        {
           // byte r = 0;
            int l = 0;
            int j = 0;
            var ca = value.ToCharArray();
            if (ca.Length == 0) return 0;
            else
                l = ca.Length;
            for (int i = 1; i <= l; i++)
            {
                if (ca[l - i].ToString() == "0") j++;
                else break;
            }
            if (j >= 11) return 1;
            if (j >= 8) return 2;
            if (j >= 5) return 3;
            else return 4;
        }
    }
}
