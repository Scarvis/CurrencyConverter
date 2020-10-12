using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.model
{
    class ValuteModel
    {
        public string ID        { get; set; }
        public int NumCode      { get; set; }
        public string CharCode  { get; set; }
        public int Nominal      { get; set; }
        public string Name      { get; set; }
        public double Value     { get; set; }
        
        public ValuteModel()
        {

        }

        public ValuteModel(string id, int numCode, string charCode, int nominal, string name, double value)
        {
            ID = id;
            NumCode = numCode;
            CharCode = charCode;
            Nominal = nominal;
            Name = name;
            Value = value;
        }

        public ValuteModel(ValuteModel valuteModel)
        {
            SetValue(valuteModel);
        }

        public void SetValue(ValuteModel valuteModel)
        {
            ID = valuteModel.ID;
            NumCode = valuteModel.NumCode;
            CharCode = valuteModel.CharCode;
            Nominal = valuteModel.Nominal;
            Name = valuteModel.Name;
            Value = valuteModel.Value;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Name, CharCode);
        }
    }
}
