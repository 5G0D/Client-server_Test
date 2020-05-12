using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NePishiteSetiNa100ballov
{
    public partial class MainWindow : Window
    {

        private List<Student> students = new List<Student>();
        private TcpListenerClass server = new TcpListenerClass();
        private DataBase db = new DataBase();

        private async void UpdateTable()
        {
            await Task.Run(() => { students = db.GetStudentsList(); });
            studentList.ItemsSource = students;
        }

        public MainWindow()
        {
            InitializeComponent();

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool success = false;
            while (!success)
            {
                string password = Authorization();
                await Task.Run(() => { db.Initialize(window, password, ref success); });
            }
            UpdateTable();


            SettingsWindow options = new SettingsWindow(db);
            Settings param = options.settings;
            server.settings = param;
            IPAddressLabel.Content = param.IPAddress;
            options.Close();
        }

        private async void DataWindow_Closing(object sender, EventArgs e)
        {
            await Task.Run(() => { server.Stop(); });
            await Task.Run(() => { db.Close(); });
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateTable();
        }

        private async void saveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите сохранить изменения?", "Сохранение изменений", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await Task.Run(() => { db.UpdateStudents(students); });
                UpdateTable();
            }
        }

        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        int index = ((DataGridRow)vis).GetIndex();
                        if (index < students.Count)
                        {
                            if (MessageBox.Show("Вы уверены, что хотите избавиться от этого студента?", "Удаление студента", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                for (int i = 0; i < students.Count; i++)
                                {
                                    if (students[i].ID == ((Student)studentList.Items[index]).ID)
                                        students.Remove(students[i]);
                                }
                                studentList.ItemsSource = null;
                                studentList.ItemsSource = students;
                            }
                        }
                    }
            }
            catch
            {
                MessageBox.Show("Невозможно произвести данное удаление!\nБудет произведен выход из системы.");
                Environment.Exit(0);
            }     
        }

        private string Authorization()
        {
            PasswordWindow passwordWindow = new PasswordWindow();

            if (passwordWindow.ShowDialog() == true)
                return passwordWindow.Password;

            return null;
        }

        private void lockButton_Click(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                PasswordWindow passwordWindow = new PasswordWindow();
                if (passwordWindow.ShowDialog() == true)
                    if (passwordWindow.Password == db.DBPassword)
                        return;
                    else
                        MessageBox.Show("Неверный пароль");
                else
                    Environment.Exit(0);
            }
        }

        private void onButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (onButton.Toggled) server.Start(onButton, db);
            else server.Stop();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow options = new SettingsWindow(db);
            if (options.ShowDialog() == true)
            {
                Settings param = options.settings;
                server.settings = param;
                IPAddressLabel.Content = param.IPAddress;
            }
        }

        private void ResultComboBoxChanged(object sender, SelectionChangedEventArgs e)
        {
            if (studentList.CurrentItem != null)
                ((Student)studentList.CurrentItem).Result = (decimal)((ComboBox)sender).SelectedItem;
        }

        private void questionButton_Click(object sender, RoutedEventArgs e)
        {
            questionsWindow questions = new questionsWindow(db);
            questions.ShowDialog();
        }
    }

    public class Student
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Group { get; set; }
        public decimal Result { get; set; }
        public int Attempts { get; set; }
        public List<string> Session { get; set; } = new List<string>();
        public int ID { get; set; }
        public List<decimal> Results { get; set; } = new List<decimal>();
    }

    public class Settings
    {
        public int Connections { get; set; }
        public int ID { get; set; }
        public string IPAddress { get; set; }
        public int Cooldown { get; set; }
    }
}
