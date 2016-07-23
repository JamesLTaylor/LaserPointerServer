using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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

namespace LaserPointerServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MessageListener messageListener;
        String addressString;
        CancellationTokenSource cancellationSource;

        public MainWindow()
        {            
            InitializeComponent();
            cancellationSource = new CancellationTokenSource();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());            
            IPAddress address = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            addressString = address.ToString();
            
            string[] addressParts = addressString.Split('.');
            tbAddress1.Text = addressParts[0];
            tbAddress2.Text = addressParts[1];
            tbAddress3.Text = addressParts[2];
            tbAddress4.Text = addressParts[3];

            double height = SystemParameters.FullPrimaryScreenHeight;
            double width = SystemParameters.FullPrimaryScreenWidth;
            tbServerOutput.Text += "Screen set to " + width + "x" + height;

            bStop.IsEnabled = false;

            KeyboardAndMouseSender sender = new KeyboardAndMouseSender();
            messageListener = new MessageListener(addressString, width, height, sender);            
        }

        private async void bStart_Click(object sender, EventArgs e)
        {
            bStart.IsEnabled = false;
            bStop.IsEnabled = true;
            cancellationSource.Dispose();
            cancellationSource = new CancellationTokenSource();

            tbServerOutput.Text = "started";
            var progress = new Progress<string>(s => tbServerOutput.Text = s);
            await Task.Factory.StartNew(() => messageListener.LoopUntilStopRequest(progress, cancellationSource.Token),
                                        TaskCreationOptions.LongRunning);
            tbServerOutput.Text += "\nfinished";
            bStart.IsEnabled = true;
        }

        private void bStop_Click(object sender, RoutedEventArgs e)
        {
            messageListener.Cancel();
            cancellationSource.Cancel();
            bStop.IsEnabled = false;
        }

        private void bReset_Click(object sender, RoutedEventArgs e)
        {
            messageListener.Reset(); 
        }
    }
}
