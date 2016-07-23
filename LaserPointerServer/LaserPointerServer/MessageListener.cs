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
        private double screenHeight;
        private String messageFromServer;
        private KeyboardAndMouseSender sender;
        private double screenWidth;
        private double topY = 5.0;
        private double bottomY = 0.0;
        private double leftX = 4.0;
        private double rightX = -4.0;
        private bool ctrlDown = false;
        private bool mouseLeftDown = false;


        public MessageListener(string addressString, double width, double height, KeyboardAndMouseSender sender)
        {
            this.addressString = addressString;
            this.screenWidth = width;
            this.screenHeight = height;
            this.sender = sender;
        }

        public void LoopUntilStopRequest(IProgress<string> progress, CancellationToken cancellationToken) {
                        
            String fullAddress = "tcp://" + addressString + ":5555";
            NetMQSocket socket = new ResponseSocket();
            uint xLatest = 0;
            uint yLatest = 0; 
            double x, y;

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
                if (message.Equals("q"))
                {                    
                    break;
                }
                else if (message[0] == 45 || (message[0] >= 48 && message[0] <= 57))
                {
                    UpdateXYFromString(message, out x, out y);
                    yLatest = Convert.ToUInt32(Math.Max(0, Math.Round((1 - (y - bottomY) / (topY - bottomY)) * screenHeight)));
                    xLatest = Convert.ToUInt32(Math.Max(0, Math.Round((1 - (x - rightX) / (leftX - rightX)) * screenWidth)));
                    sender.SetCursor(xLatest, yLatest, mouseLeftDown);
                }
                else if (message[0] == 't')
                {
                    UpdateXYFromString(message.Substring(1, message.Length - 1), out x, out y);
                    messageFromServer += "\ntop set to" + y;
                    topY = y;
                }
                else if (message[0] == 'b')
                {
                    UpdateXYFromString(message.Substring(1, message.Length - 1), out x, out y);
                    messageFromServer += "\nbottom set to" + y;
                    bottomY = y;
                }
                else if (message[0] == 'l')
                {
                    UpdateXYFromString(message.Substring(1, message.Length - 1), out x, out y);
                    messageFromServer += "\nleft set to" + x;
                    leftX = x;
                }
                else if (message[0] == 'r')
                {
                    UpdateXYFromString(message.Substring(1, message.Length - 1), out x, out y);
                    messageFromServer += "\nright set to" + x;
                    rightX = x;
                }
                else if (message.Equals("cy"))
                {
                    if (!ctrlDown)
                    {
                        ctrlDown = true;
                        mouseLeftDown = true;
                        messageFromServer += "\nCTRL and left button set to down";
                        sender.SetCtrlDown();
                    }
                    else
                    {
                        messageFromServer += "\nCTRL and left button were aleady down";
                    }
                }
                else if (message.Equals("cn"))
                {
                    if (ctrlDown)
                    {
                        ctrlDown = false;
                        mouseLeftDown = false;
                        messageFromServer += "\nCTRL and left button set to up";
                        sender.SetCtrlUp();
                    }
                    else
                    {
                        messageFromServer += "\nCTRL and left button were aleady up";
                    }
                }
                else if (message.Equals("pu"))
                {
                    if (ctrlDown) sender.SetCtrlUp();
                    sender.SendPgUp();
                    if (ctrlDown) sender.SetCtrlDown();
                    messageFromServer += "\nPage up sent";
                }
                else if (message.Equals("pd"))
                {
                    if (ctrlDown) sender.SetCtrlUp();
                    sender.SendPgDown();
                    if (ctrlDown) sender.SetCtrlDown();
                    messageFromServer += "\nPage down sent";
                }
                else if (message.Equals("dy"))
                {
                    mouseLeftDown = true;
                    messageFromServer += "\nMouse left button set to down";
                }
                else if (message.Equals("dn"))
                {
                    if (!mouseLeftDown)
                    {
                        messageFromServer += "\nMouse left button is not down, cannot release";
                    }
                    else
                    {
                        mouseLeftDown = false;
                        sender.SetCursorLeftUp(xLatest, yLatest);
                        messageFromServer += "\nMouse left button released";
                    }
                }
                else
                {
                    messageFromServer += "\nUnknown message: " + message;
                }
                socket.SendFrame("d");
                progress.Report(messageFromServer);
            }
            socket.Close();            
        }

        internal void Reset()
        {
            String fullAddress = "tcp://" + addressString + ":5555";
            NetMQSocket socket = new RequestSocket();
            socket.Connect(fullAddress);
            socket.SendFrame("cn");
            String response = socket.ReceiveFrameString();
        }

        private void UpdateXYFromString(string message, out double x, out double y)
        {
            string[] parts = message.Split(',');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Expected two values separated by a comma, got: " + message);
            }

            if (!Double.TryParse(parts[0], out x) || !Double.TryParse(parts[1], out y))
            {
                throw new ArgumentException("Expected two values separated by a comma, got: " + message);
            }
            
        }


        internal void Cancel()
        {
            String fullAddress = "tcp://" + addressString + ":5555";
            NetMQSocket socket = new RequestSocket();
            socket.Connect(fullAddress);
            socket.SendFrame("q");
            String response = socket.ReceiveFrameString();
        }
    }
}
