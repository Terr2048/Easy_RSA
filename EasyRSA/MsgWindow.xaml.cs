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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyRSA
{
    /// <summary>
    /// Interaction logic for MsgWindow.xaml
    /// </summary>
    public partial class MsgWindow : Window
    {
        public void UpdateChat(string message)
        {
            Chat.Dispatcher.Invoke(
                () => Chat.Text += message + Environment.NewLine
            );
        }

        public Action<string> Send;

        public MsgWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Send(EditMsg.Text);
        }

    }
}
