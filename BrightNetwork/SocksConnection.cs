using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BrightNetwork
{
    public static class SocksConnection
    {
        public static async Task<Socket> OpenConnection(int version, IPAddress serverAddress, int serverPort, string socksAddress, int socksPort, string username = null, string password = null)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endPoint = new DnsEndPoint(socksAddress, socksPort);
            Func<AsyncCallback, object, IAsyncResult> begin =
                (cb, s) => socket.BeginConnect(endPoint, cb, s);
            await Task.Factory.FromAsync(begin, socket.EndConnect, null);

            await OpenConnectionFromSocks(socket, version, serverAddress, serverPort, username, password);
            return socket;
        }

        public static async Task OpenConnectionFromSocks(Socket socket, int version, IPAddress serverAddress, int serverPort,
            string username = null, string password = null)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("Socket cannot be null");
            }
            else if (!socket.Connected)
            {
                throw new ArgumentException("Socket has to be connected");
            }

            if (version == 5)
            {
                await HandleSocks5(socket, serverAddress, serverPort, username, password);
            }
            else if (version == 4)
            {
                await HandleSocks4(socket, serverAddress, serverPort);
            }
        }

        private static async Task SendAsync(Socket socket, byte[] buffer, int offset, int count)
        {
            Func<AsyncCallback, object, IAsyncResult> begin =
                (cb, s) => socket.BeginSend(buffer, offset, count, SocketFlags.None, cb, s);
            await Task.Factory.FromAsync(begin, socket.EndSend, null);
        }

        private static async Task ReceiveAsync(Socket socket, byte[] buffer, int offset, int count)
        {
            Func<AsyncCallback, object, IAsyncResult> begin =
                (cb, s) => socket.BeginReceive(buffer, offset, count, SocketFlags.None, cb, s);
            await Task.Factory.FromAsync(begin, socket.EndReceive, null);
        }

        private static async Task HandleSocks5(Socket socket, IPAddress serverAddress, int serverPort, string username, string password)
        {
            byte[] buffer = new byte[1024];

            buffer[0] = 0x05;

            if (username != null)
            {
                buffer[1] = 0x02;
                buffer[2] = 0x00;
                buffer[3] = 0x02;
                await SendAsync(socket, buffer, 0, 4);
            }
            else
            {
                buffer[1] = 0x01;
                buffer[2] = 0x00;
                await SendAsync(socket, buffer, 0, 3);
            }


            await ReceiveAsync(socket, buffer, 0, 2);

            if (buffer[0] != 0x05)
            {
                throw new Exception("Received invalid version from the proxy server");
            }

            if (buffer[1] == 0x02)
            {
                byte[] usernameArray = Encoding.ASCII.GetBytes(username);
                byte[] passwordArray = Encoding.ASCII.GetBytes(password);

                int i = 0;
                buffer[i++] = 0x01;

                buffer[i++] = (byte)username.Length;
                Array.Copy(usernameArray, 0, buffer, i, username.Length);
                i += username.Length;

                buffer[i++] = (byte)password.Length;
                Array.Copy(passwordArray, 0, buffer, i, password.Length);
                i += password.Length;

                await SendAsync(socket, buffer, 0, i);
                await ReceiveAsync(socket, buffer, 0, 2);

                if (buffer[0] != 1)
                {
                    throw new Exception("Received invalid authentication version from the proxy server");
                }

                if (buffer[1] != 0)
                {
                    throw new Exception("The proxy server has refused the username/password authentication");
                }
            }
            else if (buffer[1] != 0x00)
            {
                throw new Exception("Received invalid authentication method from the proxy server");
            }

            byte[] address = serverAddress.GetAddressBytes();
            byte[] port = BitConverter.GetBytes((ushort)serverPort);
            Array.Reverse(port);

            buffer[0] = 0x05;
            buffer[1] = 0x01;
            buffer[2] = 0x00;
            buffer[3] = 0x01;
            Array.Copy(address, 0, buffer, 4, 4);
            Array.Copy(port, 0, buffer, 8, 2);
            await SendAsync(socket, buffer, 0, 10);
            await ReceiveAsync(socket, buffer, 0, 10);

            if (buffer[0] != 5)
            {
                throw new Exception("Received invalid version from the proxy server");
            }
            if (buffer[1] != 0)
            {
                throw new Exception("Received connection failure from the proxy server");
            }
            if (buffer[3] != 0x01)
            {
                throw new Exception("Received invalid address type from the proxy server");
            }
        }

        private static async Task HandleSocks4(Socket socket, IPAddress serverAddress, int serverPort)
        {
            byte[] buffer = new byte[1024];

            byte[] address = serverAddress.GetAddressBytes();
            byte[] port = BitConverter.GetBytes((ushort)serverPort);
            Array.Reverse(port);

            buffer[0] = 0x04;
            buffer[1] = 0x01;
            Array.Copy(port, 0, buffer, 2, 2);
            Array.Copy(address, 0, buffer, 4, 4);
            buffer[8] = 0x00;
            await SendAsync(socket, buffer, 0, 9);
            await ReceiveAsync(socket, buffer, 0, 8);

            if (buffer[0] != 0)
            {
                throw new Exception("Received invalid header from the proxy server");
            }
            if (buffer[1] != 0x5a)
            {
                throw new Exception("The proxy server rejected the connection");
            }
        }
    }
}