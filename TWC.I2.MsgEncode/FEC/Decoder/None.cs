using System.IO;

namespace TWC.I2.MsgEncode.FEC.Decoder
{
    internal sealed class None : FecDecoder
    {
        public None(Stream inputStream, ushort packetSize, uint filesize)
          : base(inputStream, packetSize, filesize)
        {
            this.AllocateBuffers(1);
        }

        public override bool Decode(Stream outputStream)
        {
            outputStream.SetLength(0L);
            uint decodeSize = this.Filesize;
            uint num = (uint)((int)decodeSize + (int)this.DataSize - 1) / (uint)this.DataSize;
            this.PacketsRead = 0U;
            while (num > 0U)
            {
                if (num == 1U)
                {
                    this.ReadPackets(this.InputStream, 1U, decodeSize);
                    decodeSize = 0U;
                    num = 0U;
                }
                else
                {
                    this.ReadPackets(this.InputStream, 1U, (uint)this.DataSize);
                    decodeSize -= (uint)this.DataSize;
                    --num;
                }
                if (!this.AttemptDecode(outputStream))
                    return false;
            }
            return true;
        }

        private bool AttemptDecode(Stream outputStream)
        {
            if (this.decodebuffercount != 1U || !this.validbuffers[0])
                return false;
            outputStream.Write(this.buffers[0], 0, (int)this.buffersize[0]);
            return true;
        }
    }
}
