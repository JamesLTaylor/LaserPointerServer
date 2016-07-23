using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


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
            bReset.IsEnabled = false;

            KeyboardAndMouseSender sender = new KeyboardAndMouseSender();
            messageListener = new MessageListener(addressString, width, height, sender);            
        }

        private async void bStart_Click(object sender, EventArgs e)
        {
            bStart.IsEnabled = false;
            bReset.IsEnabled = true;
            bStop.IsEnabled = true;
            cancellationSource.Dispose();
            cancellationSource = new CancellationTokenSource();

            tbServerOutput.Text = "started";
            var progress = new Progress<string>(s => tbServerOutput.Text = s);
            await Task.Factory.StartNew(() => messageListener.LoopUntilStopRequest(progress, cancellationSource.Token),
                                        TaskCreationOptions.LongRunning);
            tbServerOutput.Text += "\nfinished";
            bStart.IsEnabled = true;
            bStop.IsEnabled = false;
            bReset.IsEnabled = false;
        }

        /**
        Sends a message to cancel and keys or buttons that might be down then cancels.  Waits for message back.
        */
        private void bStop_Click(object sender, RoutedEventArgs e)
        {
            messageListener.Reset();
            messageListener.Cancel();
            cancellationSource.Cancel();
        }

        private void bReset_Click(object sender, RoutedEventArgs e)
        {
            messageListener.Reset(); 
        }
    }
}
