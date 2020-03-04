using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter
{
    interface IDataProvider
    {
        public JsonModel GetFromLocalFile(string url);
    }

    class JsonDataProvider : IDataProvider
    {
        public JsonModel GetFromLocalFile(string url)
        {
            try
            {
                StringBuilder textFromFile = new StringBuilder();
                using (StreamReader stream = new StreamReader(new BufferedStream(File.OpenRead(url), 10 * 1024 * 1024)))
                {
                    string line;
                    while ((line = stream.ReadLine()) != null)
                    {
                        if (line.Length > 0)
                            textFromFile.Append(line);
                    }
                }

                JsonModel JsonData = JsonConvert.DeserializeObject<JsonModel>(textFromFile.ToString());
                return JsonData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }

    class AppContext : IDataProvider
    {
        IDataProvider _dataProvider;

        public AppContext(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }
        
        public JsonModel GetFromLocalFile(string url)
        {
            return _dataProvider.GetFromLocalFile(url);
        }
    }
}
