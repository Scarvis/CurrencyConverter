using CurrencyConverter.config;
using CurrencyConverter.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CurrencyConverter.service
{
    class CurrencyConverterCore
    {
        private const string Url = "https://www.cbr-xml-daily.ru/daily_json.js";
        private const string RubString = "Российский рубль RUB";
        private const string RubCharCode = "RUB";
        private readonly string JsonPath = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), "daily_json.json");
        private DataModel JsonData;
        private double CurrentFactor;
        private double CurrentReverseFactor;
        private NetworkModule _NetworkModule;
        private readonly CalculateValuteAction _CalculateValuteAction;
        private readonly ConfigurationModule Configuration;

        public List<ValuteModel> _ValuteModelsList;
        public int _SelectedIndex = 0;
        public CalculateValuteModel ConvertibleValute { private set; get; } = new CalculateValuteModel();
        public CalculateValuteModel ConvertedValute { private set; get; } = new CalculateValuteModel();
        public DateTimeOffset LastUpdateTime { private set; get; }

        public CurrencyConverterCore()
        {
            _NetworkModule = new NetworkModule();
            Configuration = new ConfigurationModule();

            _NetworkModule.SetUrl(Url);
            InitData();
            Init();
        }

        private void InitData()
        {
            if (!getJsonFromLocal())
                UpdateCurrency();
        }

        private void Init()
        {
            try
            {
                CurrentReverseFactor = JsonData.Valute["USD"].Value;
                CurrentFactor = 1 / CurrentReverseFactor;
                _SelectedIndex = JsonData.Valute.Keys.ToList().IndexOf("USD");
                _ValuteModelsList = JsonData.Valute.Values.ToList();
                LastUpdateTime = JsonData.Date;
                ConvertibleValute.Name = RubString;
                ConvertibleValute.CharCode = RubCharCode;
                ConvertedValute.Name = JsonData.Valute["USD"].ToString();
                ConvertedValute.CharCode = "USD";
            }
            catch (Exception e)
            {
                CurrentReverseFactor = 0;
                CurrentFactor = 0;
                _SelectedIndex = 0;
                _ValuteModelsList = new List<ValuteModel>();
                Console.WriteLine(e.Message);
            }
        }

        private void ChangeConvertedValuteInfo(string key)
        {
            try
            {
                ConvertibleValute.Name = JsonData.Valute[key].ToString();
                ConvertibleValute.CharCode = JsonData.Valute[key].CharCode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ChangeCalculatedValuteInfo(string key)
        {
            try
            {
                ConvertedValute.Name = JsonData.Valute[key].ToString();
                ConvertedValute.CharCode = JsonData.Valute[key].CharCode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private bool getJsonFromLocal()
        {
            try
            {
                DataProvider appContext = new DataProvider(new JsonDataProvider());
                Response response = new Response();
                JsonData = (JsonModel)appContext.GetFromLocalFile(JsonPath, out response);
                if (JsonData == null)
                {
                    throw new Exception(response.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        private void UpdateCurrency()
        {
            JsonData = JsonConvert.DeserializeObject<JsonModel>(_NetworkModule.GetJson());
        }

        private void SetFactor(double factor, double reverseFactor)
        {
            CurrentFactor = factor;
            CurrentReverseFactor = reverseFactor;
        }

        private CalculateValuteModel Calculate(double Factor, CalculateValuteModel calcSum)
        {
            return _CalculateValuteAction.GetCalculateValute(calcSum, Factor);
            //CalculateValuteAction.Calculate(Factor, calcSum.IntSum, calcSum.divSumToDouble(), out long calcInt, out short calcDiv);
        }

        private short doubleToShort(double d)
        {
            string buf = Math.Round(d, 4).ToString();
            if (!buf.Contains(',')) return 0;
            short result = 0;
            short k = 10000;
            for (int i = buf.Length - 1, n = 1; i >= 0; i--, k /= 10, n *= 10)
            {
                if (buf[i].Equals(',')) break;
                result += (short)(int.Parse(buf[i].ToString()) * n);
            }
            return (short)(result * k);
        }

        public void SetIndexCalculateValute(int index)
        {
            try
            {
                string key = JsonData.Valute.Keys.ToList()[index];
                double value = JsonData.Valute[key].Value / JsonData.Valute[key].Nominal;
                if (value.Equals(0.0)) value = 1.0;

                if (ConvertedValute.Name.Equals(RubString))
                {
                    ChangeConvertedValuteInfo(key);
                    SetFactor(value, 1 / value);
                    CalculateConvertibleValute();
                }
                else if (ConvertibleValute.Name.Equals(RubString))
                {
                    ChangeCalculatedValuteInfo(key);
                    SetFactor(1 / value, value);
                    CalculateCalculateValute();
                }
                _SelectedIndex = index;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void CalculateConvertibleValute()
        {
            ConvertedValute.SetValue(Calculate(CurrentFactor, ConvertibleValute));
        }

        public void CalculateCalculateValute()
        {
            ConvertibleValute.SetValue(Calculate(CurrentReverseFactor, ConvertedValute));
        }

        public void UpdateCourses()
        {
            DateTimeOffset curDate = DateTimeOffset.Now;
            if (curDate.Day != LastUpdateTime.Day)
            {
                UpdateCurrency();
                LastUpdateTime = JsonData.Date;
                SetIndexCalculateValute(_SelectedIndex);
            }
        }

        public void ReverseValute()
        {
            CalculateValuteModel bufValute = new CalculateValuteModel(ConvertibleValute);
            ConvertibleValute = ConvertedValute;
            ConvertedValute = bufValute;

            var bufFactor = CurrentFactor;
            CurrentFactor = CurrentReverseFactor;
            CurrentReverseFactor = bufFactor;
        }

        public string ConvertibleValuteSumString
        {
            get
            {
                return ConvertibleValute.ToString();
            }
            set
            {
                ConvertibleValute.SetValueFromStr(value);
                CalculateConvertibleValute();
            }
        }

        public string CalculateValuteSumString
        {
            get
            {
                return ConvertedValute.ToString();
            }
            set
            {
                ConvertedValute.SetValueFromStr(value);
                CalculateCalculateValute();
            }
        }

        public string ConvertibleValuteInfo
        {
            get
            {
                return ConvertibleValute.Name;
            }
        }

        public string CalculateValuteInfo
        {
            get
            {
                return ConvertedValute.Name;
            }
        }

        public List<ValuteModel> ValuteModelsList
        {
            get
            {
                return _ValuteModelsList;
            }
        }

        public string CurrentCourseValutesInfo
        {
            get
            {
                string key = JsonData.Valute.Keys.ToList()[_SelectedIndex];
                string leftKey, rightKey;
                if (ConvertibleValuteInfo.Contains("RUB"))
                {
                    leftKey = "RUB";
                    rightKey = key;
                }
                else
                {
                    leftKey = key;
                    rightKey = "RUB";
                }
                return string.Format("1 {0} = {1} {2}\nОбновлено {3}",
                    leftKey, Math.Round(CurrentFactor, 4).ToString(), rightKey, LastUpdateTime.ToString());
            }
        }

        public string CurrentCourseValutesInfoShort
        {
            get
            {
                string key = ConvertibleValute.CharCode;
                return string.Format("{0} {1}\n{2}",
                    Math.Round(CurrentFactor, 4).ToString(), key, LastUpdateTime.Date.ToString("d"));
            }
        }
    }
}
