using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using GFСomparison.Model;

namespace GFСomparison
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ComparisonPolinom Polinom;
        Division divider = new Division();
        DateTime timeStart;
        TimeSpan timeAllOne;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGrid(divider);
            LabelCoutPolinom.Content = "Число лышкив: ";
        }
        private async void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                timeStart = DateTime.Now;
                int Power = Convert.ToInt32(TextBoxPower.Text);
                if (Power > 1)
                {
                    Enable(false);
                    divider.Clear();
                    UpdateGrid(divider);
                    LabelMessage.Visibility = Visibility.Hidden;
                    LabelCoutPolinom.Content = "Число лышкив: ";
                    LabelTime.Content = "00:00:00:000";
                    Polinom = new ComparisonPolinom(int.Parse(ComboboxBase.Text));
                    Polinom.ProgressEvent += () =>
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            ProgressBarProgress.Value = Polinom.Progress;
                            LabelProgress.Content = Polinom.Progress + "%";
                        }));
                    };
                    await Polinom.Generate(Power);
                    CompleteGenerate();
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Введите число", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private List<long> Dividers()
        {
            string st = Regex.Replace(TextBoxDivider.Text, @"\s+", " ");
            List<long> dividers = new List<long>();
            foreach (string value in Regex.Split(st, @"\D+"))
            {
                if (!string.IsNullOrEmpty(value) && value.Length > 1)
                {
                    dividers.Add(int.Parse(value));
                }
            }
            return dividers;
        }
        private void CompleteGenerate()
        {
            int toBase = int.Parse(ComboboxBase.Text);
            var dividers = Dividers();
            divider = new Division(dividers);

            foreach (var polinom in Polinom.PolinomComparison)
            {
                divider.Add(new Item(polinom, toBase));
            }
            LabelCoutPolinom.Content = "Число лышкив: " + divider.Count;
            Button_Divider_Click(null,null);
            LabelMessage.VCC(Visibility.Visible, Polinom.IsCansel ? Brushes.Red : Brushes.Green, Polinom.IsCansel ? "Процес отменен!" : "Процесс завершен!");
            timeAllOne = DateTime.Now - timeStart;
            LabelTime.Content = timeAllOne.ToString(@"hh\:mm\:ss\:fff");
            ProgressBarProgress.ProgressDefault();
            LabelProgress.Content = "";
            Enable(true);
        }   
        private void TextBox_Read_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (sender) as TextBox;
            textBox.Text = textBox.Text.TrimStart('0').ToString();
            try
            {
                int power = Convert.ToInt32(TextBoxPower.Text);
                if (power > 39)
                {
                    TextBoxPower.Text = "39";
                    LabelMessage.VCC(Visibility.Visible, Brushes.Red, "Максимальная степень 39");
                }
                else
                    LabelMessage.VCC(Visibility.Hidden, Brushes.Red);
            }
            catch (FormatException)
            {
                LabelMessage.VCC(Visibility.Visible, Brushes.Red, "Введите число!");
            }
        }
        private void UpdateGrid(Division divider)
        {
            Enable(false);
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn());
            for (int i = 0; i < divider.Dividers.Count; i++)
            {
                dt.Columns.Add(new DataColumn());
            }
            for (int i = 0; i < divider.Count; i++)
            {
                DataRow r = dt.NewRow();
                r[0] = new DataColumn("Остаток", typeof(string));
                r[0] = string.Join("",divider[i].Polinom);
                for (var j = 0; j < divider[i].Remainder.Count; j++)
                {
                    if (divider[i].Remainder[j] == "")
                        r[j + 1] = 0;
                    else
                        r[j + 1] = divider[i].Remainder[j];
                }
                dt.Rows.Add(r);
            }
            LabelCoutPolinom.Content = "Число лышкив: " + divider.Count;
            DataGridPolinom.ItemsSource = dt.DefaultView;

            DataGridPolinom.Columns[0].MinWidth = 112;
            var tbZero = new DataTable();
            tbZero.Columns.Add(new DataColumn("Число нолей", typeof(string)));
            for (int i = 0; i < divider.Dividers.Count; i++)
            {
                tbZero.Columns.Add(new DataColumn());
            }
            DataGridCountZero.ItemsSource = tbZero.DefaultView;
            DataGridPolinom.Columns[0].Header = "Искл.дел ➔";
            for (int i = 1; i < DataGridCountZero.Columns.Count; i++)
            {
                DataGridCountZero.Columns[i].Header = divider.CountZero(i - 1);
                DataGridPolinom.Columns[i].Header = divider.Dividers[i - 1];
            }
            Enable(true);
        }
        private void DataGrid_Polinom_LayoutUpdated(object sender, EventArgs e)
        {
            if (DataGridCountZero.Columns.Count <= 0)
                return;
            DataGridCountZero.Margin = new Thickness(DataGridPolinom.CellsPanelHorizontalOffset + 10, 2, 10, 0);
            for (int i = 0; i < DataGridPolinom.Columns.Count; i++)
            {
                if(i > 0)
                    DataGridPolinom.Columns[i].MinWidth = 70;
                double maxWidth = Math.Max(DataGridPolinom.Columns[i].ActualWidth, DataGridCountZero.Columns[i].ActualWidth);
                DataGridCountZero.Columns[i].Width = maxWidth;
                DataGridPolinom.Columns[i].Width = maxWidth;
            }
        }
        private void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            Polinom?.Cansel();
        }
        private void Button_Divider_Click(object sender, RoutedEventArgs e)
        {
            var dividers = Dividers();
            divider.Dividers = dividers;
            divider.Divide();
            UpdateGrid(divider);
        }
        private void Button_ClearZero_Click(object sender, RoutedEventArgs e)
        {
            divider.RemoveZero();
            UpdateGrid(divider);
        }
        private void Button_Reestablish_Click(object sender, RoutedEventArgs e)
        {
            if (Polinom == null)
                return;
            int toBase = int.Parse(ComboboxBase.Text);
            divider = new Division(Dividers());
            foreach (var polinom in Polinom.PolinomComparison)
            {
                divider.Add(new Item(polinom, toBase));
            }
            Button_Divider_Click(null, null);
            UpdateGrid(divider);
        }
        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            UpdateGrid(new Division());
            TextBoxPower.Text = "";
            TextBoxDivider.Text = "";
            LabelCoutPolinom.Content = "Число лышкив: ";
            LabelTime.Content = "00:00:00:000";
        }
        private void Enable(bool value)
        {
            ButtonStart.IsEnabled = value;
            ButtonStop.IsEnabled = !value;
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
    public static class EX
    {
        public static void VCC(this Label label, Visibility visibility, Brush brush, string st = "")
        {
            label.Visibility = visibility;
            label.Foreground = brush;
            label.Content = st;
        }
        public static void ProgressDefault(this ProgressBar progressBar, long max = 100, long min = 0, long val = 0)
        {
            progressBar.Maximum = max;
            progressBar.Minimum = min;
            progressBar.Value = val;
        }
    }
}
