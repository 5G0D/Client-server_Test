using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NePishiteSetiNa100ballov
{
    public partial class questionsWindow : Window
    {
        DataBase db;
        StackPanel Tquestion;
        StackPanel TaddQuestion;
        public questionsWindow(DataBase db)
        {
            InitializeComponent();
            Tquestion = XamlReader.Parse(XamlWriter.Save(template)) as StackPanel;
            TaddQuestion = XamlReader.Parse(XamlWriter.Save(addTemplate)) as StackPanel;
            this.db = db;
            LoadQuestions();
        }

        private void LoadQuestions()
        {
            listBox.Items.Clear();
            listBox.MouseDoubleClick += new MouseButtonEventHandler(listbox_MouseDoubleClick);
            List<string> questions = db.GetQuestions();
            for (int i = 0; i < questions.Count; i++)
            {
                StackPanel question = XamlReader.Parse(XamlWriter.Save(Tquestion)) as StackPanel;
                question.Name = "question" + i;
                question.Visibility = Visibility.Visible;
                ((Button)question.Children[1]).Click += viewQuestion_Click;
                ((Button)question.Children[2]).Click += EditQuestion_Click;
                ((TextBlock)(question.Children[0])).Text = questions[i];
                listBox.Items.Add(question);
            }
            StackPanel addQuestion = XamlReader.Parse(XamlWriter.Save(TaddQuestion)) as StackPanel;
            addQuestion.Name = "addQuestion";
            addQuestion.Visibility = Visibility.Visible;
            ((Button)addQuestion.Children[0]).Click += AddQuestion_Click;
            listBox.Items.Add(addQuestion);
        }
        private void listbox_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (((StackPanel)((ListBox)sender).SelectedItem) != null)
            {
                if (((StackPanel)((ListBox)sender).SelectedItem).Name != "addQuestion")
                {
                    string[] q = (((TextBlock)((StackPanel)((ListBox)sender).SelectedItem).Children[0]).Text).Split('.');
                    questionView questionV = new questionView(db, Convert.ToInt32(q[0]), true);
                    questionV.ShowDialog();
                }
                else
                {
                    db.AddQuestion();
                }
                LoadQuestions();
            }
        }

        private void viewQuestion_Click(object sender, RoutedEventArgs e)
        {
            string[] q = (((TextBlock)((StackPanel)((Button)sender).Parent).Children[0]).Text).Split('.');
            questionView questionV = new questionView(db, Convert.ToInt32(q[0]), true);
            questionV.ShowDialog();
            LoadQuestions();
        }

        private void EditQuestion_Click(object sender, RoutedEventArgs e)
        {
            string[] q = (((TextBlock)((StackPanel)((Button)sender).Parent).Children[0]).Text).Split('.');
            questionView questionV = new questionView(db, Convert.ToInt32(q[0]), false);
            questionV.ShowDialog();
            LoadQuestions();
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            db.AddQuestion();
            LoadQuestions();
        }
    }
}
