using System;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.IO;

using GFStochasticMono.Model;


namespace GFStochasticMono
{
    /// <summary>
    /// Логика взаимодействия для SaveWindow.xaml
    /// </summary>
    public partial class SaveWindow : Window
    {
        private SaveSettings saveSettings;

        private bool convert = true;

        public SaveWindow(SaveSettings settings)
        {
            saveSettings = settings;
            InitializeComponent();
        }
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            saveSettings.TextFont = ComboBox_font.Text;
            saveSettings.TextSize = Convert.ToInt32(ComboBox_Font_Size.Text);
            if (convert == true)
            {
                int.TryParse(ComboBox_CountRow.Text, out saveSettings.CountRow);
                int.TryParse(ComboBox_Сolumn.Text, out saveSettings.CountColum);
                double.TryParse(TextBox_ColumnWidth.Text.Replace('.', ','), out saveSettings.ColumnWidth);
                double.TryParse(TextBox_RowHeight.Text.Replace('.', ','), out saveSettings.RowHeight);
                saveSettings.AutoNumbering = (bool)CheckBox_AutoNumbering.IsChecked;
                new SerializeSettings().SaveSettings(saveSettings);
                this.Close();
            }
        }
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                ComboBox_font.Items.Add(font.Name);
            }
            for (int i = 1; i < 100; i++)
            {
                ComboBox_Font_Size.Items.Add(i);
            }
            SaveSettings saveSettingsSerial;
            if (File.Exists("Setting.xml"))
            {
                saveSettingsSerial = new SerializeSettings().OpenSettings();
                saveSettings.TextFont = saveSettingsSerial.TextFont;
                saveSettings.TextInterval = saveSettingsSerial.TextInterval;
                saveSettings.CountColum = saveSettingsSerial.CountColum;
                saveSettings.CountRow = saveSettingsSerial.CountRow;
                saveSettings.TextSize = saveSettingsSerial.TextSize;
                saveSettings.ColumnWidth = saveSettingsSerial.ColumnWidth;
                saveSettings.RowHeight = saveSettingsSerial.RowHeight;
                saveSettings.AutoNumbering = saveSettingsSerial.AutoNumbering;
            }
            ComboBox_font.SelectedValue = saveSettings.TextFont;
            ComboBox_font.Text = saveSettings.TextFont;

            ComboBox_Font_Size.Text = saveSettings.TextSize.ToString();
            ComboBox_Font_Size.SelectedIndex = saveSettings.TextSize - 1;
            ComboBox_Interval.Text = saveSettings.TextInterval;
            ComboBox_CountRow.Text = saveSettings.CountRow.ToString();
            ComboBox_Сolumn.Text = saveSettings.CountColum.ToString();
            TextBox_ColumnWidth.Text = saveSettings.ColumnWidth.ToString();
            TextBox_RowHeight.Text = saveSettings.RowHeight.ToString();
            CheckBox_AutoNumbering.IsChecked = saveSettings.AutoNumbering;
        }
        private void ComboBox_CountRow_PreviewMouseDown(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Name == "ComboBox_CountRow")
                convert = int.TryParse(textBox.Text, out int res);
            else
                convert = double.TryParse(textBox.Text.Replace('.', ','), out double res);

            if (convert == false)
                textBox.Foreground = System.Windows.Media.Brushes.Red;
            else
                textBox.Foreground = System.Windows.Media.Brushes.Black;
        }

        private void ComboBox_CountRow_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
