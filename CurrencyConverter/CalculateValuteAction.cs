using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter
{
    class CalculateValuteAction
    {
        private long LongMultiplication(double factor, long lValue, double dValue, out double remainder)
        {
            double dv = Math.Round(factor * dValue, 4);
            long result = (long)(lValue * factor) + (long)Math.Truncate(dv);
            remainder = Math.Round(dv - Math.Truncate(dv), 4);
            return result;
        }

        private double FractMultiplication(double factor, long lValue, double dValue)
        {
            double result = Math.Round(factor * dValue + factor * lValue, 4);
            return result;
        }

        public void Calculate(double Factor, long calcInt, double calcDiv, out long lValue, out double dValue)
        {
            double FactorDiv = Math.Round(Factor - Math.Truncate(Factor), 4);
            double FactorInt = Math.Truncate(Factor);
            double remainder;
            lValue = LongMultiplication(FactorInt, calcInt, calcDiv, out remainder);
            dValue = FractMultiplication(FactorDiv, calcInt, calcDiv) + remainder;
            lValue += (long)Math.Truncate(dValue);
            dValue = Math.Round(dValue - Math.Truncate(dValue), 4);
        }
    }
}
