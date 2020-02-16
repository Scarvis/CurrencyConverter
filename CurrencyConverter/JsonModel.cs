using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter
{
    class JsonModel
    {
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset PreviousDate { get; set; }
        public string PreviousURL { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public Dictionary<string, ValuteModel> Valute { get; set; }
    }

    class ValuteModel
    {
        public string ID { get; set; }
        public int NumCode { get; set; }
        public string CharCode { get; set; }
        public int Nominal { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", Name, CharCode);
        }
    }
}
