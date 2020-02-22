using System;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace CurrencyConverter
{
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
            checkNowDate();
        }

        private void checkNowDate()
        {
            Console.WriteLine(DateTimeOffset.Now.Date);
            if (DateTimeOffset.Now.Day != currencyConverterViewModel.LastUpdateTime.Day)
            {
                UpdateCourseHyperLinkButton.Content = "Обновить курсы";
            }
            else
            {
                UpdateCourseHyperLinkButton.Content = "Актуальные курсы";
            }
        }

        private bool checkStringForLongAndDouble(string value)
        {
            Regex regex = new Regex(@"^([1-9]){1}(\d){0,11}$|^((([1-9]){1}(\d){0,11})|((0){0,1}))(\.?)(\d){0,4}$");
            return regex.IsMatch(value);
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
                if (checkStringForLongAndDouble(buf))
                {
                    sender.Text = buf;
                    ConvertedSumString = buf;
                    lastChangeTextBox = 1;
                }
                else
                    sender.Text = ConvertedSumString;
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            currencyConverterViewModel.UpdateCourses();
            checkNowDate();
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
                if (checkStringForLongAndDouble(buf))
                {
                    sender.Text = buf;
                    CalculateSumString = buf;
                    lastChangeTextBox = 2;
                }
                else
                    sender.Text = CalculateSumString;
            }
        }
    }
}
