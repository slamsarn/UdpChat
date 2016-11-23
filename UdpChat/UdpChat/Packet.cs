using System.Collections.Generic;
using System.Text;
using static System.BitConverter;

namespace Server {
    public enum PacketType {
        Message,
        Login,
        Logout,
        Null
    }
    public class Packet {
        #region Public Members
        public PacketType PacketType { get; set; }
        public int NameLength { get; set; }
        public int MessageLength { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        #endregion
        #region Ctors
        public Packet() {
            this.PacketType = PacketType.Null;
            this.NameLength = 0;
            this.MessageLength = 0;
            this.Name = null;
            this.Message = null;
        }
        public Packet( byte[] data ) { //Add seq number, Ack func, clientside saves packets until ack
            this.PacketType = ( PacketType ) ToInt32( data, 0 );
            NameLength = ToInt32( data, 4 );
            MessageLength = ToInt32( data, 8 );

            if (this.NameLength > 0)
                Name = Encoding.UTF8.GetString( data, 12, NameLength );

            if(this.MessageLength > 0)
                Message = Encoding.UTF8.GetString( data, 12 + NameLength, MessageLength );
        }
        #endregion
        #region Methods
        public byte[] GetData() {
            List<byte> data = new List<byte>();
            //Add PacketType
            data.AddRange( GetBytes( ( int )this.PacketType) );

            //Add NameLength
            if ( this.Name != null )
                data.AddRange( GetBytes( this.Name.Length ) );
            else
                data.AddRange( GetBytes( 0 ) );

            //Add MessageLength
            if ( this.Message != null )
                data.AddRange( GetBytes( this.Message.Length ) );
            else
                data.AddRange( GetBytes( 0 ) );

            //Add Name
            if ( this.Name != null )
                data.AddRange( Encoding.UTF8.GetBytes( this.Name ) );

            //Add Message
            if ( this.Message != null )
                data.AddRange( Encoding.UTF8.GetBytes( this.Message ) );

            return data.ToArray();
        }
        #endregion
    }
}
