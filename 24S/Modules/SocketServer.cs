// A C# Program for Server 
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;

namespace _24S
{

    public sealed class SocketServer
    {
        public static SocketServer Instance { get; } = new SocketServer(); // Singleton

        public delegate void DataReceivedEventHandler(Stream clientStream, String message);

        public event DataReceivedEventHandler DataReceived;

        public async void ExecuteServer()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 11111.
                Int32 port = 11111;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[4098];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    LoggingServices.Instance.WriteLine<SocketServer>("Waiting for a connection... ", MetroLog.LogLevel.Info);

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = await server.AcceptTcpClientAsync();
                    LoggingServices.Instance.WriteLine<SocketServer>("Client connected", MetroLog.LogLevel.Info);

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    Task
                        .Factory
                        .StartNew(() => clientCommunicationThread(stream, bytes, data));

                }
            }
            catch (SocketException e)
            {
                LoggingServices.Instance.WriteLine<SocketServer>("SocketException: " + e.ToString(), MetroLog.LogLevel.Error);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }

        private void NotifySubscribers(Stream clientStream, String message)
        {
            OnDataReceived(clientStream, message);
        }

        private void OnDataReceived(Stream clientStream, String message)
        {
            DataReceived?.Invoke(clientStream, message);
        }

        private async void clientCommunicationThread(Stream stream, Byte[] bytes, String data)
        {
            try
            {
                int i;
                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    NotifySubscribers(stream, data);

                    // Process the data sent by the client.
                    //data = data.ToUpper();

                    //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                    // Send back a response.
                    //stream.Write(msg, 0, msg.Length);
                    //Debug.WriteLine("Sent: {0}", data);
                }
            }
            catch (System.IO.IOException e)
            {
                // Shutdown and end connection
                //client.Close();
            }
        }
    }
}