using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

using GFClassic.Model;
using GFLib;
using GFLib.Interface;

namespace GFClassic
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IPolinom PolinomClassic;
        public MainWindow()
        {
            InitializeComponent();
        }
        DateTime timeStart;
        TimeSpan timeAllOne;

        private void TextBox_Read_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Start_Click(sender, e);
            }
        }
        private async void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                timeStart = DateTime.Now;
                int LengthPoli = Convert.ToInt32(TextBoxPower.Text);

                if (LengthPoli > 1)
                {
                    LabelMessage.Visibility = Visibility.Hidden;
                    LabelCountPolinom.Content = "Число полиномов: ";
                    LabelTime.Content = "00:00:00:000";
                    ButtonStart.IsEnabled = false;
                    ButtonStop.IsEnabled = true;
                    ListBoxOne.Items.Clear();
                    PolinomClassic = new PolinomClassic(int.Parse(ComboBoxBase.Text));
                    PolinomClassic.ProgressEvent += () =>
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            ProgresOne.Value = PolinomClassic.Progress;
                            LabelProgres.Content = PolinomClassic.Progress + "%";
                        }));
                    };
                    await PolinomClassic.GenerateDetermAsync(LengthPoli);
                    CompleteGenerate();
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Введите число", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            PolinomClassic?.Cansel();
        }
        private async void CompleteGenerate()
        {
            int toBase = int.Parse(ComboBoxBase.Text);
            string t = PolinomClassic.Count.ToString("C0");
            t = t.Remove(t.Length - 1);
            LabelCountPolinom.Content = "Число полиномов: " + t;
            ProgresOne.Value = 100;
            LabelProgres.Content = "100 %";
            if (RadiButtonSave.IsChecked == true)
            {
                SaveFileExcel savefile = new SaveFileExcel();
                savefile.Progres += () =>
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        ProgresOne.Value = savefile.Progress;
                        LabelProgres.Content = savefile.Progress + "%";

                    }));
                };
                await savefile.Save(filename, PolinomClassic.Polynomials);
            }
            else
            await Task.Run(() => {    
                for (int i = 0; i < PolinomClassic.Count; i++)
                {
                    Dispatcher.Invoke(() => { 
                        ListBoxOne.Items.Add(i + 1 + ") --- " + string.Join("", PolinomClassic[i]));
                    });
                }
            });

            LabelMessage.VCC(Visibility.Visible, PolinomClassic.IsCansel ? Brushes.Red : Brushes.Green, PolinomClassic.IsCansel ? "Процес отменен!" : "Процесс завершен!");
            timeAllOne = DateTime.Now - timeStart;
            LabelTime.Content = timeAllOne.ToString(@"hh\:mm\:ss\:fff");
            ProgresOne.ProgressDefault();
            ButtonStart.IsEnabled = true;
            ButtonStop.IsEnabled = false;
            LabelProgres.Content = "";
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
        string filename = "";
        private bool CheckFile(out string exaption)
        {
            exaption = "";
            if (File.Exists(filename))
            {
                try
                {
                    using (var file = File.Open(filename, FileMode.Open))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    exaption = ex.Message;
                    return false;
                }
            }
            return true;
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (sender as RadioButton);
            switch (rb.Name)
            {
                case "RadiButtonSave":
                    {
                        if (CheckFile(out string exaption) == false && filename != "")
                        {
                            MessageBox.Show(exaption, "Ошыбка", MessageBoxButton.OK, MessageBoxImage.Error);
                            filename = "";
                            TextBoxFilePath.Text = filename;
                            RadiButtonShow.IsChecked = true;
                        }
                        if (RadiButtonSave.IsChecked == true && filename == "")
                        {
                            RadiButton_Save_MouseDoubleClick(null, null);
                            if (String.IsNullOrWhiteSpace(TextBoxFilePath.Text))
                                RadiButtonShow.IsChecked = true;
                        }
                    }
                    break;
            }
        }

        private void RadiButton_Save_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBoxFilePath.Text = "";
            RadiButtonShow.IsChecked = true;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "(*.xlsx)|*.xlsx";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                filename = dlg.FileName;
                if (CheckFile(out string exaption) == false)
                {
                    MessageBox.Show(exaption, "Ошыбка", MessageBoxButton.OK, MessageBoxImage.Error);
                    filename = "";
                }
                else
                {
                    RadiButtonSave.IsChecked = true;
                }
            }
        }

        private void TextBox_Save_TextChanged(object sender, TextChangedEventArgs e)
        {
            filename = TextBoxFilePath.Text;
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

