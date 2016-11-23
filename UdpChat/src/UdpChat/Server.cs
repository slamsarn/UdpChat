using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server {
    class UdpChatServer {
        #region TODOs
        /* List of clients
         * use list to track individual sequence numbers
         * ack all seq to respective client
         * if seq number is missing ask client to resend corresponding packet
         * on client side hold all sent packets until they get acks
         */
        #endregion

        private         Socket              sock            = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const   int                 PORT            = 58888;
        private static  string              IP              = "127.0.0.1";
        private static  IPAddress           serverIp        = IPAddress.Parse( IP );
        private         IPEndPoint          ipEp            = new IPEndPoint(serverIp,PORT);
        private         UdpClient           udpClient;
        private         Thread              recvThread;
        private         byte[]              recvData        = new byte[1024];
        private         bool                isRunning       = false;

        static void Main( string[] args ) {
            new UdpChatServer().Init(); //just to leave the static env.
        }

        private void Init() {
            recvThread = new Thread( new ThreadStart( RecvData ) );
        }

        private void RecvData() {
            using ( udpClient = new UdpClient( PORT ) ) {
                do
                    try {
                        ipEp = new IPEndPoint( IPAddress.Any, 0 );
                        recvData = udpClient.ReceiveFrom( ref ipEp );
                    } catch ( Exception ex ) {
                        ex.ToString();
                    }
                while ( isRunning );
            }
        }

        private Packet Deserialize( byte[] recvData ) {
            return new Packet( recvData );
        }
    }
}
