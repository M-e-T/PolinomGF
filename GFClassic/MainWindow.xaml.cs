using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

using GFLib.AbstractFactory;
using GFLib.FactoryImplementation;
using GFClassic.Model;

namespace GFClassic
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PolynomialsFactory PolynomialsGF;
        DateTime timeStart;
        TimeSpan timeAllOne;
        public MainWindow()
        {
            InitializeComponent();
        }

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
                    await PolynomialsGF.GenerateDetermAsync(LengthPoli);
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
            PolynomialsGF?.Cansel();
        }
        private async void CompleteGenerate()
        {
            int toBase = int.Parse(ComboBoxBase.Text);
            string t = PolynomialsGF.Count.ToString("C0");
            t = t.Remove(t.Length - 1);
            LabelCountPolinom.Content = "Число полиномов: " + t;
            ProgresOne.Value = 100;
            LabelProgres.Content = "100 %";
            if (RadiButtonSave.IsChecked == true)
            {
                SaveFileExcel savefile = new SaveFileExcel();
                savefile.Progres += () =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgresOne.Value = savefile.Progress;
                        LabelProgres.Content = savefile.Progress + "%";

                    });
                };
                await savefile.Save(filename, PolynomialsGF.Items);
            }
            else
            for (int i = 0; i < PolynomialsGF.Count; i++)
            {
                ListBoxOne.Items.Add(i + 1 + ") --- " + PolynomialsGF.ToString(PolynomialsGF[i]));
            }

            LabelMessage.VCC(Visibility.Visible, PolynomialsGF.IsCansel ? Brushes.Red : Brushes.Green, PolynomialsGF.IsCansel ? "Процес отменен!" : "Процесс завершен!");
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
                int maxPower = PolynomialsGF.MaxPower;
                if (power > maxPower)
                {
                    TextBoxPower.Text = maxPower.ToString();
                    LabelMessage.VCC(Visibility.Visible, Brushes.Red, $"Максимальная степень { maxPower }");
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
            RadioButton rb = sender as RadioButton;
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

        private void ComboBoxBase_DropDownClosed(object sender, EventArgs e)
        {
            PolynomialsGF = PolynomialsFactory.GetFactoryClassic(int.Parse(ComboBoxBase.Text));
            PolynomialsGF.ProgressEvent += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    ProgresOne.Value = PolynomialsGF.Progress;
                    LabelProgres.Content = PolynomialsGF.Progress + "%";
                });
            };
            TextBox_Read_TextChanged(TextBoxPower, null);
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ComboBoxBase_DropDownClosed(null, null);
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

