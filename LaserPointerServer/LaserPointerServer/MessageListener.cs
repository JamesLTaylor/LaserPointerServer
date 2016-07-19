using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LaserPointerServer
{
    class MessageListener
    {
        private string addressString;
        private String messageFromServer;        

        public MessageListener(string addressString)
        {
            this.addressString = addressString;
        }

        public void loopUntilStopRequest(IProgress<string> progress, CancellationToken cancellationToken) {
                        
            String fullAddress = "tcp://" + addressString + ":5555";
            NetMQSocket socket = new ResponseSocket();

            try {
                socket.Bind(fullAddress);
            } catch (NetMQException e)
            {
                MessageBox.Show("Check that another server is not running on the same address:\n\n" + e.Message, 
                    "Cannot bind to address", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            messageFromServer = "Started at: " + fullAddress;
            progress.Report(messageFromServer);
            while (!cancellationToken.IsCancellationRequested)            
            {
                String message = socket.ReceiveFrameString();
                messageFromServer += "\n" + message;                
                socket.SendFrame("Thanks");
                progress.Report(messageFromServer);
                if (message.Equals("q")) break;
            }
            socket.Close();
            
        }

        internal void Cancel()
        {
            String fullAddress = "tcp://" + addressString + ":5555";
            NetMQSocket socket = new RequestSocket();
            socket.Connect(fullAddress);
            socket.SendFrame("q");
        }
    }
}
