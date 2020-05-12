using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
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
    public partial class questionView : Window
    {
        string questType;
        bool viewOnly;
        int code;
        bool remF = false;
        DataBase db;
        StackPanel Tanswer;
        StackPanel TaddAnswer;

        public questionView(DataBase db, int questionCode, bool viewOnly)
        {
            InitializeComponent();
            Tanswer = XamlReader.Parse(XamlWriter.Save(template)) as StackPanel;
            TaddAnswer = XamlReader.Parse(XamlWriter.Save(addTemplate)) as StackPanel;

            this.viewOnly = viewOnly;
            this.db = db;
            this.code = questionCode;
            LoadQuestion(questionCode, viewOnly);
            if (!viewOnly)
            {
                window.Title = "Редактирование вопроса";
                StackPanel addAnswer = XamlReader.Parse(XamlWriter.Save(TaddAnswer)) as StackPanel;
                addAnswer.Name = "addAnswer";
                addAnswer.Visibility = Visibility.Visible;
                ((Button)addAnswer.Children[0]).Click += AddAnswer_Click;
                listBox.Items.Add(addAnswer);
                questionTextBox.IsReadOnly = false;
                dopParamBox.IsEnabled = true;
                ChangeAnswType();
                if (questionType.SelectedIndex == -1)
                {
                    questType = "Один ответ";
                    questionType.SelectedIndex = 0;
                }
            }
            else
            {
                saveBut.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadQuestion(int questionCode, bool viewOnly)
        {
            listBox.Items.Clear();
            OleDbDataReader reader = db.GetQuestion(questionCode);
            bool flag = true;
            List<double> answers = new List<double>();
            double sum = 0;
            while (reader.Read())
            {
                if (questionTextBox.Text == "") questionTextBox.Text = (reader[0]).ToString();
                if (questionType.SelectedIndex == -1)
                {
                    questType = (reader[1]).ToString();
                    questionType.SelectedIndex = ComboIndex();
                }
                if (flag)
                {
                    if ((reader[1]).ToString() == "TextAnswer")
                    {
                        if ((reader[3]).ToString() == "true") dopParamBox.IsChecked = true;
                        else dopParamBox.IsChecked = false;
                    }
                    else if ((reader[1]).ToString() == "SomeAnswers")
                    {
                        if ((reader[3]).ToString() == "true") dopParamBox.IsChecked = true;
                        else dopParamBox.IsChecked = false;
                    }
                }
                flag = false;
                //Загрузка ответов
                if (reader[6].ToString() != "")
                {
                    answers.Add((double)((int)reader[6]));
                    sum += (double)((int)reader[6]);
                    StackPanel answer = XamlReader.Parse(XamlWriter.Save(Tanswer)) as StackPanel;
                    answer.Name = "answer" + answers.Count;
                    answer.Visibility = Visibility.Visible;
                    ((Button)(answer.Children[3])).Click += delete_button_Click;
                    ((TextBox)(answer.Children[2])).TextChanged += right_text_TextChanged;
                    ((CheckBox)(answer.Children[1])).Checked += answer_Check;
                    ((TextBox)(answer.Children[0])).Text = reader[4].ToString();
                    if (!viewOnly)
                    {
                        if (questType == "Один ответ" || (questType == "Несколько ответов" && dopParamBox.IsChecked == false))
                        {
                            if ((decimal)reader[5] > 0) ((CheckBox)(answer.Children[1])).IsChecked = true;
                            else ((CheckBox)(answer.Children[1])).IsChecked = false;
                        }
                        if (questType == "Несколько ответов" && dopParamBox.IsChecked == true)
                        {
                            ((TextBox)(answer.Children[2])).Text = Math.Round(((decimal)reader[5]),2).ToString();
                        }
                    }
                    listBox.Items.Add(answer);
                }
            }
            reader.Close();
            if (viewOnly)
            {
                for (int i = 0; i < answers.Count; i++)
                {
                    double percent;
                    if (sum == 0) percent = 0;
                    else percent = (answers[i] / sum) * 100;
                    ((TextBox)((StackPanel)listBox.Items[i]).Children[2]).Text = ((percent == 100 || percent == 0) ? percent : Math.Round(percent, 2)).ToString() + "%";
                }
            }
        }

        private int ComboIndex()
        {
            int i;
            switch (questType)
            {
                case "Один ответ":
                case "OneAnswer":
                    i = 0;
                    break;
                case "Несколько ответов":
                case "SomeAnswers":
                    i = 1;
                    break;
                case "Ответ текстом":
                case "TextAnswer":
                    i = 2;
                    break;
                default:
                    i = -1;
                    break;
            }
            return i;
        }

        private void questionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewOnly) questionType.SelectedIndex = ComboIndex();
            if (questionType.SelectedIndex != -1)
            {
                switch (((ComboBoxItem)((ComboBox)sender).SelectedItem).Content.ToString())
                {
                    case "Один ответ":
                        dopPanel.Visibility = Visibility.Hidden;
                        break;
                    case "Несколько ответов":
                        dopPanel.Visibility = Visibility.Visible;
                        dopParam.Content = "Учёт коэффициента:";
                        break;
                    case "Ответ текстом":
                        dopPanel.Visibility = Visibility.Visible;
                        dopParam.Content = "Учёт регистра:";
                        break;
                }
                if (!viewOnly)
                {
                    questType = ((ComboBoxItem)((ComboBox)sender).SelectedItem).Content.ToString();
                    ChangeAnswType();
                }
            }
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            StackPanel answer = XamlReader.Parse(XamlWriter.Save(Tanswer)) as StackPanel;
            answer.Name = "answer" + listBox.Items.Count;
            ((TextBox)answer.Children[0]).Text = "";
            answer.Visibility = Visibility.Visible;
            ((Button)(answer.Children[3])).Click += delete_button_Click;
            ((TextBox)(answer.Children[2])).TextChanged += right_text_TextChanged;
            ((CheckBox)(answer.Children[1])).Checked += answer_Check;
            listBox.Items.Insert(listBox.Items.Count - 1, answer);
            ChangeAnswType();
    }

        private void ChangeAnswType()
        {
            foreach (StackPanel s in listBox.Items)
            {
                if (s.Name != "addTemplate" && s.Name != "addAnswer")
                {
                    ((TextBox)s.Children[0]).IsReadOnly = false;
                    ((UIElement)s.Children[1]).IsEnabled = true;
                    ((TextBox)s.Children[2]).IsReadOnly = false;
                    ((TextBox)s.Children[2]).Margin = new Thickness(0, 0, 0, 0);
                    ((Button)s.Children[3]).Margin = new Thickness(0, 0, 0, 0);
                    ((Button)s.Children[3]).Margin = new Thickness(0, 0, 0, 0);
                    ((UIElement)s.Children[1]).Visibility = Visibility.Collapsed;
                    ((UIElement)s.Children[2]).Visibility = Visibility.Collapsed;
                    ((UIElement)s.Children[3]).Visibility = Visibility.Visible;
                }
            }
            if (questType == "TextAnswer" || questType == "Ответ текстом")
            {
                foreach (StackPanel s in listBox.Items)
                {
                    if (s.Name != "addTemplate" && s.Name != "addAnswer")
                    {

                        ((Button)s.Children[3]).Margin = new Thickness(25, 0, 0, 0);
                    }
                }
            }
            else if (questType == "SomeAnswers" || questType == "Несколько ответов")
            {
                if (dopParamBox.IsChecked == true)
                {
                    foreach (StackPanel s in listBox.Items)
                    {
                        if (s.Name != "addTemplate" && s.Name != "addAnswer")
                        {
                            ((UIElement)s.Children[2]).Visibility = Visibility.Visible;
                        }
                    }
                }
                else
                {
                    foreach (StackPanel s in listBox.Items)
                    {
                        if (s.Name != "addTemplate" && s.Name != "addAnswer")
                        {
                            ((UIElement)s.Children[1]).Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            else if (questType == "OneAnswer" || questType == "Один ответ")
            {
                foreach (StackPanel s in listBox.Items)
                {
                    if (s.Name != "addTemplate" && s.Name != "addAnswer")
                    {
                        ((UIElement)s.Children[1]).Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void dopParamBox_Check(object sender, RoutedEventArgs e)
        {
            if (!viewOnly && questType != null) ChangeAnswType();
        }

        private void answer_Check(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            if (questType == "Один ответ")
            {
                foreach (StackPanel sp in listBox.Items)
                {
                    if (sp.Name != "addAnswer")
                        if (((CheckBox)sp.Children[1]).IsChecked == true)
                            counter++;
                }
            }
            if (counter > 1)
            {
                MessageBox.Show("Для данного типа вопроса возможен только один ответ!");
                foreach (StackPanel sp in listBox.Items)
                {
                    if (sp.Name != "addAnswer")
                        ((CheckBox)sp.Children[1]).IsChecked = false;
                }
            }
        }

        private void delete_button_Click(object sender, RoutedEventArgs e)
        {
            listBox.Items.Remove(((FrameworkElement)((Button)sender).Parent));
        }

        private string GetDopParam()
        {
            return ((dopParamBox.IsChecked == true) ? "true" : "false");
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!viewOnly && !remF)
            {
                MessageBoxResult answer = MessageBox.Show($"Вы уверены что хотите сохранить изменения?\nПри нажатии на кнопку \"Да\" будет сброшена статистика ответов.", "Сохранить изменения?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (answer == MessageBoxResult.Yes)
                {
                    List<string> answers = new List<string>();
                    List<decimal> koef = new List<decimal>();
                    foreach (StackPanel sp in listBox.Items)
                    {
                        if (sp.Name != "addTemplate" && sp.Name != "addAnswer" && sp.Name != "template")
                        {
                            if (((TextBox)sp.Children[0]).Text != null && ((TextBox)sp.Children[0]).Text != "")
                                answers.Add(((TextBox)sp.Children[0]).Text);
                            else
                            {
                                MessageBox.Show("Необходимо заполнить все ответы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                e.Cancel = true;
                                return;
                            }

                            if (questType == "Ответ текстом")
                                koef.Add(1);
                            else if (questType == "Несколько ответов")
                            {
                                if (GetDopParam() == "true")
                                    koef.Add(decimal.Parse(((TextBox)sp.Children[2]).Text));
                                else
                                {
                                    if (((CheckBox)sp.Children[1]).IsChecked == true) koef.Add(1);
                                    else koef.Add(0);
                                }
                            }
                            else if (questType == "Один ответ")
                            {
                                if (((CheckBox)sp.Children[1]).IsChecked == true) koef.Add(1);
                                else koef.Add(0);
                            }
                            else
                                koef.Add(0);
                        }
                    }

                    if (questionTextBox.Text == "" && questionTextBox.Text == null)
                    {
                        MessageBox.Show("Необходимо заполнить поле вопроса!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        e.Cancel = true;
                        return;
                    }

                    db.UpdateQuestion(code, questionTextBox.Text, questType, GetDopParam(), answers, koef);
                }
                else if (answer == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void right_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!viewOnly)
            {
                decimal d;
                if (((TextBox)sender).Text != "")
                {
                    if (!decimal.TryParse(((TextBox)sender).Text, out d))
                    {
                        ((TextBox)sender).Text = ((TextBox)sender).Text.Remove(((TextBox)sender).SelectionStart - 1, 1);
                        MessageBox.Show("Введены недопустимые значения");
                    }
                }
                else
                    ((TextBox)sender).Text = "0";
            }
        }

        private void saveBut_Click(object sender, RoutedEventArgs e)
        {
            if (!viewOnly)
            {
                MessageBoxResult answer = MessageBox.Show($"Вы уверены что хотите сохранить изменения?\nПри нажатии на кнопку \"Да\" будет сброшена статистика ответов.", "Сохранить изменения?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (answer == MessageBoxResult.Yes)
                {
                    List<string> answers = new List<string>();
                    List<decimal> koef = new List<decimal>();
                    foreach (StackPanel sp in listBox.Items)
                    {
                        if (sp.Name != "addTemplate" && sp.Name != "addAnswer" && sp.Name != "template")
                        {
                            if (((TextBox)sp.Children[0]).Text != null && ((TextBox)sp.Children[0]).Text != "")
                                answers.Add(((TextBox)sp.Children[0]).Text);
                            else
                            {
                                MessageBox.Show("Необходимо заполнить все ответы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            if (questType == "Ответ текстом")
                                koef.Add(1);
                            else if (questType == "Несколько ответов")
                            {
                                if (GetDopParam() == "true")
                                    koef.Add(decimal.Parse(((TextBox)sp.Children[2]).Text));
                                else
                                {
                                    if (((CheckBox)sp.Children[1]).IsChecked == true) koef.Add(1);
                                    else koef.Add(0);
                                }
                            }
                            else if (questType == "Один ответ")
                            {
                                if (((CheckBox)sp.Children[1]).IsChecked == true) koef.Add(1);
                                else koef.Add(0);
                            }
                            else
                                koef.Add(0);
                        }
                    }

                    if (questionTextBox.Text == "" && questionTextBox.Text == null)
                    {
                        MessageBox.Show("Необходимо заполнить поле вопроса!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    db.UpdateQuestion(code, questionTextBox.Text, questType, GetDopParam(), answers, koef);
                }
            }
        }

        private void removeBut_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult answer = MessageBox.Show($"Вы уверены что хотите удалить вопрос?", "Удаление вопроса", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer == MessageBoxResult.Yes)
            {
                db.DeleteQuestion(code);
                remF = true;
                window.Close();
            }
        }
    }
}
