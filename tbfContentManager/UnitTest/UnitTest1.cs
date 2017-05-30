using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Sockets;
using tbfContentManager.Classes;
using WhiteCode.Network;
using tbfContentManager;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        

        [TestMethod]
        public SimpleNetwork_Client ConnectToTCP()
        {
            SimpleNetwork_Client TCPClient = new SimpleNetwork_Client(null, 8000, "", IPAddress.Parse("62.138.6.50"),
                                                13001, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if(TCPClient.connect())
            {
                Assert.IsTrue(true);
                return TCPClient;
            }
            else {
                Assert.IsFalse(false);
                return null;
            }
            
        }



       [TestMethod]
        public void AddRoomSend()
        {
            bool bTest1 = false;
            bool bTest2 = false;


            //Arrange not necessary
            SimpleNetwork_Client TCPClient = ConnectToTCP();
            MainContentWindow mainContentWindow = new MainContentWindow(ref TCPClient, "test", 1);
            RoomManager roomManager = new RoomManager(ref TCPClient, mainContentWindow);
            
            if(TCPClient != null)
            {
                //Act
                bTest1 = roomManager.AddRoomSend(2, ";", "Hallo Welt", "http://gehtdichnixan.de/", true, "Unit Test Name");

                bTest2 = roomManager.AddRoomSend(2, ";", "Hallo Welt", "http://gehtdichnixan.de/", true, "");
            }
            //Assert
            Assert.IsTrue(bTest1);
            Assert.IsFalse(bTest2);
        }
    }
}