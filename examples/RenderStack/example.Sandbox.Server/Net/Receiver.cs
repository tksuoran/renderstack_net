//  http://gafferongames.com/networking-for-game-programmers/what-every-programmer-needs-to-know-about-game-networking/
//  http://www.winsocketdotnetworkprogramming.com/clientserversocketnetworkcommunication8r.html

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace net
{
    public class Receiver : Peer
    {
        public Receiver(string localAddress, string port)
        {
            LocalAddress = localAddress;
            Port = port;
        }
        public override void Run()
        {
            udpSender = false;
            udpSocket = null;
            buffer = new byte[bufferSize];
 
            try
            {
                udpSocket = new UdpClient(new IPEndPoint(localAddress, portNumber));
                IPEndPoint senderEndPoint = new IPEndPoint(localAddress, 0);
                while(true)
                {
                    buffer = udpSocket.Receive(ref senderEndPoint);
                    if(buffer.Length == 0)
                    {
                        break;
                    }
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
