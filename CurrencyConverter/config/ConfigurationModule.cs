using CurrencyConverter.model;
using CurrencyConverter.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.config
{
    class ConfigurationModule
    {
        private DataProvider _DataProvider = new DataProvider();
        private readonly string DocPath = "1";

        public DataModel ValuteModelList { private set; get; }
        public ValuteModel DefaultValuteToCalculate { private set; get; }
        public ValuteModel DefaultValuteConvertible { private set; get; }
        public ValuteModel DefaultValute { private set; get; }

        public ConfigurationModule()
        {
            DefaultValuteToCalculate = new ValuteModel("R01235", 840, "USD", 1, "Доллар США", 63.4536);
            DefaultValuteConvertible = new ValuteModel("1", 1, "RUB", 1, "Российский рубль", 1);
            DefaultValute = new ValuteModel("1", 1, "RUB", 1, "Российский рубль", 1);
        }

        public void SetData(DataModel valuteModelList)
        {
            ValuteModelList = new DataModel(valuteModelList);
        }

    }
}
