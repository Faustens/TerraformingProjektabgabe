using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Terraform_Server
{
    class Server
    {
        //===========================================
        //=             Klassenvariablen            =
        //===========================================
        static List<String> actionList = new List<String>();
        static Dictionary<int, TcpClient> clientList = new Dictionary<int, TcpClient>();
        static List<byte[]> statusList = new List<byte[]>();


        //=======================================
        //=             Main Methode            =
        //=======================================
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 4040);
            server.Start();

            //=======================================================
            //=     Verbinden von neuen Clients mit dem Server      =
            //=======================================================

            int clientID = -1;
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                clientList.Add(clientID+1, client);

                Console.WriteLine("Client " + (clientID+1) + " connected");

                // Starten eines Threads, der den akzeptierten Client 'bedient'
                new Thread(() => {
                    clientID += 1;
                    serveClient(client, clientID);
                }).Start();
            }

        }

        static void serveClient(TcpClient client, int ID)
        {
            // Network Stream öffnen
            NetworkStream ns = client.GetStream();
            String[] inSep;

            // ID an Clienten senden
            byte[] idBt = new byte[1024];
            idBt = Encoding.Default.GetBytes(Convert.ToString(ID));
            ns.Write(idBt, 0, idBt.Length);
            // aktueller Zustand an den neuen Client senden
            foreach (var msg in statusList)
            {
                try
                {
                    ns.Write(msg, 0, msg.Length);
                }
                catch (Exception) { }
            }

            // Während der andauernden Verbindung Nachrichten empfangen und der ActionList hinzufügen
            while (client.Connected)
            {
                try
                {
                    byte[] msg = new byte[1024];
                    ns.Read(msg, 0, msg.Length);
                    inSep = Encoding.Default.GetString(msg).Split(',');
                    Console.WriteLine("ID: " + inSep[1] + ", Vector3: (" + inSep[2] + "," + inSep[3] + "," + inSep[4] + ")");
                    statusList.Add(msg);

                    foreach (var clientEntry in clientList)
                    {
                        try
                        {
                            clientEntry.Value.GetStream().Write(msg, 0, msg.Length);
                            Console.WriteLine("Daten gesendet");
                        }
                        catch (Exception) { }
                    }

                }
                catch (Exception) { }
            }

            Console.WriteLine("Client " + ID + " disconnected");
            clientList.Remove(ID);
        }
    }
}
