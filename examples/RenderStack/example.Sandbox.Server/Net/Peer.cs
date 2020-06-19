using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;

namespace net
{
    public abstract class Peer
    {
        protected IPAddress   localAddress = IPAddress.Any;
        protected IPAddress   destAddress = null;
        protected ushort      portNumber = 25565;
        protected int         bufferSize = 256;
        protected int         sendCount = 5;
        protected UdpClient   udpSocket = null;
        protected byte[]      buffer;
        protected bool        udpSender = false;
        protected Thread      thread;

        public string LocalAddress
        {
            set
            {
                localAddress = IPAddress.Parse(value);
            }
        }
        public string SendCount
        {
            set
            {
                sendCount = System.Convert.ToInt32(value);
            }
        }
        public string Port
        {
            set
            {
                portNumber = System.Convert.ToUInt16(value);
            }
        }
        public string RemoteAddress
        {
            set
            {
                destAddress =  IPAddress.Parse(value);
            }
        }
        public string BufferSize
        {
            set
            {
                bufferSize = System.Convert.ToInt32(value);
            }
        }

        public void Start()
        {
            thread = new Thread(Run);
            thread.Start();
        }
        public void Join()
        {
            thread.Join();
        }
        public abstract void Run();
    }
}