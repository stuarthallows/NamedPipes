using System.IO;
using System.Text;

namespace NamedPipeShared
{
    // Defines the data protocol for reading and writing strings on our stream
    public class StreamString
    {
        private readonly Stream _ioStream;
        private readonly ASCIIEncoding _streamEncoding = new ASCIIEncoding();

        public StreamString(Stream ioStream)
        {
            _ioStream = ioStream;
        }

        public string ReadString()
        {
            var len = _ioStream.ReadByte() * 256;
            len += _ioStream.ReadByte();
            var inBuffer = new byte[len];
            _ioStream.Read(inBuffer, 0, len);

            return _streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = _streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            
            if (len > ushort.MaxValue)
            {
                len = ushort.MaxValue;
            }

            _ioStream.WriteByte((byte)(len / 256));
            _ioStream.WriteByte((byte)(len & 255));
            _ioStream.Write(outBuffer, 0, len);
            _ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}