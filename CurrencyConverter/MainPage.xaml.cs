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
            //args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
            //Regex regex = new Regex("[^0-9.-]+");
            //Regex regex = new Regex("[^0-9.-]+");
            //args.Cancel = regex.IsMatch(sender.Text);
        }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            //sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            currencyConverterViewModel.UpdateCourses();
        }
    }
}
