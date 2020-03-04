using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter
{
    class NetworkModule
    {
        private readonly HttpClient _Client = new HttpClient();
        private string _URL;

        private async Task<string> GetJsonString(string Url, HttpClient client)
        {
            string strData = string.Empty;
            HttpResponseMessage response = await client.GetAsync(Url);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    strData = await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return strData;
        }

        public void SetUrl(string url)
        {
            _URL = url;
            _Client.BaseAddress = new Uri(_URL);
        }

        public string GetJson()
        {
            var t = Task.Run(() => GetJsonString(_URL, _Client));
            t.Wait();
            return t.Result;
        }

        public string GetJson(string url)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            var t = Task.Run(() => GetJsonString(url, client));
            t.Wait();
            return t.Result;
        }
    }
}
