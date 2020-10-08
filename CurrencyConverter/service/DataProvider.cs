using CurrencyConverter.interfaces;
using CurrencyConverter.model;

namespace CurrencyConverter.service
{
    class DataProvider : IDataProvider
    {
        IDataProvider _DataProvider;
        
        public DataProvider()
        {
            _DataProvider = new JsonDataProvider();
        }

        public DataProvider(IDataProvider dataProvider)
        {
            _DataProvider = dataProvider;
        }

        public DataModel GetFromLocalFile(string url, out Response response)
        {
            return _DataProvider.GetFromLocalFile(url, out response);
        }
    }
}
