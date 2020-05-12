using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NePishiteSetiNa100ballov
{
    public class DataBase
    {
        private string connectString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=serverDB.mdb;Jet OLEDB:Database Password=";
        private OleDbConnection DBConnection;
        public string DBPassword { get; set; }
        private int questions_count = 40;

        public void Initialize(MainWindow window, string password, ref bool success)
        {
            if (password == null) Environment.Exit(0);
            DBPassword = password;
            success = true;
            connectString += password + ";";
            try
            {
                DBConnection = new OleDbConnection(connectString);
                DBConnection.Open();
            }
            catch
            {
                DBPassword = null;
                success = false;
                window.Dispatcher.Invoke(() => { MessageBox.Show("Ошибка при подключении к базе данных"); });
            }
            finally
            {
                connectString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=serverDB.mdb;Jet OLEDB:Database Password=";
            }
        }

        public void Close()
        {
            DBConnection.Close();
        }

        public List<Student> GetStudentsList()
        {
            List<Student> students = new List<Student>();
            string query = "SELECT CODE, FIRST_NAME, LAST_NAME, SECOND_NAME, GROUPING, RESULT, ATTEMPTS FROM STUDENTS";

            OleDbCommand command = new OleDbCommand(query, DBConnection);
            OleDbDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                students.Add(new Student
                {
                    ID = (int)reader[0],
                    FirstName = reader[1].ToString(),
                    LastName = (reader[2].ToString()),
                    SecondName = reader[3].ToString(),
                    Group = reader[4].ToString(),
                    Result = Math.Round((decimal)reader[5],2),
                    Attempts = (int)reader[6]
                });

                query = $"SELECT RESULT FROM RESULT_HISTORY WHERE STUDENT_CODE = {students[students.Count - 1].ID} ORDER BY TIME DESC";
                command = new OleDbCommand(query, DBConnection);
                OleDbDataReader reader1 = command.ExecuteReader();

                while (reader1.Read())
                {
                    students[students.Count - 1].Results.Add(Math.Round((decimal)reader1[0],2));
                }

                reader1.Close();

                query = $"SELECT SESSION_BEGIN FROM SESSIONS WHERE STUDENT_CODE = {students[students.Count - 1].ID} ORDER BY SESSION_BEGIN DESC";
                command = new OleDbCommand(query, DBConnection);
                reader1 = command.ExecuteReader();

                while (reader1.Read())
                {
                    students[students.Count - 1].Session.Add(reader1[0].ToString());
                }

                reader1.Close();
            }

            reader.Close();

            return students;
        }

        public void UpdateSession(int ID)
        {
            string query = $"INSERT INTO SESSIONS (STUDENT_CODE, SESSION_BEGIN) VALUES ({ID},@timestamp)";

            OleDbCommand command = new OleDbCommand(query, DBConnection);

            var timestamp = command.Parameters.AddWithValue("@timestamp", DateTime.Now);
            timestamp.OleDbType = OleDbType.Date;
            command.ExecuteNonQuery();
        }

        public void UpdateAttempt(int ID)
        {
            string query = $"UPDATE STUDENTS SET ATTEMPTS = ATTEMPTS + 1 WHERE CODE = {ID}";

            OleDbCommand command = new OleDbCommand(query, DBConnection);
            command.ExecuteNonQuery();
        }

        public void UpdateStudents(List<Student> students)
        {
            string query;
            OleDbCommand command;

            foreach (Student student in students)
            {
                query = $"UPDATE STUDENTS SET LAST_NAME = '{student.LastName}', FIRST_NAME = '{student.FirstName}', SECOND_NAME = '{student.SecondName}', GROUPING = '{student.Group}', RESULT = @result, ATTEMPTS = {student.Attempts} WHERE CODE = {student.ID}";

                command = new OleDbCommand(query, DBConnection);
                var result = command.Parameters.AddWithValue("@result", student.Result);
                result.OleDbType = OleDbType.Currency;

                command.ExecuteNonQuery();
            }

            query = $"SELECT COUNT(*) FROM STUDENTS";

            command = new OleDbCommand(query, DBConnection);
            OleDbDataReader reader = command.ExecuteReader();
            reader.Read();

            if (students.Count > (int)reader[0])
            {
                for (int i = (int)reader[0]; i < students.Count; i++)
                {
                    query = $"INSERT INTO STUDENTS (LAST_NAME, FIRST_NAME, SECOND_NAME, GROUPING, RESULT, ATTEMPTS) VALUES ('{students[i].LastName}', '{students[i].FirstName}', '{students[i].SecondName}', '{students[i].Group}', @result, '{students[i].Attempts}')";

                    command = new OleDbCommand(query, DBConnection);
                    var result = command.Parameters.AddWithValue("@result", students[i].Result);
                    result.OleDbType = OleDbType.Currency;

                    command.ExecuteNonQuery();
                }

            }

            if (students.Count < (int)reader[0])
            {
                query = $"SELECT CODE FROM STUDENTS";

                command = new OleDbCommand(query, DBConnection);
                reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    bool flag = false;
                    foreach (var student in students)
                        if (student.ID == (int)reader[0]) flag = true;
                    if (!flag)
                    {
                        query = $"DELETE FROM STUDENTS WHERE CODE = {(int)reader[0]}";
                        command = new OleDbCommand(query, DBConnection);
                        command.ExecuteNonQuery();

                        query = $"DELETE FROM SESSIONS WHERE STUDENT_CODE = {(int)reader[0]}";
                        command = new OleDbCommand(query, DBConnection);
                        command.ExecuteNonQuery();

                        query = $"DELETE FROM RESULT_HISTORY WHERE STUDENT_CODE = {(int)reader[0]}";
                        command = new OleDbCommand(query, DBConnection);
                        command.ExecuteNonQuery();
                    }
                }
            }
            reader.Close();
        }

        public string GetQuestionsString()
        {
            StringBuilder result = new StringBuilder();

            string query = $"SELECT Q.CODE, SWITCH(Q.QUESTION = \"\", \"Вопрос отсутствует\", Q.QUESTION <> \"\", Q.QUESTION),  SWITCH(Q.QUESTION_TYPE IS NULL, \"Один ответ\", Q.QUESTION_TYPE IS NOT NULL, Q.QUESTION_TYPE), SWITCH(Q.QUESTION_TYPE = \"Ответ текстом\", \"\", Q.QUESTION_TYPE <> \"Ответ текстом\", A.ANSWER) FROM QUESTIONS Q LEFT JOIN ANSWERS A ON Q.CODE = A.QUESTION_CODE ORDER BY Q.CODE, A.ANSWER_CODE";
            OleDbCommand command = new OleDbCommand(query, DBConnection);
            OleDbDataReader reader = command.ExecuteReader();

            List<string> questions = new List<string>();
            Random rand = new Random();
            int cur_code = -1;

            while (reader.Read())
            {
                if ((int)reader[0] == cur_code)
                {
                    if (reader[3].ToString() != "") questions[questions.Count - 1] += "|" + reader[3];
                }
                else
                {
                    if (questions.Count - 1 >= 0) questions[questions.Count - 1] += "/";
                    cur_code = (int)reader[0];
                    questions.Add((cur_code).ToString() + "|" + reader[2] + "|" + reader[1]);
                    if (reader[3].ToString() != "") questions[questions.Count - 1] += "|" + reader[3];
                }
            }
            if (questions.Count - 1 >= 0) questions[questions.Count - 1] += "/";

            for (int i = 0; i < questions_count; i++)
            {
                string question = questions[rand.Next(0, questions.Count)];
                result.Append(question);
                questions.Remove(question);
                if (questions.Count == 0) break;
            }

            return result.ToString();
        }

        public List<Question> GetAnswers()
        {
            string query = $"SELECT A.QUESTION_CODE, A.ANSWER_CODE, A.K, Q.QUESTION_TYPE, Q.DOP_PARAMS, A.ANSWER FROM ANSWERS A LEFT JOIN QUESTIONS Q ON Q.CODE = A.QUESTION_CODE";

            OleDbCommand Command = new OleDbCommand(query, DBConnection);
            OleDbDataReader reader = Command.ExecuteReader();

            List<Question> questions = new List<Question>();
            int cur_code = -1;

            while (reader.Read())
            {
                if ((int)reader[0] == cur_code)
                {
                    questions[questions.Count - 1].AddAnswer(reader[1], reader[5], reader[2]);
                }
                else
                {
                    cur_code = (int)reader[0];
                    questions.Add(new Question(reader[0],reader[1],reader[2],reader[3],reader[4], reader[5]));
                }
            }

            return questions; 
        }

        public void UpdateResult(decimal result, int id)
        {
            string query = $"INSERT INTO RESULT_HISTORY ([STUDENT_CODE], [RESULT],[TIME]) VALUES ({id},@resultC,@timestamp)";

            OleDbCommand command = new OleDbCommand(query, DBConnection);

            var resultC = command.Parameters.AddWithValue("@resultC", result);
            resultC.OleDbType = OleDbType.Currency;

            var timestamp = command.Parameters.AddWithValue("@timestamp", DateTime.Now);
            timestamp.OleDbType = OleDbType.Date;

            command.ExecuteNonQuery();

            query = $"UPDATE STUDENTS SET RESULT = @resultC WHERE CODE = {id}";

            command = new OleDbCommand(query, DBConnection);

            resultC = command.Parameters.AddWithValue("@resultC", result);
            resultC.OleDbType = OleDbType.Currency;
            command.ExecuteNonQuery();
        }

        public List<Settings> GetSettings()
        {
            List<Settings> settings = new List<Settings>();

            string query = "SELECT CODE, IP_ADDRESS, COOLDOWN, MAX_CONNECTIONS FROM SETTINGS";

            OleDbCommand Command = new OleDbCommand(query, DBConnection);
            OleDbDataReader reader = Command.ExecuteReader();

            while (reader.Read())
            {
                Settings setting = new Settings();
                setting.ID = (int)reader[0];
                setting.IPAddress = reader[1].ToString();
                setting.Cooldown = (int)reader[2];
                setting.Connections = (int)reader[3];
                settings.Add(setting);
            }
            reader.Close();

            return settings;
        }

        public void UpdateSettings(Settings settings)
        {
            string query = $"UPDATE SETTINGS SET IP_ADDRESS = '{settings.IPAddress}', MAX_CONNECTIONS = {settings.Connections}, COOLDOWN = {settings.Cooldown} WHERE CODE = 2";

            OleDbCommand command = new OleDbCommand(query, DBConnection);
            command.ExecuteNonQuery();
        }

        public DateTime GetLastSession(int ID)
        {
            string query = $"SELECT SWITCH(MAX(SESSION_BEGIN) IS NOT NULL, MAX(SESSION_BEGIN), MAX(SESSION_BEGIN) IS NULL, NOW()-1) FROM SESSIONS WHERE STUDENT_CODE = {ID}";

            OleDbCommand Command = new OleDbCommand(query, DBConnection);
            OleDbDataReader reader = Command.ExecuteReader();

            reader.Read();

            DateTime session = (DateTime)reader[0];

            reader.Close();

            return session;
        }

        public List<string> GetQuestions()
        {
            string query = $"SELECT CODE,QUESTION FROM QUESTIONS ORDER BY CODE";
            List<string> questons = new List<string>();

            OleDbCommand Command = new OleDbCommand(query, DBConnection);
            OleDbDataReader reader = Command.ExecuteReader();


            while (reader.Read())
            {
                questons.Add(reader[0].ToString() + ". " + reader[1].ToString());
            }
            reader.Close();

            return questons;
        }

        public OleDbDataReader GetQuestion(int questionCode)
        {
            string query = $"SELECT Q.QUESTION, Q.QUESTION_TYPE, Q.CODE, Q.DOP_PARAMS, A.ANSWER, A.K, A.CHOOSEN_COUNT FROM QUESTIONS Q LEFT JOIN ANSWERS A ON Q.CODE = A.QUESTION_CODE WHERE Q.CODE = {questionCode}";

            OleDbCommand Command = new OleDbCommand(query, DBConnection);
            OleDbDataReader reader = Command.ExecuteReader();

            return reader;
        }

        public void UpdateQuestion(int code, string question, string question_type, string dop_params, List<string> answers, List<decimal> koef)
        {
            string query = $"SELECT Q.QUESTION FROM QUESTIONS Q WHERE Q.CODE = {code}";

            OleDbCommand Command = new OleDbCommand(query, DBConnection);
            OleDbDataReader reader = Command.ExecuteReader();

            reader.Read();

            if (reader.HasRows)
            {
                query = $"UPDATE QUESTIONS SET QUESTION = \"{question}\", QUESTION_TYPE = \"{question_type}\", DOP_PARAMS = \"{dop_params}\" WHERE CODE = {code}";

                Command = new OleDbCommand(query, DBConnection);
                Command.ExecuteNonQuery();

                query = $"DELETE FROM ANSWERS WHERE QUESTION_CODE = {code}";

                Command = new OleDbCommand(query, DBConnection);
                Command.ExecuteNonQuery();

                for (int i = 0; i < answers.Count; i++)
                {
                    query = $"INSERT INTO ANSWERS (QUESTION_CODE, ANSWER, K, CHOOSEN_COUNT, ANSWER_CODE) VALUES ({code},\"{answers[i]}\",\"{koef[i]}\", 0, {i})";
                    Command = new OleDbCommand(query, DBConnection);
                    Command.ExecuteNonQuery();
                }
            }

            reader.Close();

        }

        public void DeleteQuestion(int code)
        {
            string query = $"DELETE FROM ANSWERS WHERE QUESTION_CODE = {code}";

            OleDbCommand Command = new OleDbCommand(query, DBConnection);
            Command.ExecuteNonQuery();

            query = $"DELETE FROM QUESTIONS WHERE CODE = {code}";

            Command = new OleDbCommand(query, DBConnection);
            Command.ExecuteNonQuery();
        }

        public void AddQuestion()
        {
            string query = $"INSERT INTO QUESTIONS (QUESTION, QUESTION_TYPE, DOP_PARAMS) VALUES (\"Новый вопрос\", \"Один ответ\", \"true\")";

            OleDbCommand Command = new OleDbCommand(query, DBConnection);
            Command.ExecuteNonQuery();
        }

        public void IncAnswerStats(int question_code, int answer_code)
        {
            string query = $"UPDATE ANSWERS SET CHOOSEN_COUNT = CHOOSEN_COUNT + 1 WHERE QUESTION_CODE = { question_code } AND ANSWER_CODE = { answer_code }";
            OleDbCommand command = new OleDbCommand(query, DBConnection);
            command.ExecuteNonQuery();
        }
    }
}
