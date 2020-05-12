using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using System.Timers;
using System.Web;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int ID;
        int AnswerCount = 40;
        int timeCounter = 3600;
        TcpClient client;
        IPAddress IPAddres;
        DispatcherTimer timer;
        string Final;
        public MainWindow()
        {
            InitializeComponent();

        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPAddres = IPAddress.Parse(IpTextBox.Text);
                //IPAddres = IPAddress.Parse("127.0.0.1");
            }
            catch (Exception)
            {
                MessageBox.Show("Неверный формат IP адреса");
            }

            

            client = new TcpClient();
            client.BeginConnect(IPAddres, 1081, new AsyncCallback(TODO), client);
            State.Content = "Подключение...";
            ConnectButton.IsEnabled = false;

        }

        async void shoot()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    string message;
                    var stream = client.GetStream();

                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    message = HttpUtility.UrlDecode(builder.ToString(), Encoding.UTF8);
                    var arr = message.Split('/');
                    foreach (var item in arr)
                    {
                        Student one = new Student(item);
                        Dispatcher.Invoke((() => { StunetsList.Items.Add(one); }));
                    }

                }
            });
        }

        void TODO(IAsyncResult res)
        {

            try
            {
                client.EndConnect(res);
                var stream = client.GetStream();
                this.Dispatcher.Invoke(() => { State.Content = "Подключение прошло успешно"; });
                string message;
                message = "призывники";
                message = HttpUtility.UrlEncode(message, Encoding.UTF8);

                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
                {
                    data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    message = HttpUtility.UrlDecode(builder.ToString(), Encoding.UTF8);
                    var arr = message.Split('/');
                    try
                    {

                        foreach (var item in arr)
                        {
                            Student one = new Student(item);
                            Dispatcher.Invoke((() => { StunetsList.Items.Add(one); }));
                        }
                    }
                    catch
                    {

                    }
                    stream.Close();
                    client.Close();
                }
            }
            catch
            {

                this.Dispatcher.Invoke(() =>
                {
                    ConnectButton.IsEnabled = true;
                    State.Content = "Не удалось подключится к серверу";
                });
                client.Close();

            }

        }

        void StartRequest(IAsyncResult res)
        {

            try
            {
                client.EndConnect(res);
                var stream = client.GetStream();
                string message;
                message = "получитьвопросы:"+ID.ToString();
                message = HttpUtility.UrlEncode(message, Encoding.UTF8);

                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
                {
                    data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    message = HttpUtility.UrlDecode(builder.ToString(), Encoding.UTF8);
                    var arr = message.Split('/');
                    try
                    {

                        foreach (var item in arr)
                        {
                            var one = new Quetion(item);
                            Dispatcher.Invoke((() => {
                                QuetionPage two = new QuetionPage(one);
                                Quetions.Items.Add(two);
                            }));
                        }
                    }
                    catch
                    {

                    }
                    stream.Close();
                    client.Close();

                }
            }
            catch
            {

                this.Dispatcher.Invoke(() =>
                {
                    ConnectButton.IsEnabled = true;
                    State.Content = "Не удалось подключится к серверу";
                });
                client.Close();

            }

        }

        void Report(IAsyncResult res)
        {

            try
            {
                client.EndConnect(res);
                var stream = client.GetStream();
                string message;
                Final = HttpUtility.UrlEncode(Final, Encoding.UTF8);
                byte[] data = Encoding.Unicode.GetBytes(Final);
                stream.Write(data, 0, data.Length);
                {
                    data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    message = HttpUtility.UrlDecode(builder.ToString(), Encoding.UTF8);

                    decimal result =decimal.Parse(message);


                    Dispatcher.Invoke(() => {
                        Quetions.Visibility = Visibility.Collapsed;
                        NextQuetionButton.Visibility = Visibility.Collapsed;
                        PrevQuetionButton.Visibility = Visibility.Collapsed;
                        PrevQuetionButton.Visibility = Visibility.Collapsed;
                        GraduteButton.Visibility = Visibility.Collapsed; 
                        GraduteButton.Visibility = Visibility.Collapsed; 
                        CountLabel.Visibility = Visibility.Collapsed; 
                        TimeLabel.Visibility = Visibility.Collapsed;

                        ResultLabel.Content = result.ToString();
                        ResultLabel.Visibility = Visibility.Visible;
                        Leave.Visibility = Visibility.Visible;

                    });
                        stream.Close();
                }
            }
            catch
            {

            }
            finally
            {
                client.Close();
            }

        }

        void ShowORHide(bool flag)
        {
            if (flag)
            {
                StunetsList.Visibility = Visibility.Visible;
                Group.Visibility = Visibility.Visible;
                LastName.Visibility = Visibility.Visible;
                Name.Visibility = Visibility.Visible;
                FatherName.Visibility = Visibility.Visible;
                LastResult.Visibility = Visibility.Visible;
                Start.Visibility = Visibility.Visible;
            }
            else
            {

                StunetsList.Visibility = Visibility.Collapsed;
                Group.Visibility = Visibility.Collapsed;
                LastName.Visibility = Visibility.Collapsed;
                Name.Visibility = Visibility.Collapsed;
                FatherName.Visibility = Visibility.Collapsed;
                LastResult.Visibility = Visibility.Collapsed;
                Start.Visibility = Visibility.Collapsed;
            }
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            //Host.Source
            this.MainControl.SelectedIndex = 1;
            client = new TcpClient();
            client.BeginConnect(IPAddres, 1081, new AsyncCallback(StartRequest), client);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (timeCounter <= 0)
            {
                GraduteButton_Click(null, null);
                timer.Stop();
            }
            else if(timeCounter ==3540&&Quetions.Items.Count!=40)
            {
                MessageBox.Show("Сервер не смог передать все вопросы");
                Close();
            }
            else
            {
                timeCounter--;
                TimeLabel.Content = string.Format("{0}:{1}", timeCounter / 60, timeCounter % 60);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (client != null)
            {
                client.Close();
            }

        }

        private void PrevQuetionButton_Click(object sender, RoutedEventArgs e)
        {
            Quetions.SelectedIndex--;
        }

        private void NextQuetionButton_Click(object sender, RoutedEventArgs e)
        {
            Quetions.SelectedIndex++;
        }

        private void GraduteButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder temp = new StringBuilder("получитьрезультат:"+ID.ToString() + "/");
            for (int i = 0; i < Quetions.Items.Count; i++)
            {
                temp.Append(((QuetionPage)Quetions.Items[i]).GetAnswers());
            }
            temp.Remove(temp.Length - 1, 1);
            Final = temp.ToString();
            client = new TcpClient();
            client.BeginConnect(IPAddres, 1081, new AsyncCallback(Report), client);

        }

        private void Quetions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Quetions.SelectedIndex == 0)
                PrevQuetionButton.IsEnabled = false;
            else
                PrevQuetionButton.IsEnabled = true;
            if (Quetions.SelectedIndex == AnswerCount - 1)
                NextQuetionButton.IsEnabled = false;
            else
                NextQuetionButton.IsEnabled = true;

            CountLabel.Content = string.Format("{0}/{1}", Quetions.SelectedIndex + 1, AnswerCount);
        }

        private void Student_Changed(object sender, SelectionChangedEventArgs e)
        {
            ID = ((Student)StunetsList.SelectedItem).ID;
            Group.Text = "Группа: " + ((Student)StunetsList.SelectedItem).Group;
            LastName.Text = "Фамилия: " + ((Student)StunetsList.SelectedItem).LastName;
            Name.Text = "Имя: " + ((Student)StunetsList.SelectedItem).Name;
            FatherName.Text = "Отчество: " + ((Student)StunetsList.SelectedItem).FatherName;
            LastResult.Text = "Последний результат: " + ((Student)StunetsList.SelectedItem).Result;
            Start.IsEnabled = true;
        }

        private void Leave_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
