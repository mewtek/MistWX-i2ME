using System.IO;

namespace TWC.I2.MsgEncode.FEC
{
    public class FecPacketHeader
    {
        private byte[] data = new byte[9];
        public const int Length = 9;

        public uint PacketId()
        {
            return (uint)((int)this.data[0] + ((int)this.data[1] << 8) + ((int)this.data[2] << 16) + ((int)this.data[3] << 24));
        }

        public FecEncoding Encoding()
        {
            return (FecEncoding)this.data[4];
        }

        public uint Filesize()
        {
            return (uint)((int)this.data[5] + ((int)this.data[6] << 8) + ((int)this.data[7] << 16) + ((int)this.data[8] << 24));
        }

        public byte MinGridWidth()
        {
            return this.data[5];
        }

        public byte MaxGridWidth()
        {
            return this.data[6];
        }

        public void Set(
          uint packetId,
          FecEncoding encoding,
          byte minGridWidth,
          byte maxGridWidth,
          uint filesize)
        {
            this.data[0] = (byte)(packetId & (uint)byte.MaxValue);
            this.data[1] = (byte)(packetId >> 8 & (uint)byte.MaxValue);
            this.data[2] = (byte)(packetId >> 16 & (uint)byte.MaxValue);
            this.data[3] = (byte)(packetId >> 24 & (uint)byte.MaxValue);
            this.data[4] = (byte)encoding;
            if (this.HasFilesize())
            {
                this.data[5] = (byte)(filesize & (uint)byte.MaxValue);
                this.data[6] = (byte)(filesize >> 8 & (uint)byte.MaxValue);
                this.data[7] = (byte)(filesize >> 16 & (uint)byte.MaxValue);
                this.data[8] = (byte)(filesize >> 24 & (uint)byte.MaxValue);
            }
            else
            {
                this.data[5] = minGridWidth;
                this.data[6] = maxGridWidth;
                this.data[7] = (byte)0;
                this.data[8] = (byte)0;
            }
        }

        public bool IsValid()
        {
            return this.PacketId() > 0U;
        }

        public bool HasFilesize()
        {
            return this.Encoding() == FecEncoding.None || this.PacketId() <= 4U || this.PacketId() % 2U == 1U;
        }

        public bool HasGridWidth()
        {
            return !this.HasFilesize();
        }

        public void Write(Stream stream)
        {
            stream.Write(this.data, 0, this.data.Length);
        }

        public void Read(Stream stream)
        {
            stream.Read(this.data, 0, this.data.Length);
        }

        public static bool Find(
          Stream stream,
          ushort packetSize,
          out FecEncoding encoding,
          out uint filesize,
          out byte minGridWidth,
          out byte maxGridWidth)
        {
            int num1 = (int)packetSize - 9;
            FecPacketHeader fecPacketHeader = new FecPacketHeader();
            bool flag1 = false;
            bool flag2 = false;
            minGridWidth = (byte)1;
            maxGridWidth = (byte)0;
            filesize = 0U;
            encoding = FecEncoding.None;
            stream.Seek(0L, SeekOrigin.Begin);
            while (stream.Position <= stream.Length - (long)packetSize)
            {
                fecPacketHeader.Read(stream);
                if (fecPacketHeader.IsValid() && fecPacketHeader.HasFilesize())
                {
                    flag1 = true;
                    encoding = fecPacketHeader.Encoding();
                    filesize = fecPacketHeader.Filesize();
                    uint num2 = (uint)((ulong)((long)filesize + (long)num1 - 1L) / (ulong)num1);
                    if (encoding == FecEncoding.None || num2 <= 4U)
                        flag2 = true;
                }
                if (fecPacketHeader.IsValid() && fecPacketHeader.HasGridWidth())
                {
                    flag2 = true;
                    encoding = fecPacketHeader.Encoding();
                    minGridWidth = fecPacketHeader.MinGridWidth();
                    maxGridWidth = fecPacketHeader.MaxGridWidth();
                }
                if (flag1 && flag2)
                    return true;
                stream.Seek((long)num1, SeekOrigin.Current);
            }
            return false;
        }
    }
}
