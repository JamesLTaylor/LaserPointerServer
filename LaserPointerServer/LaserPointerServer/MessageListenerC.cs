
using System;
using System.Threading;
using System.Windows;
using ZeroMQ;

namespace LaserPointerServer
{
    class MessageListenerC
    {
        private string addressString;
        private String messageFromServer;        

        public MessageListenerC(string addressString)
        {
            this.addressString = addressString;
        }

        public void loopUntilStopRequest(IProgress<string> progress, CancellationToken cancellationToken) {
                        
            String fullAddress = "tcp://" + addressString + ":5555";
            var context = new ZContext();
            ZSocket socket = new ZSocket(context, ZSocketType.REP);

            socket.Bind(fullAddress);

            messageFromServer = "Started at: " + fullAddress;
            progress.Report(messageFromServer);
            while (!cancellationToken.IsCancellationRequested)            
            {
                ZFrame reply = socket.ReceiveFrame();
                String message = "hi";
                messageFromServer += "\n" + message;
                socket.SendFrame(new ZFrame("thanks"));

                progress.Report(messageFromServer);
            }
            socket.Close();
            
        }

    }
}
