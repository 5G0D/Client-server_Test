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

using System.Windows.Controls.Primitives;

namespace Client
{
    class Quetion
    {
        public Quetion(string mes)
        {
            answers = new List<string>();
            var arr = mes.Split('|');
            QuetionCode = int.Parse(arr[0]);
            switch(arr[1])
            {
                case "Один ответ":
                    type = QuetionType.single;
                    break;
                case "Несколько ответов":
                    type = QuetionType.multiple;
                    break;
                case "Ответ текстом":
                    type = QuetionType.text;
                    break;
            }
            quetion = arr[2];
            for (int i = 3; i < arr.Length; i++)
            {
                answers.Add(arr[i]);
            }
        }
        public QuetionType type;
        public string quetion;
        public List<string> answers;
        public int QuetionCode;
    }
    public enum QuetionType
    {
        single, multiple, text
    }
    class QuetionPage : TabItem
    {
        Quetion quetion;
        int AnswerCount;
        TextBlock quetionBlock;
        List<FrameworkElement> answers;
        StackPanel Panel;
        public QuetionPage(Quetion q) : base()
        {
            Visibility = Visibility.Collapsed;
            quetionBlock = new TextBlock();
            answers = new List<FrameworkElement>();
            this.quetion = q;
            Panel = new StackPanel();
            this.Content = Panel;
            setQuetion(quetion.quetion);
            setAnswers(quetion.answers);
            Panel.Children.Add(quetionBlock);
            foreach (var item in answers)
            {
                Panel.Children.Add(item);
            }
        }


        public string GetAnswers()
        {
            StringBuilder temp = new StringBuilder(quetion.QuetionCode.ToString() + "|");
            bool flag = false;
            switch (quetion.type)
            {
                case QuetionType.single:
                    for (int i = 0; i < AnswerCount; i++)
                    {
                        if (((RadioButton)answers[i]).IsChecked.Value)
                        {
                            flag = true;
                            temp.Append(i.ToString() + "/");
                            break;
                        }
                    }
                    break;
                case QuetionType.multiple:
                    for (int i = 0; i < AnswerCount; i++)
                    {
                        if (((CheckBox)answers[i]).IsChecked.Value)
                        {
                            flag = true;
                            temp.Append(i.ToString() + "|");
                        }
                    }
                    temp[temp.Length - 1] = '/';
                    break;

                case QuetionType.text:
                    flag = true;
                    temp.Append(((TextBox)answers[0]).Text + "/");
                    break;
                default:
                    break;
            }

            if (!flag)
                temp.Append("/");
            return temp.ToString();
        }

        void setQuetion(string Quetion)
        {
            quetionBlock.Text = Quetion;
            quetionBlock.FontSize = 15;
            quetionBlock.Margin = new Thickness(10, 0, 0, 0);
            quetionBlock.TextWrapping = TextWrapping.Wrap;
            quetionBlock.MinHeight = 50;


        }
        void setAnswers(List<string> Answers)
        {
            AnswerCount = Answers.Count;
            switch (quetion.type)
            {
                case QuetionType.single:
                    foreach (var item in Answers)
                    {
                        answers.Add(new RadioButton() { MinHeight = 30, FontSize = 15, Content = new TextBlock() { Text = item, TextWrapping = TextWrapping.Wrap } });
                    }
                    break;
                case QuetionType.multiple:
                    foreach (var item in Answers)
                    {
                        answers.Add(new CheckBox() { MinHeight = 30, FontSize = 15, Content = new TextBlock() { Text = item, TextWrapping = TextWrapping.Wrap } });
                    }
                    break;
                case QuetionType.text:
                    answers.Add(new TextBox() { MinHeight = 15, Width = 300, FontSize = 15});
                    break;
                default:
                    break;
            }
        }

    }





    class Student
    {
        public Student() { }

        public Student(string mes)
        {
            var temp = new StringBuilder(mes);
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] == '|')
                {
                    char[] buffer = new char[i + 1];
                    temp.CopyTo(0, buffer, 0, i);
                    ID = int.Parse(new string(buffer));
                    temp.Remove(0, i + 1);
                    break;
                }
            }
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] == '|')
                {
                    char[] buffer = new char[i + 1];
                    temp.CopyTo(0, buffer, 0, i);
                    LastName = Cut(new string(buffer));
                    temp.Remove(0, i + 1);
                    break;
                }
            }
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] == '|')
                {
                    char[] buffer = new char[i + 1];
                    temp.CopyTo(0, buffer, 0, i);
                    Name = Cut(new string(buffer));
                    temp.Remove(0, i + 1);
                    break;
                }
            }
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] == '|')
                {
                    char[] buffer = new char[i + 1];
                    temp.CopyTo(0, buffer, 0, i);
                    FatherName = Cut(new string(buffer));
                    temp.Remove(0, i + 1);
                    break;
                }
            }
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] == '|')
                {
                    char[] buffer = new char[i + 1];
                    temp.CopyTo(0, buffer, 0, i);
                    Group = Cut(new string(buffer));
                    temp.Remove(0, i + 1);
                    break;
                }
            }
            temp.Replace("/", "");
            Result = double.Parse(temp.ToString());

        }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string Group { get; set; }
        public double Result { get; set; }
        public int ID { get; set; }
        public override string ToString()
        {
            return LastName + " " + Name + " " + FatherName;
        }

        private string Cut(string str)
        {
            return str.Remove(str.Length - 1);
        }
    }
}
