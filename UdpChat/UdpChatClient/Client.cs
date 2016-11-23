using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPChat {
    class Client {
        #region Private Members
        private                 Socket      clientSocket;
        private static readonly int         serverPort      = 58888;
        private static readonly string      IPstring        = "192.168.1.88";
        private static readonly IPAddress   serverIp        = IPAddress.Parse( IPstring );
        private                 EndPoint    serverEndPoint;
        private                 bool        isRunning       = true;
        private static readonly int         BUFFER_SIZE     = 1024;
        private                 byte[]      buffer          = new byte[ BUFFER_SIZE ];
        private                 string      clientName      = String.Empty;
        #endregion

        static void Main( string[] args ) {
            new Client().ClientLoop();
        }

        private void ClientLoop() {
            while ( string.IsNullOrWhiteSpace( clientName ) ) {
                Console.Write( "Enter your name: " );
                clientName = Console.ReadLine().Trim();
            }
            //Initial packet telling server and other user this client has connected
            var loginData = new Packet {
                PacketType = PacketType.Login,
                Name = clientName,
                NameLength = clientName.Length,
                Message = null,
            }.GetData();
            //Initialize
            try {
                clientSocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
                IPEndPoint serverIpEp = new IPEndPoint( serverIp, serverPort );
                serverEndPoint = ( EndPoint ) serverIpEp;
                Console.WriteLine( "Enter 'exit' to close application" );
                clientSocket.BeginSendTo( loginData, 0, loginData.Length, SocketFlags.None, serverEndPoint, new AsyncCallback( SendData ), null );
                this.buffer = new byte[1024];
                //TODO: wait for ack from server then continue.
                clientSocket.BeginReceiveFrom( buffer, 0, buffer.Length, SocketFlags.None, ref serverEndPoint, new AsyncCallback( this.RecvData ), null );
            } catch ( Exception excpt ) {
                Console.WriteLine( excpt );
            }
            while ( isRunning ) {
                try {
                    string input = string.Empty;
                    while ( string.IsNullOrEmpty( input ) ) {
                        Console.Write( "Enter a message: " );
                        input = Console.ReadLine();
                    }
                    if ( input == "exit" ) {
                        isRunning = false;
                        CloseAndDispose();
                        break;
                    }
                    var sendData = new Packet {
                        PacketType = PacketType.Message,
                        Name = clientName,
                        NameLength = clientName.Length,
                        Message = input,
                        MessageLength = input.Length
                    }.GetData();
                    clientSocket.BeginSendTo( sendData, 0, sendData.Length, SocketFlags.None, serverEndPoint, new AsyncCallback( SendData ), null );
                    this.buffer = new byte[1024];
                    clientSocket.BeginReceiveFrom( buffer, 0, buffer.Length, SocketFlags.None, ref serverEndPoint, new AsyncCallback( this.SendData ), null );
                } catch ( Exception excpt ) {
                    Console.WriteLine( excpt );
                }
            }

            //send logout msg and wait for ack from server
            CloseAndDispose();
        }



        private void RecvData( IAsyncResult ar ) {
            try {
                clientSocket.EndReceive( ar );
                Packet recvData = new Packet(this.buffer);
                if ( recvData.Message != null ) {
                    Console.WriteLine( recvData.Message );
                }
                this.buffer = new byte[1024];
                clientSocket.BeginReceiveFrom( this.buffer, 0, this.buffer.Length, SocketFlags.None, ref serverEndPoint, new AsyncCallback( RecvData ), null );
            } catch ( Exception excpt ) {
                Console.WriteLine( excpt );
            }
        }

        private void SendData( IAsyncResult ar ) {
            try {
                clientSocket.EndSend( ar );
            } catch ( Exception excpt ) {
                Console.WriteLine( excpt );
            }
        }
        private void CloseAndDispose() {
            Console.WriteLine( "Client shutting down in 1 second..." );
            //Send disconnection message to server
            var logoutData = new Packet {
                PacketType = PacketType.Logout,
                Name = clientName,
                NameLength = clientName.Length,
                Message = null,
            }.GetData();
            clientSocket.SendTo( logoutData, 0, logoutData.Length, SocketFlags.None, serverEndPoint);
            //TODO: wait for ack or resend disconnection msg

            Thread.Sleep( 800 );
            if ( clientSocket != null ) {
                clientSocket.Close();
                clientSocket.Dispose();
            }
            Environment.Exit( 0 );
        }
    }
}
