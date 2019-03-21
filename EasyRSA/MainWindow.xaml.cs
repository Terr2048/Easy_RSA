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

namespace EasyRSA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            var msg_window = new MsgWindow();
            try
            {
                string ip = Edit_ip.Text;
                int port = Int32.Parse(Edit_port.Text);
                bool isServer = Radio_server.IsChecked ?? false;
                Task.Run(() => ChatClient.StartChat(msg_window.UpdateChat, ref msg_window.Send, ip, port, isServer));
                msg_window.Show();
            }
            catch (Exception ee) { msg_window.Close(); }
        }
    }
}
