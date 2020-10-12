using CurrencyConverter.interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.config
{
    class ApplicationConstants : IApplicationConstants
    {
        private static string ConstantsFileUrl = "";
        private static List<string> ConstantsList;

        public void SetConstantsFileUrl(string url)
        {
            if (Directory.Exists(url))
            {
                ConstantsFileUrl = url;
            }

        }

        public string Tr(string key) 
        {
            if (ConstantsList == null) return "";
            string result = ConstantsList.Find(constant => constant == key);
            return result == null ? "" : result;
        }
    }
}
