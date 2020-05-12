using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NePishiteSetiNa100ballov
{
    public partial class SettingsWindow : Window
    {
        public DataBase db;
        public Settings settings { get; set; } = new Settings();
        private List<Settings> param = new List<Settings>();

        public SettingsWindow(DataBase db)
        {
            this.db = db;
            InitializeComponent();
            SetParams();
        }

        private void SetParams()
        {
            param = db.GetSettings();

            //Получение и выбор IP адресов
            IPAdressComboBox.Items.Add("127.0.0.1");
            foreach (var ip in System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList)
                IPAdressComboBox.Items.Add(ip.ToString());
            bool flag = false;
            foreach (var a in IPAdressComboBox.Items)
                if (a.ToString() == param[1].IPAddress)
                {
                    IPAdressComboBox.SelectedItem = param[1].IPAddress;
                    flag = true;
                    break;
                }
            if (!flag)
                IPAdressComboBox.SelectedItem = param[0].IPAddress;
            //

            CDTextBox.Text = param[1].Cooldown.ToString();
            ConnectionsTextBox.Text = param[1].Connections.ToString();

            settings.Cooldown = param[1].Cooldown;
            settings.Connections = param[1].Connections;
            settings.IPAddress = IPAdressComboBox.SelectedItem.ToString();
        }

        private void AcceptButt_Click(object sender, RoutedEventArgs e)
        {
            settings.IPAddress = IPAdressComboBox.SelectedItem.ToString();
            int a;

            if (!Int32.TryParse(CDTextBox.Text, out a))
            {
                MessageBox.Show("Неверное значение интервала!");
                return;
            }
            else
                settings.Cooldown = a;

            if (!Int32.TryParse(ConnectionsTextBox.Text, out a))
            {
                MessageBox.Show("Неверное значение подключений!");
                return;
            }
            else
                settings.Connections = a;

            db.UpdateSettings(settings);
            this.DialogResult = true;
        }

        private void CancelButt_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void DefaultButt_Click(object sender, RoutedEventArgs e)
        {
            IPAdressComboBox.SelectedItem = param[0].IPAddress.ToString();
            CDTextBox.Text = param[0].Cooldown.ToString();
            ConnectionsTextBox.Text = param[0].Connections.ToString();
        }
    }
}
