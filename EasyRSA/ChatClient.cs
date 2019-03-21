using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Numerics;
using System.Linq;

namespace EasyRSA
{
    public class ChatClient
    {
        public static bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var boundary = (int)Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i += 2)
                if (number % i == 0)
                    return false;

            return true;
        }

        private static int Calculate_e(int d, int m)
        {
            int e = 10;

            while (true)
            {
                if ((e * d) % m == 1)
                    break;
                else
                    e++;
            }

            return e;
        }

        private static int Calculate_d(int m)
        {
            int d = m - 1;

            for (int i = 2; i <= m; i++)
                if ((m % i == 0) && (d % i == 0)) //если имеют общие делители
                {
                    d--;
                    i = 1;
                }

            return d;
        }

        static private string RSA_Encode(string s, long e, long n)
        {
            List<string> result = new List<string>();

            BigInteger bi;

            for (int i = 0; i < s.Length; i++)
            {
                bi = new BigInteger(s[i]);
                bi = BigInteger.Pow(bi, (int)e);

                BigInteger n_ = new BigInteger((int)n);

                bi = bi % n_;

                result.Add(bi.ToString());
            }

            return String.Join(";", result);
        }

        static private string RSA_Decode(string input, long d, long n)
        {
            string result = "";

            BigInteger bi;

            foreach (string item in input.Split(';'))
            {
                bi = new BigInteger(Convert.ToDouble(item));
                bi = BigInteger.Pow(bi, (int)d);

                BigInteger n_ = new BigInteger((int)n);

                bi = bi % n_;

                int index = Convert.ToInt32(bi.ToString());

                result += (char)index;
            }

            return result;
        }

        public static void StartChat(Action<string> chatWrite, ref Action<string> SendToChat, string ip, int port, bool is_server = false)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the   
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(ip);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint EndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket s = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                if (is_server)
                {
                    s.Bind(EndPoint);
                    s.Listen(10);

                    chatWrite("Ожидание подключения клиента...");
                    // Program is suspended while waiting for an incoming connection.  
                    s = s.Accept();

                } else
                {
                    s.Connect(EndPoint);
                }

                var rnd = new Random(s.Handle.ToInt32());
                int p = rnd.Next(10, 100), q = rnd.Next(10, 100);
                while (!IsPrime(++p));
                while (!IsPrime(++q) || p == q);
                int n = p * q;
                int m = (p - 1) * (q - 1);
                int d = Calculate_d(m);
                int e = Calculate_e(d, m);

                s.Send(Encoding.ASCII.GetBytes(String.Format("{0} {1}", e, n)));
                int bytesRec = s.Receive(bytes);
                var pubkey = Encoding.ASCII.GetString(bytes, 0, bytesRec).Split(' ');

                chatWrite(String.Format("Ваш собеседник: {0}",
                    s.RemoteEndPoint.ToString()));
                chatWrite(String.Format("Случайные числа: p = {0}, q = {1}",
                    p, q));
                chatWrite(String.Format("Ваш ключ: d = {0}, e = {1}, n = {2}",
                    d, e, n));
                chatWrite(String.Format("Ключ собеседника: e = {0}, n = {1}",
                    pubkey[0], pubkey[1]));

                SendToChat = message => {
                    chatWrite(String.Format("Я: {0}", message));
                    string enc = RSA_Encode(message, Int32.Parse(pubkey[0]), Int32.Parse(pubkey[1]));
                    chatWrite(String.Format("Я (зашифровано): {0}", enc));
                    s.Send(Encoding.ASCII.GetBytes(enc));
                };

                // An incoming connection needs to be processed.  
                while (true)
                {
                    bytesRec = s.Receive(bytes);
                    string enc = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    chatWrite(String.Format("Оно (зашифровано): {0}", enc));
                    string data = RSA_Decode(enc, d, n);
                    chatWrite(String.Format("Оно: {0}", data));
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                s.Shutdown(SocketShutdown.Both);
                s.Close();

            }
            catch (Exception e)
            {
                chatWrite(e.ToString());
            }

            chatWrite("Server closed!");

        }
    }
}
