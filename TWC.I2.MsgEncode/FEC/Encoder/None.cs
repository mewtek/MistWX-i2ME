using System.IO;

namespace TWC.I2.MsgEncode.FEC.Encoder
{
    internal sealed class None : FecEncoder
    {
        public None(ushort packetSize)
          : base(FecEncoding.None, packetSize, (byte)0, (byte)0)
        {
        }

        protected override int BufferCount
        {
            get
            {
                return 1;
            }
        }

        protected override uint InternalEncode(Stream inputStream, Stream outputStream)
        {
            inputStream.Seek(0L, SeekOrigin.Begin);
            outputStream.SetLength(0L);
            uint num1 = (uint)inputStream.Length;
            uint num2 = (uint)((int)num1 + (int)this.DataSize - 1) / (uint)this.DataSize;
            this.Filesize = num1;
            this.PacketsSent = 0U;
            while (num2 > 0U)
            {
                if (num2 == 1U)
                {
                    this.ClearBuffer(0);
                    inputStream.Read(this.buffers[0], 0, (int)num1);
                    this.WritePacket(outputStream, 0);
                    num1 = 0U;
                    num2 = 0U;
                }
                else
                {
                    inputStream.Read(this.buffers[0], 0, (int)this.DataSize);
                    this.WritePacket(outputStream, 0);
                    num1 -= (uint)this.DataSize;
                    --num2;
                }
                ++this.PacketsSent;
            }
            return this.PacketsSent;
        }
    }
}
