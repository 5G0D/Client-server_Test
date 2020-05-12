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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NePishiteSetiNa100ballov
{
    public partial class ToggleButton : UserControl
    {
        Thickness LeftSide = new Thickness(-39, 0, 0, 0);
        Thickness RightSide = new Thickness(0, 0, -39, 0);
        SolidColorBrush Off = new SolidColorBrush(Color.FromRgb(176, 100, 100));
        SolidColorBrush On = new SolidColorBrush(Color.FromRgb(130, 190, 125));
        private bool toggled = false;

        public ToggleButton()
        {
            InitializeComponent();
            Back.Fill = Off;
            toggled = false;
            Dot.Margin = LeftSide;
        }

        public bool Toggled { get { return toggled; } set { toggled = value; } }

        private void Dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!toggled)
            {
                Back.Fill = On;
                toggled = true;
                Dot.Margin = RightSide;
            }
            else
            {
                Back.Fill = Off;
                toggled = false;
                Dot.Margin = LeftSide;
            }
        }

        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!toggled)
            {
                Back.Fill = On;
                toggled = true;
                Dot.Margin = RightSide;
            }
            else
            {
                Back.Fill = Off;
                toggled = false;
                Dot.Margin = LeftSide;
            }

        }

        public void ChangeToggle(bool flag)
        {
            if (flag)
            {
                Back.Fill = On;
                toggled = true;
                Dot.Margin = RightSide;
            }
            else
            {
                Back.Fill = Off;
                toggled = false;
                Dot.Margin = LeftSide;
            }
        }
    }
}
