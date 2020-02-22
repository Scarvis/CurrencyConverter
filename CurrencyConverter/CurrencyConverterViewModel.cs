using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CurrencyConverter
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
            catch(Exception e)
            {
                IntSum = 0;
                DivSum = 0;
                Console.WriteLine(e.Message);
            }
        }
    }

    class CurrencyConverterCore
    {
        static readonly HttpClient client = new HttpClient();
        private const string Url = "https://www.cbr-xml-daily.ru/daily_json.js";
        private const string RubString = "Российский рубль RUB";
        private const string RubCharCode = "RUB";
        private readonly string JsonPath = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), "daily_json.json");
        private JsonModel JsonData;
        private double CurrentFactor;
        private double CurrentReverseFactor;

        public List<ValuteModel> _ValuteModelsList;
        public int _SelectedIndex = 0;
        public ValuteSum CurrentConvertibleSum;
        public ValuteSum CurrentCalculateSum;
        public DateTimeOffset LastUpdateTime { private set; get; }

        public CurrencyConverterCore()
        {
            client.BaseAddress = new Uri(Url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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
            catch(Exception e)
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

        private async Task<string> GetJsonString()
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

        private void UpdateCurrency()
        {
            var t = Task.Run(() => GetJsonString());
            t.Wait();
            JsonData = JsonConvert.DeserializeObject<JsonModel>(t.Result);
        }

        private void SetFactor(double factor, double reverseFactor)
        {
            CurrentFactor = factor;
            CurrentReverseFactor = reverseFactor;
        }

        private long LongMultiplication(double factor, long lValue, double dValue, out double remainder)
        {
            double dv = Math.Round(factor * dValue, 4);
            long result = (long)(lValue * factor) + (long)Math.Truncate(dv);
            remainder = Math.Round(dv - Math.Truncate(dv), 4);
            return result;
        }

        private double FractMultiplication(double factor, long lValue, double dValue)
        {
            double result = Math.Round(factor * dValue + factor * lValue, 4);
            return result;
        }

        private void FourStepMultiplication(double FactorDiv, double FactorInt, long calcInt, double calcDiv, out long lValue, out double dValue)
        {
            double remainder;
            lValue = LongMultiplication(FactorInt, calcInt, calcDiv, out remainder);
            dValue = FractMultiplication(FactorDiv, calcInt, calcDiv) + remainder;
            lValue += (long)Math.Truncate(dValue);
            dValue = Math.Round(dValue - Math.Truncate(dValue), 4);
        }

        private ValuteSum Calculate(double Factor, ValuteSum calcSum)
        {
            ValuteSum valuteSum = new ValuteSum();
            double FactorDiv = Math.Round(Factor - Math.Truncate(Factor), 4);
            double FactorInt = Math.Truncate(Factor);
            long calcInt;
            double calcDiv;
            FourStepMultiplication(FactorDiv, FactorInt, calcSum.IntSum, calcSum.divSumToDouble(), out calcInt, out calcDiv);

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
            catch(Exception e)
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

    class CurrencyConverterViewModel : INotifyPropertyChanged
    {
        private CurrencyConverterCore currencyConverterCore;

        public int _SelectedIndex = 0;
        public DateTimeOffset LastUpdateTime { private set; get; }
        public event PropertyChangedEventHandler PropertyChanged;

        public CurrencyConverterViewModel()
        {
            currencyConverterCore = new CurrencyConverterCore();
            LastUpdateTime = currencyConverterCore.LastUpdateTime;
            _SelectedIndex = currencyConverterCore._SelectedIndex;
        }

        private void notifyAll()
        {
            NotifyPropertyChanged("SelectedIndex");
            NotifyPropertyChanged("CurrentConvertibleSumString");
            NotifyPropertyChanged("CurrentCalculateSumString");
            NotifyPropertyChanged("ValuteModelsList");
            NotifyPropertyChanged("ConvertibleValuteInfo");
            NotifyPropertyChanged("CalculateValuteInfo");
            NotifyPropertyChanged("CurrentCourseValute");
            NotifyPropertyChanged("CurrentCourseValuteShort");
        }

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ReverseValute()
        {
            currencyConverterCore.ReverseValute();
            notifyAll();
        }

        public void UpdateCourses()
        {
            currencyConverterCore.UpdateCourses();
            notifyAll();
        }

        public string CurrentConvertibleSumString
        {
            get
            {
                return currencyConverterCore.ConvertibleValuteSumString;
            }
            set
            {
                if (value != currencyConverterCore.ConvertibleValuteSumString)
                {
                    currencyConverterCore.ConvertibleValuteSumString = value;
                    NotifyPropertyChanged("CurrentConvertibleSumString");
                    NotifyPropertyChanged("CurrentCalculateSumString");
                }
            }
        }

        public string CurrentCalculateSumString
        {
            get
            {
                return currencyConverterCore.CalculateValuteSumString;
            }
            set
            {
                if (value != currencyConverterCore.CalculateValuteSumString)
                {
                    currencyConverterCore.CalculateValuteSumString = value;
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
                    currencyConverterCore.SetIndexCalculateValute(_SelectedIndex);
                    NotifyPropertyChanged("SelectedIndex");
                    NotifyPropertyChanged("CurrentConvertibleSumString");
                    NotifyPropertyChanged("CurrentCalculateSumString");
                    NotifyPropertyChanged("CurrentCourseValute");
                    NotifyPropertyChanged("CurrentCourseValuteShort");
                }
            }
        }

        public List<ValuteModel> ValuteModelsList
        {
            get
            {
                return currencyConverterCore.ValuteModelsList;
            }
        }

        public string ConvertibleValuteInfo
        {
            get
            {
                return currencyConverterCore.ConvertibleValuteInfo;
            }
        }

        public string CalculateValuteInfo
        {
            get
            {
                return currencyConverterCore.CalculateValuteInfo;
            }
        }

        public string CurrentCourseValute
        {
            get
            {
                return currencyConverterCore.CurrentCourseValutesInfo;
            }
        }

        public string CurrentCourseValuteShort
        {
            get
            {
                return currencyConverterCore.CurrentCourseValutesInfoShort;
            }
        }
    }
}
