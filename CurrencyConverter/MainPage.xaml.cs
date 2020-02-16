using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace CurrencyConverter
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        CurrencyConverterViewModel currencyConverterViewModel = new CurrencyConverterViewModel();
        string ConvertedSumString;
        string CalculateSumString;
        int lastChangeTextBox = 3;
        public MainPage()
        {
            DataContext = currencyConverterViewModel;
            this.InitializeComponent();
        }

        private void ReverseValuteButton_Click(object sender, RoutedEventArgs e)
        {
            currencyConverterViewModel.ReverseValute();
        }

        private void TextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (lastChangeTextBox == 2)
            {
                lastChangeTextBox = 3;
                return;
            }
            if (sender.Text.Length > 0)
            {
                ConvertedSumString = sender.Text.Replace(',', '.'); 
            }
        }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (sender.Text.Length > 0)
            {
                string buf = sender.Text.Replace(',', '.');
                Regex regex = new Regex(@"^([1-9]){1}(\d){0,11}$|^((([1-9]){1}(\d){0,11})|((0){0,1}))(\.?)(\d){0,4}$");
                if (regex.IsMatch(buf))
                {
                    sender.Text = buf;
                    lastChangeTextBox = 1;
                }
                else
                    sender.Text = ConvertedSumString;
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            currencyConverterViewModel.UpdateCourses();
        }

        private void CalculateSumTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (lastChangeTextBox == 1)
            {
                lastChangeTextBox = 3;
                return;
            }
            if (sender.Text.Length > 0)
            {
                CalculateSumString = sender.Text.Replace(',', '.');
            }
        }

        private void CalculateSumTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (sender.Text.Length > 0)
            {
                string buf = sender.Text.Replace(',', '.');
                Regex regex = new Regex(@"^([1-9]){1}(\d){0,11}$|^((([1-9]){1}(\d){0,11})|((0){0,1}))(\.?)(\d){0,4}$");
                if (regex.IsMatch(buf))
                {
                    sender.Text = buf;
                    lastChangeTextBox = 2;
                }
                else
                    sender.Text = CalculateSumString;
            }
        }
    }
}
