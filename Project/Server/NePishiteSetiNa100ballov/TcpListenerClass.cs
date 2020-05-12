using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Web;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Windows.Threading;

namespace NePishiteSetiNa100ballov
{
    class TcpListenerClass
    {
        public Settings settings { get; set; }
        private const int port = 1081;
        private TcpListener server;
        private Thread ListenerThread;
        private DataBase db;
        private int connections = 0;
        private int questions_count = 40;

        public void Start(ToggleButton tb = null, DataBase db = null)
        {
            ListenerThread = new Thread(() =>
            {
                try
                {
                    this.db = db;
                    IPAddress localAddr = IPAddress.Parse(settings.IPAddress);
                    server = new TcpListener(localAddr, port);
                    server.Start();
                    Listener();
                }
                catch {
                    tb.Dispatcher.Invoke(() => { if (tb != null) tb.ChangeToggle(false); });
                }
                finally
                {
                    Thread.CurrentThread.Abort();
                    Stop();
                }
            });
            ListenerThread.Start();
        }

        public void Stop()
        {
            if (server != null)
                server.Stop();
            if (ListenerThread != null)
                ListenerThread.Abort();
        }

        private void Listener()
        {
            while (true)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    (new Thread(() => talkWithClient(client))).Start();  
                }
                catch { }
            }
        }

        private void talkWithClient(TcpClient client)
        {
            connections++;
            NetworkStream stream = client.GetStream();
            try
            {
                if (connections > settings.Connections) return;

                StringBuilder response = new StringBuilder();
                byte[] data = new byte[256];

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    response.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);

                string normalResponse = HttpUtility.UrlDecode(response.ToString(), Encoding.UTF8);

                if (normalResponse.Contains("'")) throw new Exception();

                if (normalResponse == "призывники")
                    SendStudentList(stream);
                if (normalResponse.Contains("получитьвопросы:"))
                {
                    string command = "получитьвопросы:";
                    normalResponse = normalResponse.Remove(normalResponse.IndexOf(command), command.Length);
                    int id;
                    Int32.TryParse(normalResponse, out id);

                    TimeSpan ts = DateTime.Now - db.GetLastSession(id);

                    if (ts.Minutes < settings.Cooldown) return;

                    db.UpdateSession(id);
                    db.UpdateAttempt(id);
                    SendQuestionsString(stream);
                }
                if (normalResponse.Contains("получитьрезультат:"))
                {
                    string command = "получитьрезультат:";
                    int id = 0;
                    decimal result = CalculateResult(normalResponse, command, ref id, 40);
                    if (result != -1)
                    {
                        db.UpdateResult(result, id);
                    }
                    byte[] SendData = HttpUtility.UrlEncodeToBytes(result.ToString(), Encoding.UTF8);
                    stream.Write(SendData, 0, SendData.Length);
                        

                }
            }
            catch { }
            finally
            {
                stream.Close();
                client.Close();
                connections--;
                Thread.CurrentThread.Abort();
            }
        }
        private void SendStudentList(NetworkStream stream)
        {
            try
            {
                List<Student> students = db.GetStudentsList();
                StringBuilder response = new StringBuilder();
                foreach (var student in students)
                {
                    if (student.ID > 0)
                        response.Append($"{student.ID}|{student.LastName}|{student.FirstName}|{student.SecondName}|{student.Group}|{student.Result}/");
                }
                byte[] data = HttpUtility.UrlEncodeToBytes(response.ToString(), Encoding.UTF8);
                stream.Write(data, 0, data.Length);
            }
            catch { }
        }

        private void SendQuestionsString(NetworkStream stream)
        {
            try
            {
                string response = db.GetQuestionsString(); 
                byte[] data = HttpUtility.UrlEncodeToBytes(response, Encoding.UTF8);
                stream.Write(data, 0, data.Length);
            }
            catch
            {
                MessageBox.Show("Ошибка при отправке вопросов");
            }
        }

        private decimal CalculateResult(string str, string command, ref int id, int count = 40)
        {
            try
            {
                List<Question> questions = db.GetAnswers();

                str = str.Remove(str.IndexOf(str), command.Length);
                string[] arr = str.Split('/');

                Int32.TryParse(arr[0], out id);
                decimal result = 0;
                decimal oneMax = (decimal)100 / questions_count;
                //Проход по всем полученным ответам
                for (int i = 1; i < arr.Length; i++)
                {
                    //MessageBox.Show(arr[i]);
                    string[] a = arr[i].Split('|'); //a[0] - код вопроса, остальные - полученные ответы

                    ///////////////////////////////////////////////////////////////////////////////////////////
                    //Очистка массива от пустых значений
                    //Я обожаю эксепшены и всё что с ними связано
                    //Мб этот кусок г***а даже не нужен но раз уж написал то пусть чилит
                    /*List<string> a = new List<string>();
                    foreach (string s in ar)
                    {
                        bool flag = false;
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(s)) flag = true;
                        }
                        catch
                        {
                            flag = false;
                        }
                        if (flag) a.Add(s);
                    }*/
                    ///////////////////////////////////////////////////////////////////////////////////////////

                    //Проход по всем возможным вопросам с их ответами
                    foreach (var q in questions)
                    {
                        //MessageBox.Show(a[0]);
                        //Если находим вопрос с кодом как у полученного кода вопроса
                        if (q.Code.ToString() == a[0])
                        {
                            bool flag1 = false, flag2 = false;
                            //Проходим по всем полученным ответам. С 1, потому что 0 - код
                            for (int d = 1; d < a.Length; d++)
                            {
                                if (!string.IsNullOrWhiteSpace(a[d]))
                                {
                                    if (q.Type == "Несколько ответов" && q.Dop_param == "true")
                                    {
                                        if (q.GetRightCount() < a.Length - 1) break;
                                        //Проходим по ответам (не полученным)
                                        for (int c = 0; c < q.Answer_code.Count; c++)
                                        {
                                            if (a[d] == q.Answer_code[c].ToString())
                                            {
                                                db.IncAnswerStats(q.Code, q.Answer_code[c]);
                                                if (q.SumK != 0)
                                                    result += Math.Round(((q.K[c] / q.SumK) * oneMax), 2);
                                            }
                                        }
                                    }
                                    else if (q.Type == "Ответ текстом")
                                    {
                                        //C учетом регистра
                                        if (q.Dop_param == "true")
                                        {
                                            //Проходим по ответам (не полученным)
                                            for (int c = 0; c < q.Answer_code.Count; c++)
                                            {
                                                if (a[d] == q.Answer[c].ToString())
                                                {
                                                    db.IncAnswerStats(q.Code, q.Answer_code[c]);
                                                    result += Math.Round((oneMax), 2);
                                                    break;
                                                }
                                            }
                                        }
                                        //Без учета регистра
                                        else
                                        {
                                            //Проходим по ответам (не полученным)
                                            for (int c = 0; c < q.Answer_code.Count; c++)
                                            {
                                                if (a[d].ToUpper() == q.Answer[c].ToString().ToUpper())
                                                {
                                                    db.IncAnswerStats(q.Code, q.Answer_code[c]);
                                                    result += Math.Round((oneMax), 2);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    //Для одного ответа или нескольких без коефа
                                    else if (q.Type == "Один ответ" || (q.Type == "Несколько ответов" && q.Dop_param == "false"))
                                    {
                                        flag1 = true;
                                        bool check = false;
                                        //Проходим по ответам (не полученным)
                                        for (int c = 0; c < q.Answer_code.Count; c++)
                                        {
                                            //MessageBox.Show(a[d] + " " + q.Answer_code[c].ToString());
                                            if (a[d] == q.Answer_code[c].ToString())
                                            {
                                                db.IncAnswerStats(q.Code, q.Answer_code[c]);
                                                if (q.K[c] > 0) check = true;
                                            }
                                        }

                                        if (!check) flag2 = true;
                                    }
                                }   
                            }
                            if (flag1 && !flag2) result += Math.Round((oneMax), 2);
                            break;
                        }
                    }
                }

                result = result >= 100 ? 100 : Math.Round(result, 2);
                return result;
            }
            catch
            {
                MessageBox.Show("Ошибка при подсчёте результата!\nСтудента помянем...");
                return -1;
            }
        }
    }
}
