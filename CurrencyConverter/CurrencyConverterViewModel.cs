using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CurrencyConverter
{
    struct ValuteSum
    {
        public long IntSum { get; set; }
        public short DivSum { get; set; }
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
        }
        public ValuteSum(ValuteSum valuteSum)
        {
            IntSum = valuteSum.IntSum;
            DivSum = valuteSum.DivSum;
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
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    class CurrencyConverterViewModel : INotifyPropertyChanged
    {
        static readonly HttpClient client = new HttpClient();
        private const string Url = "https://www.cbr-xml-daily.ru/daily_json.js";
        private const string RubString = "Российский рубль RUB";
        private readonly string JsonPath = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), "daily_json.json");
        private JsonModel JsonData = new JsonModel();
        private double CurrentFactor;
        private double CurrentReverseFactor;

        public string _ConvertibleValuteInfo = "Российский рубль RUB";
        public string _CalculateValuteInfo = "Доллар США USD";
        public List<ValuteModel> _ValuteModelsList;
        public int _SelectedIndex = 0;
        public ValuteSum CurrentConvertibleSum;
        public ValuteSum CurrentCalculateSum;
        public DateTimeOffset LastUpdateTime { private set; get; }
        public event PropertyChangedEventHandler PropertyChanged;

        public CurrencyConverterViewModel()
        {
            client.BaseAddress = new Uri(Url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!getJsonFromLocal())
                UpdateCurrency();
            Init();
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
            }
            catch(Exception e)
            {
                CurrentReverseFactor = 0;
                CurrentFactor = 0;
                _SelectedIndex = 0;
                _ValuteModelsList = new List<ValuteModel>();
                Console.WriteLine(e.Message);
            }
        }

        private bool getJsonFromLocal()
        {
            try
            {
                StringBuilder textFromFile = new StringBuilder();
                using (StreamReader stream = new StreamReader(new BufferedStream(File.OpenRead(JsonPath), 10 * 1024 * 1024)))
                {
                    string line;
                    while ((line = stream.ReadLine()) != null)
                    {
                        if (line.Length > 0)
                            textFromFile.Append(line);
                    }
                }

                JsonData = JsonConvert.DeserializeObject<JsonModel>(textFromFile.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        static async Task<string> GetJsonString()
        {
            string strData = string.Empty;
            HttpResponseMessage response = await client.GetAsync(Url);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    strData = await response.Content.ReadAsStringAsync();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return strData;
        }

        private void UpdateCurrency()
        {
            var t = Task.Run(() => GetJsonString());
            t.Wait();
            //string outputString = t.Result;
            JsonData = JsonConvert.DeserializeObject<JsonModel>(t.Result);
            //try
            //{
            //    using (StreamWriter sw = File.CreateText(JsonPath))
            //    {
            //        JsonSerializer serializer = new JsonSerializer();
            //        serializer.Serialize(sw, outputString);
            //    }
            //}
            //catch(Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
        }

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetCalculateValute()
        {
            string key = JsonData.Valute.Keys.ToList()[SelectedIndex];
            double value = JsonData.Valute[key].Value / JsonData.Valute[key].Nominal;
            if (value.Equals(0.0)) value = 1.0;
            if (_CalculateValuteInfo.Equals(RubString))
            {
                _ConvertibleValuteInfo = JsonData.Valute[key].ToString();
                CurrentFactor = value;
                CurrentReverseFactor = 1 / value;
                Calculate();
                NotifyPropertyChanged("ConvertibleValuteInfo");
                NotifyPropertyChanged("CurrentConvertibleSumString");
            }
            else if (_ConvertibleValuteInfo.Equals(RubString))
            {
                CurrentReverseFactor = value;
                CurrentFactor = 1 / value;
                _CalculateValuteInfo = JsonData.Valute[key].ToString();
                ReverseCalculate();
                NotifyPropertyChanged("CalculateValuteInfo");
                NotifyPropertyChanged("CurrentCalculateSumString");
            }
        }

        private ValuteSum Calculate(double Factor, ValuteSum calcSum)
        {
            ValuteSum valuteSum = new ValuteSum();
            double FactorDiv = Math.Round(Factor - Math.Truncate(Factor), 4);
            double FactorInt = Math.Truncate(Factor);

            long intSum = (long)(calcSum.IntSum * FactorInt);
            double alpha = Math.Round(FactorInt * calcSum.divSumToDouble(), 4);
            intSum += (long)(Math.Truncate(alpha));
            short divSum = doubleToShort(Math.Round(alpha - Math.Truncate(alpha), 4));

            alpha = Math.Round(FactorDiv * calcSum.IntSum, 4);
            intSum += (long)(Math.Truncate(alpha));
            divSum += doubleToShort(Math.Round(alpha - Math.Truncate(alpha), 4));

            alpha = Math.Round(FactorDiv * calcSum.divSumToDouble(), 4);
            intSum += (long)(Math.Truncate(alpha));
            divSum += doubleToShort(Math.Round(alpha - Math.Truncate(alpha), 4));

            valuteSum.IntSum = intSum;
            valuteSum.DivSum = divSum;
            return valuteSum;
        }

        private void ReverseCalculate()
        {
            CurrentConvertibleSum.setValue(Calculate(CurrentReverseFactor, CurrentCalculateSum));
        }

        private void Calculate()
        {
            CurrentCalculateSum.setValue(Calculate(CurrentFactor, CurrentConvertibleSum));
        }
        
        private short doubleToShort(double d)
        {
            short result = 0;
            string buf = d.ToString();
            for (int i = buf.Length - 1,  n = 1; i >= 0; i--, n*=10)
            {
                if (buf[i].Equals(',') || buf[i].Equals('0')) break;
                result += (short)(int.Parse(buf[i].ToString()) * n);
            }
            return result;
        }

        public void UpdateCourses()
        {
            DateTimeOffset curDate = DateTimeOffset.Now;
            if (curDate.Day != LastUpdateTime.Day)
            {
                UpdateCurrency();
                LastUpdateTime = curDate;
                SetCalculateValute();
                NotifyPropertyChanged("SelectedIndex");
                NotifyPropertyChanged("CurrentConvertibleSumString");
                NotifyPropertyChanged("CurrentCalculateSumString");
                NotifyPropertyChanged("CurrentCourseValute");
            }
        }

        public void ReverseValute()
        {
            ValuteSum bufValute = new ValuteSum(CurrentConvertibleSum.IntSum, CurrentConvertibleSum.DivSum);
            CurrentConvertibleSum = CurrentCalculateSum;
            CurrentCalculateSum = bufValute;

            string bufStr = _CalculateValuteInfo;
            _CalculateValuteInfo = _ConvertibleValuteInfo;
            _ConvertibleValuteInfo = bufStr;

            var bufFactor = CurrentFactor;
            CurrentFactor = CurrentReverseFactor;
            CurrentReverseFactor = bufFactor;

            NotifyPropertyChanged("CurrentConvertibleSumString");
            NotifyPropertyChanged("CurrentCalculateSumString");
            NotifyPropertyChanged("ConvertibleValuteInfo");
            NotifyPropertyChanged("CalculateValuteInfo");
            NotifyPropertyChanged("CurrentCourseValute");
        }

        public string CurrentConvertibleSumString
        {
            get
            {
                return CurrentConvertibleSum.SumStr;
            }
            set
            {
                if (value != this.CurrentConvertibleSum.SumStr)
                {
                    CurrentConvertibleSum.SetValueFromStr(value);
                    Calculate();
                    NotifyPropertyChanged("CurrentConvertibleSumString");
                    NotifyPropertyChanged("CurrentCalculateSumString");
                }
            }
        }

        public string CurrentCalculateSumString
        {
            get
            {
                return CurrentCalculateSum.SumStr; 
            }
            set
            {
                if (value != this.CurrentCalculateSum.SumStr)
                {
                    CurrentCalculateSum.SetValueFromStr(value);
                    ReverseCalculate();
                    NotifyPropertyChanged("CurrentConvertibleSumString");
                    NotifyPropertyChanged("CurrentCalculateSumString");
                }
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                if(value != _SelectedIndex)
                {
                    _SelectedIndex = value;
                    SetCalculateValute();
                    NotifyPropertyChanged("SelectedIndex");
                    NotifyPropertyChanged("CurrentConvertibleSumString");
                    NotifyPropertyChanged("CurrentCalculateSumString");
                    NotifyPropertyChanged("CurrentCourseValute");
                }
            }
        }

        public List<ValuteModel> ValuteModelsList
        {
            get
            {
                return _ValuteModelsList;
            }
        }

        public string ConvertibleValuteInfo
        {
            get
            {
                return _ConvertibleValuteInfo;
            }
        }

        public string CalculateValuteInfo
        {
            get
            {
                return _CalculateValuteInfo;
            }
        }

        public string CurrentCourseValute
        {
            get
            {
                string key = JsonData.Valute.Keys.ToList()[SelectedIndex];
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
    }
}
