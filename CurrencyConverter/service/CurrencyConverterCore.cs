using CurrencyConverter.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CurrencyConverter.service
{
    struct ValuteSum
    {
        public long IntSum { get; set; }
        public short DivSum { get; set; }
        public string ValuteCharCode { get; set; }
        public string ValuteInfo { get; set; }
        public string SumStr
        {
            get
            {
                return string.Format("{0}.{1}", IntSum.ToString(), DivSum.ToString());
            }
        }
        public ValuteSum(long intSum, short divSum)
        {
            IntSum = intSum;
            DivSum = divSum;
            ValuteCharCode = string.Empty;
            ValuteInfo = string.Empty;
        }
        public ValuteSum(ValuteSum valuteSum)
        {
            IntSum = valuteSum.IntSum;
            DivSum = valuteSum.DivSum;
            ValuteCharCode = valuteSum.ValuteCharCode;
            ValuteInfo = valuteSum.ValuteInfo;
        }
        public double divSumToDouble()
        {
            short buf = DivSum;
            double it = 1.0;
            while (buf > 0)
            {
                buf /= 10;
                it *= 10;
            }
            return (DivSum / it);
        }
        public void setValue(ValuteSum val)
        {
            IntSum = val.IntSum;
            DivSum = val.DivSum;
        }
        public void SetValueFromStr(string val)
        {
            val.Replace(",", ".");
            if (!val.Contains(".") || val.IndexOf(".") == val.Length - 1)
            {
                if (val.Length == 0)
                {
                    IntSum = 0;
                }
                else
                {
                    IntSum = Convert.ToInt64(val.Trim('.'));
                }
                DivSum = 0;
                return;
            }
            try
            {
                int dotIndex = val.IndexOf(".");
                string l = val.Substring(0, dotIndex);
                string r = val.Substring(dotIndex + 1);
                IntSum = Convert.ToInt64(l);
                DivSum = Convert.ToInt16(r);
            }
            catch (Exception e)
            {
                IntSum = 0;
                DivSum = 0;
                Console.WriteLine(e.Message);
            }
        }
    }

    class CurrencyConverterCore
    {
        private const string Url = "https://www.cbr-xml-daily.ru/daily_json.js";
        private const string RubString = "Российский рубль RUB";
        private const string RubCharCode = "RUB";
        private readonly string JsonPath = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), "daily_json.json");
        private JsonModel JsonData;
        private double CurrentFactor;
        private double CurrentReverseFactor;
        private CalculateValuteAction calculateValuteAction;
        private NetworkModule networkModule = new NetworkModule();

        public List<ValuteModel> _ValuteModelsList;
        public int _SelectedIndex = 0;
        public ValuteSum CurrentConvertibleSum;
        public ValuteSum CurrentCalculateSum;
        public DateTimeOffset LastUpdateTime { private set; get; }

        public CurrencyConverterCore()
        {
            networkModule.SetUrl(Url);
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
                CurrentConvertibleSum.ValuteInfo = RubString;
                CurrentConvertibleSum.ValuteCharCode = RubCharCode;
                CurrentCalculateSum.ValuteInfo = JsonData.Valute["USD"].ToString();
                CurrentCalculateSum.ValuteCharCode = "USD";
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
                CurrentConvertibleSum.ValuteInfo = JsonData.Valute[key].ToString();
                CurrentConvertibleSum.ValuteCharCode = JsonData.Valute[key].CharCode;
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
                CurrentCalculateSum.ValuteInfo = JsonData.Valute[key].ToString();
                CurrentCalculateSum.ValuteCharCode = JsonData.Valute[key].CharCode;
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
            JsonData = JsonConvert.DeserializeObject<JsonModel>(networkModule.GetJson());
        }

        private void SetFactor(double factor, double reverseFactor)
        {
            CurrentFactor = factor;
            CurrentReverseFactor = reverseFactor;
        }

        private ValuteSum Calculate(double Factor, ValuteSum calcSum)
        {
            ValuteSum valuteSum = new ValuteSum();
            long calcInt;
            double calcDiv;
            calculateValuteAction.Calculate(Factor, calcSum.IntSum, calcSum.divSumToDouble(), out calcInt, out calcDiv);
            valuteSum.IntSum = calcInt;
            valuteSum.DivSum = doubleToShort(calcDiv);
            return valuteSum;
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

                if (CurrentCalculateSum.ValuteInfo.Equals(RubString))
                {
                    ChangeConvertedValuteInfo(key);
                    SetFactor(value, 1 / value);
                    CalculateConvertibleValute();
                }
                else if (CurrentConvertibleSum.ValuteInfo.Equals(RubString))
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
            CurrentCalculateSum.setValue(Calculate(CurrentFactor, CurrentConvertibleSum));
        }

        public void CalculateCalculateValute()
        {
            CurrentConvertibleSum.setValue(Calculate(CurrentReverseFactor, CurrentCalculateSum));
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
            ValuteSum bufValute = new ValuteSum(CurrentConvertibleSum);
            CurrentConvertibleSum = CurrentCalculateSum;
            CurrentCalculateSum = bufValute;

            var bufFactor = CurrentFactor;
            CurrentFactor = CurrentReverseFactor;
            CurrentReverseFactor = bufFactor;
        }

        public string ConvertibleValuteSumString
        {
            get
            {
                return CurrentConvertibleSum.SumStr;
            }
            set
            {
                CurrentConvertibleSum.SetValueFromStr(value);
                CalculateConvertibleValute();
            }
        }

        public string CalculateValuteSumString
        {
            get
            {
                return CurrentCalculateSum.SumStr;
            }
            set
            {
                CurrentCalculateSum.SetValueFromStr(value);
                CalculateCalculateValute();
            }
        }

        public string ConvertibleValuteInfo
        {
            get
            {
                return CurrentConvertibleSum.ValuteInfo;
            }
        }

        public string CalculateValuteInfo
        {
            get
            {
                return CurrentCalculateSum.ValuteInfo;
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
                string key = CurrentConvertibleSum.ValuteCharCode;
                return string.Format("{0} {1}\n{2}",
                    Math.Round(CurrentFactor, 4).ToString(), key, LastUpdateTime.Date.ToString("d"));
            }
        }
    }
}
