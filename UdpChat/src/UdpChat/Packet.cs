using System.Collections.Generic;
using System.Text;
using static System.BitConverter;

namespace Server {
    public class Packet {
        #region Public Members
        public int NameLength { get; set; }
        public int MessageLength { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        #endregion
        #region Ctors
        public Packet() {
            this.NameLength = 0;
            this.MessageLength = 0;
            this.Name = null;
            this.Message = null;
        }
        public Packet( byte[] data ) { //Add seq number, on clientside add a list of all sent packages and remove them when server send acks
            NameLength = ToInt32( data, 0 );
            MessageLength = ToInt32( data, 4 );
            Name = Encoding.UTF8.GetString( data, 8, NameLength );
            Message = Encoding.UTF8.GetString( data, 8 + NameLength, MessageLength );
        }
        #endregion
        #region Methods
        public byte[] GetData() {
            List<byte> data = new List<byte>();

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
