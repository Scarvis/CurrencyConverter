using CurrencyConverter.model;
using System;

namespace CurrencyConverter
{
    class CalculateValuteModel : ValuteModel
    {
        public long IntSum { get; set; }
        public double DivSum { get; set; }
        public string Sum
        {
            get
            {
                return string.Format("{0}.{1}", IntSum.ToString(), DivSum.ToString());
            }
        }

        public CalculateValuteModel(long intSum, double divSum)
        {
            IntSum = intSum;
            DivSum = divSum;
        }
        public CalculateValuteModel(CalculateValuteModel valuteSum)
        {
            Copy(valuteSum);
        }

        private void Copy(CalculateValuteModel value)
        {
            IntSum = value.IntSum;
            DivSum = value.DivSum;
            CharCode = value.CharCode;
            ID = value.ID;
            Name = value.Name;
            Nominal = value.Nominal;
            NumCode = value.NumCode;
            Value = value.Value;
        }

        public void setValue(CalculateValuteModel val)
        {
            Copy(val);
        }

        public void SetValueFromStr(string val)
        {
            val.Replace(",", ".");
            if (!val.Contains(".") || val.IndexOf(".") == val.Length - 1)
            {
                if (val.Length == 0)
                {
                    IntSum = 0;
                }
                else
                {
                    IntSum = Convert.ToInt64(val.Trim('.'));
                }
                DivSum = 0;
                return;
            }
            try
            {
                int dotIndex = val.IndexOf(".");
                string l = val.Substring(0, dotIndex);
                string r = val.Substring(dotIndex + 1);
                IntSum = Convert.ToInt64(l);
                DivSum = Convert.ToDouble(r);
            }
            catch (Exception e)
            {
                IntSum = 0;
                DivSum = 0;
                Console.WriteLine(e.Message);
            }
        }
    }
}
