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
            Windows.UI.ViewManagement.ApplicationView appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.SetPreferredMinSize(new Size(650, 700));
            this.SizeChanged += MainPage_SizeChanged;
            checkNowDate();
        }

        private void checkNowDate()
        {
            if (DateTimeOffset.Now.Day != currencyConverterViewModel.LastUpdateTime.Day)
            {
                UpdateCourseHyperLinkButton.Content = "Обновить курсы";
            }
            else
            {
                UpdateCourseHyperLinkButton.Content = "Актуальные курсы";
            }
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
                Regex regex = new Regex(@"^([1-9]){1}(\d){0,11}$|^((([1-9]){1}(\d){0,11})|((0){0,1}))(\.?)(\d){0,4}$");
                if (regex.IsMatch(buf))
                {
                    sender.Text = buf;
                    CalculateSumString = buf;
                    lastChangeTextBox = 2;
                }
                else
                    sender.Text = CalculateSumString;
            }
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double MainWidth = this.ActualWidth;
            double MainHeight = this.ActualHeight;

            MainPageGrid.Width = MainWidth;
            MainPageGrid.Height = MainHeight;

            MainPageGridFirst.Width = new GridLength(MainWidth * 0.36);
            MainPageGridSecond.Width = new GridLength(MainWidth * 0.64);

            ValuteStackPanel.Width = MainWidth * 0.36;
            ValuteStackPanel.Height = MainHeight;

            CoursesStackPanel.Width = MainWidth * 0.64;
            CoursesStackPanel.Height = MainHeight;

            double widthFactor = 0.75;
            double heightInfoFactor = 0.046;
            double heightTextBoxFactor = 0.075;

            ConvertibleSumInfo.Width = ValuteStackPanel.ActualWidth * widthFactor;
            ConvertibleSumInfo.Height = ValuteStackPanel.ActualHeight * heightInfoFactor;

            ConvertibleSumTextBox.Width = ValuteStackPanel.ActualWidth * widthFactor;
            ConvertibleSumTextBox.Height = ValuteStackPanel.ActualHeight * heightTextBoxFactor;

            ReverseValuteButton.Width = ValuteStackPanel.ActualWidth * 0.2;
            ReverseValuteButton.Height = ValuteStackPanel.ActualHeight * 0.2;
            ReverseValuteButton.Margin = new Thickness(0, ValuteStackPanel.ActualHeight * 0.065, 0, 0);

            ReverseValuteButtonImage.Width = ValuteStackPanel.ActualWidth * 0.2;
            ReverseValuteButtonImage.Height = ValuteStackPanel.ActualHeight * 0.2;

            CalculateSumInfo.Width = ValuteStackPanel.ActualWidth * widthFactor;
            CalculateSumInfo.Height = ValuteStackPanel.ActualHeight * heightInfoFactor;

            CalculateSumTextBox.Width = ValuteStackPanel.ActualWidth * widthFactor;
            CalculateSumTextBox.Height = ValuteStackPanel.ActualHeight * heightTextBoxFactor;

            CurrentCourseAndDateInfo.Width = ValuteStackPanel.ActualWidth;
            CurrentCourseAndDateInfo.Height = ValuteStackPanel.ActualHeight * 0.1;

            UpdateCourseHyperLinkButton.Width = ValuteStackPanel.ActualWidth;
            UpdateCourseHyperLinkButton.Height = ValuteStackPanel.ActualHeight * 0.1;
            UpdateCourseHyperLinkButton.FontSize = UpdateCourseHyperLinkButton.Height * 0.3;

            CoursesValuteTextBlock.Width = CoursesStackPanel.ActualWidth;
            CoursesValuteTextBlock.Height = CoursesStackPanel.ActualHeight * heightInfoFactor;

            CoursesValuteListBox.Width = CoursesStackPanel.ActualWidth * 0.95;
            CoursesValuteListBox.Height = (CoursesStackPanel.ActualHeight - CoursesValuteTextBlock.ActualHeight) * 0.9;

            TextBoxFontSizeChanged(ConvertibleSumTextBox);
            TextBoxFontSizeChanged(CalculateSumTextBox);
            TextBlockFontSizeChanged(ConvertibleSumInfo);
            TextBlockFontSizeChanged(CalculateSumInfo);
            TextBlockFontSizeChanged(CurrentCourseAndDateInfo);
            TextBlockFontSizeChanged(CoursesValuteTextBlock);
        }

        private void TextBoxFontSizeChanged(TextBox textBox)
        {
            textBox.FontSize = textBox.Height * 0.4;
        }

        private void TextBlockFontSizeChanged(TextBlock textBlock)
        {
            if (textBlock.Height * 0.45 < 22.1)
                textBlock.FontSize = textBlock.Height * 0.45;
        }
    }
}
