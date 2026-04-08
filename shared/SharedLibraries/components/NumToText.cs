using System;
using System.Numerics;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharedLibraries.components
{
    public class NumToText
    {
        public static string NumberToText(double number)
        {
            if (number == 0)
                return "нуль";

            StringBuilder sb = new StringBuilder();

            if (number < 0)
            {
                sb.Append("мінус ");
                number = Math.Abs(number);
            }

            int integerPart = (int)number;

            double fractionalPart = Math.Round(number - Math.Truncate(number), 2);
            string fractionalPartStr = fractionalPart.ToString("0.##").TrimStart('0').Replace(",", "").Replace(".", "");

            bool isLeadingZero = fractionalPartStr.Length == 2 && fractionalPartStr.StartsWith("0");
            Console.WriteLine($"\n----------- {fractionalPart} = {fractionalPartStr} \n");

            sb.Append(IntegerToText(integerPart));

            if (fractionalPartStr.Length > 0)
            {
                sb.Append("цілих ");
                if (isLeadingZero)
                {
                    sb.Append(HundredsToText(Convert.ToInt32(fractionalPartStr)));
                }
                else if (Convert.ToInt32(fractionalPartStr) < 10)
                {
                    sb.Append(TensToText(Convert.ToInt32(fractionalPartStr)));
                }
                else if (Convert.ToInt32(fractionalPartStr) >= 10)
                {
                    sb.Append(HundredsToText(Convert.ToInt32(fractionalPartStr)));
                }
            }

            return sb.ToString().Trim();
        }

        private static string TensToText(double num)
        {
            if (num == 1)
            {
                return "одна десята";
            }

            string numberInWords = num switch
            {
                2 => "дві",
                3 => "три",
                4 => "чотири",
                5 => "п'ять",
                6 => "шість",
                7 => "сім",
                8 => "вісім",
                9 => "дев'ять",
                _ => ""
            };

            return numberInWords + " десятих";
        }

        private static string HundredsToText(int num)
        {
            Console.WriteLine($"Hundred to {num}");
            StringBuilder sb = new StringBuilder();

            if (num >= 10 && num <= 19)
            {
                return num switch
                {
                    10 => "десять сотих",
                    11 => "одинадцять сотих",
                    12 => "дванадцять сотих",
                    13 => "тринадцять сотих",
                    14 => "чотирнадцять сотих",
                    15 => "п'ятнадцять сотих",
                    16 => "шістнадцять сотих",
                    17 => "сімнадцять сотих",
                    18 => "вісімнадцять сотих",
                    19 => "дев'ятнадцять сотих",
                    _ => ""
                };
            }

            if (num >= 20)
            {
                sb.Append((num / 10) switch
                {
                    2 => "двадцять ",
                    3 => "тридцять ",
                    4 => "сорок ",
                    5 => "п'ятдесят ",
                    6 => "шістдесят ",
                    7 => "сімдесят ",
                    8 => "вісімдесят ",
                    9 => "дев'яносто ",
                    _ => ""
                });

                num %= 10;
            }

            if (num > 0)
            {
                sb.Append(num switch
                {
                    1 => "одна сота",
                    2 => "дві сотих",
                    3 => "три сотих",
                    4 => "чотири сотих",
                    5 => "п'ять сотих",
                    6 => "шість сотих",
                    7 => "сім сотих",
                    8 => "вісім сотих",
                    9 => "дев'ять сотих",
                    _ => ""
                });
            }

            return sb.ToString().Trim();
        }

        private static string IntegerToText(int number)
        {
            StringBuilder sb = new StringBuilder();

            if (number >= 20000)
            {
                sb.Append((number / 10000) switch
                {
                    2 => "двадцять ",
                    3 => "тридцять ",
                    4 => "сорок ",
                    5 => "п'ятдесят ",
                    6 => "шістдесят ",
                    7 => "сімдесят ",
                    8 => "вісімдесят ",
                    9 => "дев'яносто ",
                    _ => ""
                });

                sb.Append("тисяч ");

                number %= 10000;

                if (number >= 1000)
                {
                    sb.Append((number / 1000) switch
                    {
                        1 => "одна тисяча ",
                        2 => "дві тисячі ",
                        3 => "три тисячі ",
                        4 => "чотири тисячі ",
                        5 => "п'ять тисяч ",
                        6 => "шість тисяч ",
                        7 => "сім тисяч ",
                        8 => "вісім тисяч ",
                        9 => "дев'ять тисяч ",
                        _ => ""
                    });

                    number %= 1000;
                }
            }
            else if (number >= 10000)
            {
                sb.Append((number / 1000) switch
                {
                    10 => "десять тисяч ",
                    11 => "одинадцять тисяч ",
                    12 => "дванадцять тисяч ",
                    13 => "тринадцять тисяч ",
                    14 => "чотирнадцять тисяч ",
                    15 => "п'ятнадцять тисяч ",
                    16 => "шістнадцять тисяч ",
                    17 => "сімнадцять тисяч ",
                    18 => "вісімнадцять тисяч ",
                    19 => "дев'ятнадцять тисяч ",
                    _ => ""
                });

                number %= 1000;
            }
            else if (number >= 1000)
            {
                sb.Append((number / 1000) switch
                {
                    1 => "одна тисяча ",
                    2 => "дві тисячі ",
                    3 => "три тисячі ",
                    4 => "чотири тисячі ",
                    5 => "п'ять тисяч ",
                    6 => "шість тисяч ",
                    7 => "сім тисяч ",
                    8 => "вісім тисяч ",
                    9 => "дев'ять тисяч ",
                    _ => ""
                });

                number %= 1000;
            }

            if (number >= 100)
            {
                sb.Append((number / 100) switch
                {
                    1 => "сто ",
                    2 => "двісті ",
                    3 => "триста ",
                    4 => "чотириста ",
                    5 => "п'ятсот ",
                    6 => "шістсот ",
                    7 => "сімсот ",
                    8 => "вісімсот ",
                    9 => "дев'ятсот ",
                    _ => ""
                });

                number %= 100;
            }

            if (number >= 20)
            {
                sb.Append((number / 10) switch
                {
                    2 => "двадцять ",
                    3 => "тридцять ",
                    4 => "сорок ",
                    5 => "п'ятдесят ",
                    6 => "шістдесят ",
                    7 => "сімдесят ",
                    8 => "вісімдесят ",
                    9 => "дев'яносто ",
                    _ => ""
                });

                number %= 10;
            }

            sb.Append((number >= 20 ? number % 10 : number) switch
            {
                1 => "один",
                2 => "два ",
                3 => "три ",
                4 => "чотири ",
                5 => "п'ять ",
                6 => "шість ",
                7 => "сім ",
                8 => "вісім ",
                9 => "дев'ять ",
                10 => "десять ",
                11 => "одинадцять ",
                12 => "дванадцять ",
                13 => "тринадцять ",
                14 => "чотирнадцять ",
                15 => "п'ятнадцять ",
                16 => "шістнадцять ",
                17 => "сімнадцять ",
                18 => "вісімнадцять ",
                19 => "дев'ятнадцять ",
                _ => ""
            });
            return sb.ToString();
        }

        public static string NumberMonthToText(string date)
        {
            var StringToDate = Convert.ToDateTime(date);
            string dayNumber = StringToDate.Day.ToString("00");
            int monthNumber = StringToDate.Month;
            int yearNumber = StringToDate.Year;
            string[] monthNames = {
            "січня", "лютого", "березня", "квітня", "травня", "червня",
            "липня", "серпня", "вересня", "жовтня", "листопада", "грудня"
            };

            if (monthNumber >= 1 && monthNumber <= 12)
            {
                string fullDate = $"«{dayNumber}» {monthNames[monthNumber - 1]} {yearNumber}";
                return fullDate;
            }
            else
            {
                return "Not corrected month";
            }
        }

        public static DateTime TestDataStrokdii(DateTime date)
        {
            var newDate = date.AddYears(1).AddDays(-1);
            string fullDate = newDate.ToString("dd.MM.yyyy");
            Console.WriteLine("------------------>" + fullDate);
            return newDate;
        }
        public static string TestDataStan(DateTime date)//DateTime
        {
            var StringToDate = Convert.ToDateTime(date);
            int dayNumber = StringToDate.Day;
            int monthNumber = StringToDate.Month;
            int yearNumber = StringToDate.Year;
            string fullDate = $"{dayNumber - 1}.{monthNumber - 1}.{yearNumber + 1}";
            return fullDate;
        }

        public static string SumToText(double num)
        {
            StringBuilder sb = new StringBuilder();

            if (num == 0)
                return "нуль";

            int integerPart = (int)num;
            int fractionalPart = (int)((num * 100) % 100);

            sb.Append($"({IntegerToTextForSum(integerPart)})  грн. ");

            sb.Append(fractionalPart.ToString("D2"));
            sb.Append(" коп.");
            return sb.ToString().Trim();
        }
        public static string IntegerToTextForSum(int num)
        {
            StringBuilder sb = new StringBuilder();

            if (num >= 100000)
            {
                sb.Append((num / 100000) switch
                {
                    1 => "сто ",
                    2 => "двісті ",
                    3 => "триста ",
                    4 => "чотириста ",
                    5 => "п'ятсот ",
                    6 => "шістсот ",
                    7 => "сімсот ",
                    8 => "вісімсот ",
                    9 => "дев'ятсот ",
                    _ => ""
                });

                num %= 100000;
            }

            if (num >= 20000)
            {
                sb.Append((num / 10000) switch
                {
                    2 => "двадцять ",
                    3 => "тридцять ",
                    4 => "сорок ",
                    5 => "п'ятдесят ",
                    6 => "шістдесят ",
                    7 => "сімдесят ",
                    8 => "вісімдесят ",
                    9 => "дев'яносто ",
                    _ => ""
                });

                num %= 10000;

                if (num >= 1000)
                {
                    sb.Append((num / 1000) switch
                    {
                        1 => "одна тисяча ",
                        2 => "дві тисячі ",
                        3 => "три тисячі ",
                        4 => "чотири тисячі ",
                        5 => "п'ять тисяч ",
                        6 => "шість тисяч ",
                        7 => "сім тисяч ",
                        8 => "вісім тисяч ",
                        9 => "дев'ять тисяч ",
                        _ => ""
                    });

                    num %= 1000;
                }
                else
                {
                    sb.Append("тисяч ");
                }
            }
            else if (num >= 10000)
            {
                sb.Append((num / 1000) switch
                {
                    10 => "десять тисяч ",
                    11 => "одинадцять тисяч ",
                    12 => "дванадцять тисяч ",
                    13 => "тринадцять тисяч ",
                    14 => "чотирнадцять тисяч ",
                    15 => "п'ятнадцять тисяч ",
                    16 => "шістнадцять тисяч ",
                    17 => "сімнадцять тисяч ",
                    18 => "вісімнадцять тисяч ",
                    19 => "дев'ятнадцять тисяч ",
                    _ => ""
                });

                num %= 1000;
            }
            else if (num >= 1000)
            {
                sb.Append((num / 1000) switch
                {
                    1 => "одна тисяча ",
                    2 => "дві тисячі ",
                    3 => "три тисячі ",
                    4 => "чотири тисячі ",
                    5 => "п'ять тисяч ",
                    6 => "шість тисяч ",
                    7 => "сім тисяч ",
                    8 => "вісім тисяч ",
                    9 => "дев'ять тисяч ",
                    _ => ""
                });

                num %= 1000;
            }

            if (num >= 100)
            {
                sb.Append((num / 100) switch
                {
                    1 => "сто ",
                    2 => "двісті ",
                    3 => "триста ",
                    4 => "чотириста ",
                    5 => "п'ятсот ",
                    6 => "шістсот ",
                    7 => "сімсот ",
                    8 => "вісімсот ",
                    9 => "дев'ятсот ",
                    _ => ""
                });

                num %= 100;
            }

            if (num >= 20)
            {
                sb.Append((num / 10) switch
                {
                    2 => "двадцять ",
                    3 => "тридцять ",
                    4 => "сорок ",
                    5 => "п'ятдесят ",
                    6 => "шістдесят ",
                    7 => "сімдесят ",
                    8 => "вісімдесят ",
                    9 => "дев'яносто ",
                    _ => ""
                });

                num %= 10;
            }

            sb.Append((num >= 20 ? num % 10 : num) switch
            {
                1 => "одна",
                2 => "дві",
                3 => "три",
                4 => "чотири",
                5 => "п'ять",
                6 => "шість",
                7 => "сім",
                8 => "вісім",
                9 => "дев'ять",
                10 => "десять",
                11 => "одинадцять",
                12 => "дванадцять",
                13 => "тринадцять",
                14 => "чотирнадцять",
                15 => "п'ятнадцять",
                16 => "шістнадцять",
                17 => "сімнадцять",
                18 => "вісімнадцять",
                19 => "дев'ятнадцять",
                _ => ""
            });

            return sb.ToString().Trim();
        }
    }
}
