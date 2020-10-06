using System;
using System.Collections.Generic;
using System.ComponentModel;
using CurrencyConverter.model;
using CurrencyConverter.service;

namespace CurrencyConverter
{
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
