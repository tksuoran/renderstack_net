//  http://gafferongames.com/networking-for-game-programmers/what-every-programmer-needs-to-know-about-game-networking/
//  http://www.winsocketdotnetworkprogramming.com/clientserversocketnetworkcommunication8r.html

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading;

using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace net
{
    public class Sender : Peer
    {
        public Sender(string remoteAddress, string localAddress, string port)
        {
            RemoteAddress = remoteAddress;
            LocalAddress = localAddress;
            Port = port;
        }
        public override void Run()
        {
            buffer = new byte[bufferSize];
            int rc;
 
            try
            {
                udpSocket = new UdpClient(new IPEndPoint(localAddress, 0));
                udpSocket.Connect(destAddress, portNumber);
                for(int i = 0; i < sendCount; i++)
                {
                    rc = udpSocket.Send(buffer, buffer.Length);
                }
                for(int i = 0; i < 3; i++)
                {
                    rc = udpSocket.Send(buffer, 0);
                    System.Threading.Thread.Sleep(250);
                }
            }
            catch(SocketException)
            {
            }
            finally
            {
                if(udpSocket != null)
                {
                    udpSocket.Close();
                }
            }
        }
    }
}
