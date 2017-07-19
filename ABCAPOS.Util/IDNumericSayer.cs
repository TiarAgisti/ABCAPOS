using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace ABCAPOS.Util
{
    public class IDNumericSayer
    {

        public string DateFormatingInd(DateTime date)
        {
            //date.ToString("ddMMyyyy");
            return date.ToString("dd") + " " + SayMonth(date.Month) + " " + date.Year;
        }

        public string CutLastSevenDigit(string salesOrderList)
        {
            var trimSo = salesOrderList.Replace(" ", "");
            var dataSo = trimSo.Split(',');
            var newStr = "";
            foreach (var datSo in dataSo)
            {
                if (datSo != "")
                    newStr += "*" + datSo.Substring(datSo.Length - 7, 7) + ",";
            }
            return newStr;
        }

        public string SayMonth(int MonthNum)
        {
            String MonthString = "";
            switch (MonthNum)
            {
                case 1:
                    MonthString = "Januari";
                    break;
                case 2:
                    MonthString = "Februari";
                    break;
                case 3:
                    MonthString = "Maret";
                    break;
                case 4:
                    MonthString = "April";
                    break;
                case 5:
                    MonthString = "Mei";
                    break;
                case 6:
                    MonthString = "Juni";
                    break;
                case 7:
                    MonthString = "Juli";
                    break;
                case 8:
                    MonthString = "Agustus";
                    break;
                case 9:
                    MonthString = "September";
                    break;
                case 10:
                    MonthString = "Oktober";
                    break;
                case 11:
                    MonthString = "November";
                    break;
                case 12:
                    MonthString = "Desember";
                    break;
                default:
                    MonthString = "";
                    break;
            }
            return MonthString;
        }

        public decimal Pembulatan(decimal number)
        {
            var length = (number + 1).ToString().Length;
            if (number.ToString().Length > 3)
                return number + ((number.ToString().Substring(length - 4, 3) == "000") ? 1 : 0);
            else
                return number;
        }
        public string Say(decimal number, string currencySay)
        {
            NumberFormatInfo numInfo = CultureInfo.CurrentCulture.NumberFormat;
            string decimalSeparator = numInfo.CurrencyDecimalSeparator;

            string strNumber = number.ToString("N2");

            string strFrontNumber = "";
            string decimalSay = "";

            if (strNumber.Contains(decimalSeparator))
            {
                int index = strNumber.IndexOf(decimalSeparator);
                string strDecimalChunk = strNumber.Substring(index + 1);

                if (Convert.ToInt32(strDecimalChunk) != 0)
                {
                    decimalSay = " koma " + SayChunk(Convert.ToDecimal(strDecimalChunk)) + " cents";
                }

                strFrontNumber = strNumber.Substring(0, index);
            }
            else
                strFrontNumber = number.ToString("N0");

            //if (!string.IsNullOrEmpty(currencySay))
            //    currencySay = currencySay;

            return SayChunk(Convert.ToDecimal(strFrontNumber)) + currencySay + decimalSay;

        }
        private string SayChunk(decimal number)
        {
            string str = number.ToString();

            List<string> list = new List<string>();

            int length = str.Length - 3;
            for (int x = length; x >= 0; x = x - 3)
            {
                if (x <= 0)
                    break;

                str = str.Insert(x, " ");
            }

            string[] chunks = str.Split(new char[] { ' ' });

            string say = "";
            int chunkPos = 0;
            for (int x = chunks.Length - 1; x >= 0; x--)
            {
                say = InterpretChunkSay(chunks[x], chunkPos++) + say;
            }

            return say;
        }

        private string InterpretChunkSay(string chunk, int chunkPos)
        {
            if (chunk.Length == 1)
                chunk = "  " + chunk;
            else if (chunk.Length == 2)
                chunk = " " + chunk;


            string str = "";

            if (chunk[1] == '1')
            {
                if (chunk[2] == '0')
                    str = "sepuluh ";
                else if (chunk[2] == '1')
                    str = "sebelas ";
                else
                    str = InterpretSingle(chunk[2].ToString(), chunk, 2, chunkPos) + "belas ";

                chunk = chunk[0] + "00";
            }

            for (int x = 2; x >= 0; x--)
            {
                if (x >= chunk.Length)
                    continue;

                str = InterpretSingle(chunk.Substring(x, 1), chunk, x, chunkPos) + str;
            }

            if (str != "")
                str += InterpretChunkPosSay(chunkPos);

            return str;
        }

        private string InterpretChunkPosSay(int chunkPos)
        {
            string say = "";

            switch (chunkPos)
            {
                case 1:
                    say = "ribu ";
                    break;
                case 2:
                    say = "juta ";
                    break;
                case 3:
                    say = "milyar ";
                    break;
                case 4:
                    say = "trilyun ";
                    break;
            }

            return say;
        }

        private string InterpretSingle(string single, string chunk, int hundredPos, int chunkPos)
        {
            string str = "";
            switch (single)
            {
                case "1":
                    if (hundredPos == 2 && (chunkPos == 0 || chunkPos > 1 || (chunkPos == 1 && chunk[hundredPos - 1] != ' ')))
                        str = "satu ";
                    else
                        str = "se";
                    break;
                case "2":
                    str = "dua ";
                    break;
                case "3":
                    str = "tiga ";
                    break;
                case "4":
                    str = "empat ";
                    break;
                case "5":
                    str = "lima ";
                    break;
                case "6":
                    str = "enam ";
                    break;
                case "7":
                    str = "tujuh ";
                    break;
                case "8":
                    str = "delapan ";
                    break;
                case "9":
                    str = "sembilan ";
                    break;
            }

            if (str != "")
            {
                if (hundredPos == 0)
                    str += "ratus ";
                else if (hundredPos == 1)
                    str += "puluh ";
            }

            return str;
        }
    }
}
