using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core
{
    public class Utils
    {
        public static int Round(double number)
        {
            return (int)Math.Round(number);
        }

        public static int Floor(double number)
        {
            return (int)Math.Floor(number);
        }

        public static int Ceiling(double number)
        {
            return (int)Math.Ceiling(number);
        }
    }
}
