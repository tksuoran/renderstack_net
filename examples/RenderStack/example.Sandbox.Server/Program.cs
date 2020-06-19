using System;
using net;

public class Program
{
    public static void Main(string[] args)
    {
        var receiver = new Receiver("127.0.0.1", "25565");
        var sender = new Sender("127.0.0.1", "127.0.0.1", "25565");
        receiver.Start();
        sender.Start();
        receiver.Join();
        sender.Join();
        Console.WriteLine("done");
    }
}
