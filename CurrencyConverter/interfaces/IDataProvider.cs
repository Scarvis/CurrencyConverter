using CurrencyConverter.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.interfaces
{
    interface IDataProvider
    {
        public DataModel GetFromLocalFile(string url, out Response response);
    }
}
