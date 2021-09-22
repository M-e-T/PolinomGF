using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;

using GFLib;
using GFLib.FactoryImplementation;
using GFStochasticMono.Model;
using GFLib.Interface;
using GFLib.AbstractFactory;

namespace GFStochasticMono
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IPolynomials Polinom;
        private SaveSettings saveSettings = new SaveSettings();

        private DateTime timeStart;
        private TimeSpan timeAllOne;

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
                int ToBase = int.Parse(ComboBoxBase.Text);
                int Power = Convert.ToInt32(TextBoxPower.Text);
                int PolinomCount = Convert.ToInt32(TextBoxCountPolinom.Text);

                if (Power > 1 && PolinomCount > 0)
                {
                    saveSettings.Power = Power;
                    LabelMessage.Visibility = Visibility.Hidden;
                    LabelCountTest.Content = "Протестированных полиномов: ";
                    LabelTime.Content = "00:00:00:000";
                    ButtonStart.IsEnabled = false;
                    ButtonStop.IsEnabled = true;
                    ListBoxOne.Items.Clear();
                    Polinom = PolynomialsFactory.GetFactoryLiner(ToBase);// new PolynomialsGFLiner(ToBase);
                    Polinom.ProgressEvent += () =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ProgresOne.Value = Polinom.Progress;
                            LabelProgress.Content = Polinom.Progress + "%";
                            string st = "Протестированных полиномов: " + Polinom.CountTesting.ToString("C0");
                            LabelCountTest.Content = st.Remove(st.Length - 1);

                        });
                    };
                    await Polinom.GenerateStohasAsync(Power, PolinomCount);
                    СompleteGenerate();
                }
                if (PolinomCount == 0)
                    LabelMessage.VCC(Visibility.Visible, Brushes.Red);
            }
            catch (FormatException)
            {
                MessageBox.Show("Введите число", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LabelMessage.VCC(Visibility.Hidden, Brushes.Red);
        }
        private void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            Polinom.Cansel();
        }
        public async void СompleteGenerate()
        {
            LabelCountTest.Content = "Протестированных полиномов: ";
            string s = "Протестированных полиномов: " + Polinom.CountTesting.ToString("C0");
            LabelCountTest.Content = s.Remove(s.Length - 1);

            if (RadiButtonSave.IsChecked == true)
            {
                SaveFileWord savefile = new SaveFileWord(saveSettings);
                savefile.ProgressEvent += () =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgresOne.Value = savefile.Progress;
                        LabelProgress.Content = savefile.Progress + "%";
                    });
                };
                if (Polinom.Count > 0)
                {
                    int countSymbol = TextBoxCountSymbol.Text == "" ? int.Parse(TextBoxPower.Text) + 1 : Convert.ToInt32(TextBoxCountSymbol.Text);
                    await savefile.Save(filename, Polinom, countSymbol);
                }
            }
            else
                for (int i = 0; i < Polinom.Count; i++)
                {
                        ListBoxOne.Items.Add(i + 1 + ") --- " + Polinom.ToString(Polinom[i]));
                }

            LabelMessage.VCC(Visibility.Visible, Polinom.IsCansel ? Brushes.Red : Brushes.Green, Polinom.IsCansel ? "Процес отменен!" : "Процесс завершен!");
            timeAllOne = DateTime.Now - timeStart;
            LabelTime.Content = timeAllOne.ToString(@"hh\:mm\:ss\:fff");
            ProgresOne.ProgressDefault();
            ButtonStart.IsEnabled = true;
            ButtonStop.IsEnabled = false;
            LabelProgress.Content = "";
        }
        private void TextBox_Read_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (sender) as TextBox;
            textBox.Text = textBox.Text.TrimStart('0').ToString();
            try
            {
                int power = Convert.ToInt32(TextBoxPower.Text);
                if (power > 2048)
                {
                    TextBoxPower.Text = "2048";
                    LabelMessage.VCC(Visibility.Visible, Brushes.Red, "Максимальная степень 2048");
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
                            RadiButtonShow.IsChecked = true;
                        }
                        if (RadiButtonSave.IsChecked == true && filename == "")
                        {
                            RadiButton_Save_MouseDoubleClick(null, null);
                            if (String.IsNullOrWhiteSpace(TextBoxFilePath.Text))
                                RadiButtonShow.IsChecked = true;
                        }
                        TextBoxFilePath.Text = filename;
                    }
                    break;
            }
        }

        private void RadiButton_Save_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBoxFilePath.Text = "";
            RadiButtonShow.IsChecked = true;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "(*.docx)|*.docx";
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
                    if (filename.EndsWith("docx") == true)
                    {
                        Window window = new SaveWindow(saveSettings);
                        window.Topmost = true;
                        window.ShowDialog();
                    }
                    RadiButtonSave.IsChecked = true;
                }
            }
        }
        private void TextBox_Save_TextChanged(object sender, TextChangedEventArgs e)
        {
            filename = TextBoxFilePath.Text;
        }

        private void Button_Refrech_Click(object sender, RoutedEventArgs e)
        {
            TextBoxPower.Text = "";
            TextBoxCountSymbol.Text = "";
            TextBoxCountPolinom.Text = "";
            TextBoxFilePath.Text = "";
            LabelCountTest.Content = "Протестированных полиномов: ";
            RadiButtonShow.IsChecked = true;
            LabelMessage.Visibility = Visibility.Hidden;
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
