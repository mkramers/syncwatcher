using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Common.IO
{
    //derived from https://msdn.microsoft.com/en-us/library/system.net.sockets.tcplistener(v=vs.110).aspx
    public class Listener
    {
        public static void Start(string _ipAdress, int _port, Action<ListenerEventArgs> _onReceivedMessage = null)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_ipAdress));
            Debug.Assert(_port > 0);
            //Debug.Assert(_onReceivedMessage != null);

            TcpListener server = null;
            try
            {
                IPAddress localAddr = IPAddress.Parse(_ipAdress);

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, _port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                byte[] bytes = new byte[256];
                string data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        _onReceivedMessage?.Invoke(new ListenerEventArgs(data));

                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }

    public class ListenerEventArgs : EventArgs
    {
        public string Message { get; set; }

        public ListenerEventArgs(string _message)
        {
            Debug.Assert(_message != null);

            Message = _message;
        }
    }
}
