using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server {

    public class UdpChatServer {
        #region TODOs
        /* List of clients *DONE*
         * send login ack to client *DONE*
         * use list to track individual sequence numbers
         * ack all seq to respective client
         * if seq number is missing ask client to resend corresponding packet
         * on client side hold/save all sent packets until they get acks
         */
        #endregion

        private         Socket              serverSocket;
        private const   int                 serverPort      = 58888;
        private static  string              IP              = "192.168.1.88";
        private static  IPAddress           serverIp        = IPAddress.Parse( IP );
        private static  IPEndPoint          serverIpEp;
        private         byte[]              buffer          = new byte[1024];
        private         List<Client>        clientList;
        private         bool                isRunning       = true;

        public struct Client {
            public EndPoint EndPoint;
            public string Name;
        }

        static void Main( string[] args ) {
            new UdpChatServer().Init(); //just to leave the static env.
            Console.WriteLine( "Press any key to close server" );
            Console.ReadKey();
            Console.Clear();
            Console.WriteLine( "closing server in 3 sec..." );
            Thread.Sleep( 3000 );
        }

        private void Init() {
            clientList = new List<Client>();
            serverSocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
            serverIpEp = new IPEndPoint( IPAddress.Any, serverPort );
            serverSocket.Bind( serverIpEp );
            Console.WriteLine( "Server up and running" );
            try {
                IPEndPoint remoteIpEp = new IPEndPoint( IPAddress.Any, 0 );
                EndPoint senderEp = ( EndPoint ) remoteIpEp;
                serverSocket.BeginReceiveFrom( this.buffer, 0, this.buffer.Length, SocketFlags.None, ref senderEp, new AsyncCallback( RecvData ), senderEp );
            } catch ( Exception ex ) {
                ex.ToString();
            }
        }

        private void RecvData( IAsyncResult asyncResult ) {
            try {
                byte[] data;
                Packet recvData = new Packet(this.buffer);
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)clients;
                serverSocket.EndReceiveFrom( asyncResult, ref epSender );
                Console.WriteLine( recvData.Name + " " + recvData.Message );
                Packet sendData = new Packet {
                    PacketType = recvData.PacketType,
                    Name = recvData.Name
                };

                switch ( recvData.PacketType ) {
                    case PacketType.Message:
                        sendData.Message = $"{recvData.Name}: {recvData.Message}";
                        Console.WriteLine(sendData.Message);
                        break;
                    case PacketType.Login:
                        clientList.Add( new Client {
                            EndPoint = epSender,
                            Name = recvData.Name
                        } );
                        sendData.Message = $"{recvData.Name} has connected!";
                        break;
                    case PacketType.Logout:
                        foreach ( Client client in clientList ) {
                            if ( client.EndPoint.Equals( epSender ) ) {
                                this.clientList.Remove( client );
                                break;
                            }
                        }
                        sendData.Message = $"{recvData.Name} has disconnected!";
                        break;
                }
                data = sendData.GetData();

                foreach ( Client client in clientList ) {
                    if ( client.EndPoint != epSender) { //broadcast message to a
                        serverSocket.BeginSendTo( data, 0, data.Length, SocketFlags.None, client.EndPoint, new AsyncCallback( this.SendData ), client.EndPoint );
                    } else {
                        if ( sendData.PacketType == PacketType.Login ) {// send Ack to client
                            var ackData = new Packet { Message = "You have sucessfully connected to the server", Name = "Server", PacketType = PacketType.Null }.GetData();
                            serverSocket.BeginSendTo( ackData, 0, ackData.Length, SocketFlags.None, client.EndPoint, new AsyncCallback( this.SendData ), client.EndPoint );
                        }
                    }
                }

                serverSocket.BeginReceiveFrom( this.buffer, 0, this.buffer.Length, SocketFlags.None, ref epSender, new AsyncCallback( RecvData ), epSender );
                Console.WriteLine( "sending: " + sendData.Message );
            } catch ( Exception excpt ) {
                Console.WriteLine( excpt );
            }
        }

        private void SendData( IAsyncResult ar ) {
            try {
                serverSocket.EndSend( ar );
            } catch ( Exception excpt ) {
                Console.WriteLine( excpt );
            }
        }
    }
}
