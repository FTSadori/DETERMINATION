using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Sans
{
    internal class BigNumsConverter
    {
        public const double Infinity = 6.66e66;

        public static string GetInPrettyENotation(double num, double beginFrom = 1e6)
        {
            if (num < beginFrom) return num.ToString("0");
            if (num > Infinity) return Double.PositiveInfinity.ToString();
            return num.ToString("0.##e0");
        }
    }
}
