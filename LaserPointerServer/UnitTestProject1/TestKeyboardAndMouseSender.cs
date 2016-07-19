using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace LaserPointerServer
{
    [TestClass]
    public class TestKeyboardAndMouseSender
    {
        [TestMethod]
        public void TestMethod1()
        {
            KeyboardAndMouseSender sender = new KeyboardAndMouseSender();
            Thread.Sleep(1000);

            /*
            sender.SetCursorLeftDown(300, 300);
            Thread.Sleep(100);
            sender.SetCursorLeftUp(350, 350);
            */
            
            /*sender.SetCtrlDown();
            Thread.Sleep(1000);
            sender.SetCtrlUp();
            */

        }
    }
}
