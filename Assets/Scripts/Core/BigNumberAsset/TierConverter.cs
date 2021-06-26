using System;

namespace Core.BigNumberAsset
{
    internal static class TierConverter
    {
        public static string TierToString(int tier)
        {
            if (tier < 0) throw new ArgumentOutOfRangeException();
            if (tier > 706) throw new NotImplementedException();

            switch (tier)
            {
                case 0:
                    return "";
                case 1:
                    return "k";
                case 2:
                    return "m";
                case 3:
                    return "b";
                case 4:
                    return "t";
                case 5:
                    return "q";
                case 6:
                    return "s";
                case 7:
                    return "o";
                case 8:
                    return "n";
            }

            return GetColumnName(tier - 9);
        }
        
        private static string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];

            value += letters[index % letters.Length];

            return value;
        }
    }
}