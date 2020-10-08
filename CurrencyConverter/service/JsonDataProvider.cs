using CurrencyConverter.interfaces;
using CurrencyConverter.model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace CurrencyConverter.service
{
    class JsonDataProvider : IDataProvider
    {
        public DataModel GetFromLocalFile(string url, out Response response)
        {
            response = new Response();
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

                DataModel JsonData = JsonConvert.DeserializeObject<DataModel>(textFromFile.ToString());
                return JsonData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                response.Message = e.Message;
                return null;
            }
        }
    }
}
