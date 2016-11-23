using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPChat {
    class Client {
        #region Private Members
        private static          Socket      clientSocket    = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
        private static readonly string      IPstring        = "127.0.0.1";
        private static readonly IPAddress   serverIp        = IPAddress.Parse( IPstring );
        private static readonly IPEndPoint  serverEndPoint  = new IPEndPoint(serverIp,serverPort);
        private static          UdpClient   udpClient;
        private static          bool        isRunning       = true;
        private static readonly int         serverPort      = 58888;
        private static          int         payloadSize     = 0;
        private static readonly int         BUFFER_SIZE     = 1024;
        private static readonly int         TIMEOUT_MS      = 5000;
        private static          byte[]      payload         = new byte[ BUFFER_SIZE ];
        private static          string      clientName      = String.Empty;
        #endregion

        static void Main( string[] args ) {
            Console.WriteLine( "Enter 'exit' to close application" );
            while ( string.IsNullOrWhiteSpace( clientName ) ) {
                Console.Write( "Enter your name: " );
                clientName = Console.ReadLine();
            }
            while ( isRunning ) {
                using (udpClient = new UdpClient(serverEndPoint))
                if ( ValidInput() ) {
                    try {
                        udpClient.Send( payload, payloadSize );
                    } catch ( Exception excpt ) {
                        Console.WriteLine( excpt );
                    }
                }
            }
            udpClient.Dispose();
            clientSocket.Dispose();
            Environment.Exit( 0 );
        }

        private static bool ValidInput() {
            string input = null;
            while ( string.IsNullOrWhiteSpace( input ) ) {
                Console.Write( "Enter a message: " );
                input = Console.ReadLine();
            }

            if ( input == "exit" ) {
                isRunning = false; // dispose and exit
            } else if ( input.Length <= BUFFER_SIZE ) {
                payload = UTF8Encoding.UTF8.GetBytes( input );
                payloadSize = payload.Length;
                return true;
            } else {
                Console.WriteLine( $"Message longer than {BUFFER_SIZE} characters not allowed." );
            }
            return false;
        }
    }
}
