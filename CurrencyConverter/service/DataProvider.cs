using CurrencyConverter.interfaces;
using CurrencyConverter.model;

namespace CurrencyConverter.service
{
    class DataProvider : IDataProvider
    {
        IDataProvider _dataProvider;

        public DataProvider(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public DataModel GetFromLocalFile(string url, out Response response)
        {
            return _dataProvider.GetFromLocalFile(url, out response);
        }
    }
}
