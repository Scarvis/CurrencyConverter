using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.model
{
    class DataModel
    {
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset PreviousDate { get; set; }
        public string PreviousURL { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public Dictionary<string, ValuteModel> Valute { get; set; }
    }
}
