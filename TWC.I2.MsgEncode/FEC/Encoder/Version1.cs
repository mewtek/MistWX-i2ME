using System.IO;

namespace TWC.I2.MsgEncode.FEC.Encoder
{
    internal class Version1 : FecEncoder
    {
        public Version1(ushort packetSize, byte minGridWidth, byte maxGridWidth)
          : base(FecEncoding.Version1, packetSize, minGridWidth, maxGridWidth)
        {
        }

        protected override int BufferCount
        {
            get
            {
                return (int)this.MaxGridWidth * (int)this.MaxGridWidth + 2 * (int)this.MaxGridWidth;
            }
        }

        protected override uint InternalEncode(Stream inputStream, Stream outputStream)
        {
            inputStream.Seek(0L, SeekOrigin.Begin);
            outputStream.SetLength(0L);
            uint decodesize = (uint)inputStream.Length;
            uint num1 = (uint)((int)decodesize + (int)this.DataSize - 1) / (uint)this.DataSize;
            this.Filesize = decodesize;
            this.PacketsSent = 0U;
            while (num1 > 0U)
            {
                bool flag = false;
                for (byte maxGridWidth = this.MaxGridWidth; !flag && (int)maxGridWidth >= (int)this.MinGridWidth; --maxGridWidth)
                {
                    uint num2 = (uint)maxGridWidth * (uint)maxGridWidth;
                    uint num3 = 2U * (uint)maxGridWidth;
                    uint num4 = num2 + num3;
                    if (num1 > num2)
                    {
                        this.FecEncodeGridPackets(inputStream, outputStream, maxGridWidth, (uint)this.DataSize * num2);
                        decodesize -= (uint)this.DataSize * num2;
                        num1 -= num2;
                        flag = true;
                        this.PacketsSent += num4;
                    }
                    else if ((int)num1 == (int)num2)
                    {
                        this.FecEncodeGridPackets(inputStream, outputStream, maxGridWidth, decodesize);
                        decodesize = 0U;
                        num1 = 0U;
                        flag = true;
                        this.PacketsSent += num4;
                    }
                }
                if (!flag)
                {
                    switch (num1)
                    {
                        case 1:
                            this.FecEncodeOnePacket(inputStream, outputStream, decodesize);
                            decodesize = 0U;
                            num1 = 0U;
                            flag = true;
                            this.PacketsSent += 2U;
                            break;
                        case 2:
                            this.FecEncodeTwoPackets(inputStream, outputStream, decodesize);
                            decodesize = 0U;
                            num1 = 0U;
                            flag = true;
                            this.PacketsSent += 3U;
                            break;
                        default:
                            this.FecEncodeTwoPackets(inputStream, outputStream, (uint)this.DataSize * 2U);
                            decodesize -= (uint)this.DataSize * 2U;
                            num1 -= 2U;
                            flag = true;
                            this.PacketsSent += 3U;
                            break;
                    }
                }
                if (!flag)
                    return 0;
            }
            return this.PacketsSent;
        }

        private void FecEncodeOnePacket(Stream inputStream, Stream outputStream, uint decodesize)
        {
            this.ClearBuffer(0);
            inputStream.Read(this.buffers[0], 0, (int)decodesize);
            this.WritePacket(outputStream, 0);
            this.WritePacket(outputStream, 0);
        }

        private void FecEncodeTwoPackets(Stream inputStream, Stream outputStream, uint decodesize)
        {
            this.ClearBuffer(1);
            this.ClearBuffer(2);
            inputStream.Read(this.buffers[0], 0, (int)this.DataSize);
            inputStream.Read(this.buffers[2], 0, (int)decodesize - (int)this.DataSize);
            this.FecBuffer((byte)0, (byte)2, (byte)1);
            this.WritePacket(outputStream, 0);
            this.WritePacket(outputStream, 1);
            this.WritePacket(outputStream, 2);
        }

        private void FecEncodeGridPackets(
          Stream inputStream,
          Stream outputStream,
          byte gridWidth,
          uint decodesize)
        {
            for (byte index = 0; (int)index < this.buffers.Length; ++index)
                this.ClearBuffer((int)index);
            int num1 = 2 * (int)gridWidth;
            uint num2;
            for (num2 = decodesize; num2 >= (uint)this.DataSize; num2 -= (uint)this.DataSize)
                inputStream.Read(this.buffers[num1++], 0, (int)this.DataSize);
            if (num2 > 0U)
            {
                Stream stream = inputStream;
                byte[][] buffers = this.buffers;
                int index = num1;
                int num3 = index + 1;
                byte[] buffer = buffers[index];
                int count = (int)num2;
                stream.Read(buffer, 0, count);
            }
            for (int index = 0; index < (int)gridWidth; ++index)
            {
                this.FecRow((int)gridWidth, index);
                this.FecColumn((int)gridWidth, index);
            }
            for (int index = 0; index <= (int)gridWidth; ++index)
            {
                int col = index;
                for (int row = 0; row <= (int)gridWidth && col <= (int)gridWidth; ++col)
                {
                    int bufferIndex = this.GetBufferIndex((int)gridWidth, col, row);
                    if (bufferIndex >= 0)
                        this.WritePacket(outputStream, bufferIndex);
                    ++row;
                }
            }
            for (int index = 1; index <= (int)gridWidth; ++index)
            {
                int row = index;
                for (int col = 0; col <= (int)gridWidth && row <= (int)gridWidth; ++row)
                {
                    int bufferIndex = this.GetBufferIndex((int)gridWidth, col, row);
                    if (bufferIndex >= 0)
                        this.WritePacket(outputStream, bufferIndex);
                    ++col;
                }
            }
        }

        private void FecBuffer(byte b1, byte b2, byte p1)
        {
            this.XOrBuffer((int)p1, (int)b1);
            this.XOrBuffer((int)p1, (int)b2);
        }

        private void FecRow(int gridWidth, int row)
        {
            for (int col = 0; col < gridWidth; ++col)
            {
                int playloadBufferIndex = this.GetPlayloadBufferIndex(gridWidth, col, row);
                this.XOrBuffer(row, playloadBufferIndex);
            }
        }

        private void FecColumn(int gridWidth, int col)
        {
            for (int row = 0; row < gridWidth; ++row)
            {
                int playloadBufferIndex = this.GetPlayloadBufferIndex(gridWidth, col, row);
                this.XOrBuffer(gridWidth + col, playloadBufferIndex);
            }
        }

        private int GetBufferIndex(int gridWidth, int col, int row)
        {
            if (col < gridWidth && row < gridWidth)
                return this.GetPlayloadBufferIndex(gridWidth, col, row);
            if (row == gridWidth && col < gridWidth)
                return gridWidth + col;
            return col == gridWidth && row < gridWidth ? row : -1;
        }

        private int GetPlayloadBufferIndex(int gridWidth, int col, int row)
        {
            return gridWidth * 2 + row * gridWidth + col;
        }
    }
}
