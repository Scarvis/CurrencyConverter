using System;
using System.Linq;

namespace CurrencyConverter
{
    class CalculateValuteAction
    {
        public double FactorCalculate { private set; get; } = 0.0;
        
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

        private CalculateValuteModel Calculate(CalculateValuteModel calculateValute, double factor)
        {
            long calcInt = calculateValute.IntSum;
            double calcDiv = DivSumToDouble(calculateValute.DivSum);

            double FactorDiv = Math.Round(factor - Math.Truncate(factor), 4);
            double FactorInt = Math.Truncate(factor);
            double remainder;
            try
            {
                long lValue = LongMultiplication(FactorInt, calcInt, calcDiv, out remainder);
                double dValue = FractMultiplication(FactorDiv, calcInt, calcDiv) + remainder;
                lValue += (long)Math.Truncate(dValue);
                dValue = Math.Round(dValue - Math.Truncate(dValue), 4);
                return new CalculateValuteModel(lValue, DoubleToShort(dValue));
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return new CalculateValuteModel(0, 0);
            }
        }

        private short DoubleToShort(double d)
        {
            string buf = Math.Round(d, 4).ToString();
            if (!buf.Contains(',')) return 0;
            short result = 0;
            short k = 10000;
            for (int i = buf.Length - 1, n = 1; i >= 0; i--, k /= 10, n *= 10)
            {
                if (buf[i].Equals(',')) break;
                result += (short)(int.Parse(buf[i].ToString()) * n);
            }
            return (short)(result * k);
        }

        private double DivSumToDouble(short divSum)
        {
            short buf = divSum;
            double it = 1.0;
            while (buf > 0)
            {
                buf /= 10;
                it *= 10;
            }
            return (divSum / it);
        }

        public void SetFactor(double factor)
        {
            FactorCalculate = factor;
        }

        public CalculateValuteModel GetCalculateValute(CalculateValuteModel calculateValute)
        {
            return Calculate(calculateValute, FactorCalculate);
        }

        public CalculateValuteModel GetCalculateValute(CalculateValuteModel calculateValute, double factor)
        {
            return Calculate(calculateValute, factor);
        }
    }
}
