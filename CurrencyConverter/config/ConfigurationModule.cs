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
        private DataProvider DataProvider = new DataProvider(new JsonDataProvider());
        private readonly string DocPath = "1";

        public ValuteModel DefaultValute { private set; get; } = new ValuteModel();

        public ConfigurationModule()
        {
            
        }



    }
}
