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
        private double CurrentFactor;
        private double CurrentReverseFactor;
        private NetworkModule _NetworkModule;
        private readonly CalculateValuteAction _CalculateValuteAction;
        private readonly ConfigurationModule Configuration;

        //public List<ValuteModel> _ValuteModelsList;
        public List<ValuteModel> ValuteDataList;
        public int _SelectedIndex = 0;
        public CalculateValuteModel ConvertibleValute { private set; get; } = new CalculateValuteModel();
        public CalculateValuteModel ConvertedValute { private set; get; } = new CalculateValuteModel();
        public DateTimeOffset LastUpdateTime { private set; get; }

        public CurrencyConverterCore()
        {
            _NetworkModule = new NetworkModule();
            Configuration = new ConfigurationModule();
            ValuteDataList = new List<ValuteModel>();

            _NetworkModule.SetUrl(Url);
            InitData();
            Init();
        }

        private void InitData()
        {
            if (!getJsonFromLocal())
                UpdateCurrency();
        }

        private bool getJsonFromLocal()
        {
            try
            {
                DataProvider appContext = new DataProvider(new JsonDataProvider());
                Response response = new Response();
                DataModel JsonData = appContext.GetFromLocalFile(JsonPath, out response);
                if (JsonData == null)
                {
                    throw new Exception(response.Message);
                }
                Configuration.SetData(JsonData);
                ValuteDataList = JsonData.Valute.Values.ToList();
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
            DataModel JsonData = JsonConvert.DeserializeObject<JsonModel>(_NetworkModule.GetJson());
            ValuteDataList = JsonData.Valute.Values.ToList();
            Configuration.SetData(JsonData);
        }

        private void Init()
        {
            try
            {
                CurrentReverseFactor = Configuration.DefaultValuteToCalculate.Value;
                CurrentFactor = 1 / CurrentReverseFactor;
                _SelectedIndex = Configuration.ValuteModelList.Valute.Keys.ToList().IndexOf(Configuration.DefaultValuteToCalculate.CharCode);
                LastUpdateTime = Configuration.ValuteModelList.Date;
                ConvertibleValute.SetValue(Configuration.DefaultValuteConvertible);
                ConvertedValute.SetValue(Configuration.DefaultValuteToCalculate);
            }
            catch (Exception e)
            {
                CurrentReverseFactor = 0;
                CurrentFactor = 0;
                _SelectedIndex = 0;
                ValuteDataList = new List<ValuteModel>();
                Console.WriteLine(e.Message);
            }
        }

        private void ChangeConvertedValuteInfo(string charCode)
        {
            try
            {
                ConvertibleValute.SetValue(ValuteDataList.Find(valute => valute.CharCode == charCode));
                //ConvertibleValute.Name = JsonData.Valute[key].ToString();
                //ConvertibleValute.CharCode = JsonData.Valute[key].CharCode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ChangeCalculatedValuteInfo(string charCode)
        {
            try
            {
                ConvertedValute.SetValue(ValuteDataList.Find(valute => valute.CharCode == charCode));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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

        public void SetIndexCalculateValute(int index)
        {
            try
            {
                string charCode = ValuteDataList[index].CharCode;
                double value = ValuteDataList[index].Value / ValuteDataList[index].Nominal;
                if (value.Equals(0.0)) value = 1.0;

                if (ConvertedValute.Name.Equals(RubString))
                {
                    ChangeConvertedValuteInfo(charCode);
                    SetFactor(value, 1 / value);
                    CalculateConvertibleValute();
                }
                else if (ConvertibleValute.Name.Equals(RubString))
                {
                    ChangeCalculatedValuteInfo(charCode);
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
                LastUpdateTime = Configuration.ValuteModelList.Date;
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
                return ValuteDataList;
            }
        }

        public string CurrentCourseValutesInfo
        {
            get
            {
                string charCode = ValuteDataList[_SelectedIndex].CharCode;
                string leftKey, rightKey;
                if (ConvertibleValute.Name.Contains(Configuration.DefaultValute.CharCode))
                {
                    leftKey = "RUB";
                    rightKey = charCode;
                }
                else
                {
                    leftKey = charCode;
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
                string charCode = ConvertibleValute.CharCode;
                return string.Format("{0} {1}\n{2}",
                    Math.Round(CurrentFactor, 4).ToString(), charCode, LastUpdateTime.Date.ToString("d"));
            }
        }
    }
}
