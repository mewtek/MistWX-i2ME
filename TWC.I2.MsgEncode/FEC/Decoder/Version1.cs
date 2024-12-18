using System;
using System.Diagnostics;
using System.IO;

namespace TWC.I2.MsgEncode.FEC.Decoder
{
    internal class Version1 : FecDecoder
    {
        private byte gridWidth;
        private byte minGridWidth;
        private byte maxGridWidth;

        public Version1(
          Stream inputStream,
          ushort packetSize,
          uint filesize,
          byte minGridWidth,
          byte maxGridWidth)
          : base(inputStream, packetSize, filesize)
        {
            this.gridWidth = (byte)0;
            this.minGridWidth = minGridWidth;
            this.maxGridWidth = maxGridWidth;
            this.AllocateBuffers(Math.Max((int)maxGridWidth * (int)maxGridWidth + 2 * (int)maxGridWidth, 3));
        }

        public override bool Decode(Stream outputStream)
        {
            outputStream.SetLength(0L);
            uint num1 = this.Filesize;
            uint num2 = (uint)((int)num1 + (int)this.DataSize - 1) / (uint)this.DataSize;
            this.PacketsRead = 0U;
            while (num2 > 0U)
            {
                bool flag = false;
                this.gridWidth = (byte)0;
                for (byte maxGridWidth = this.maxGridWidth; !flag && (int)maxGridWidth >= (int)this.minGridWidth; --maxGridWidth)
                {
                    uint num3 = (uint)maxGridWidth * (uint)maxGridWidth;
                    uint num4 = 2U * (uint)maxGridWidth;
                    uint numBuffers = num3 + num4;
                    if (num2 > num3)
                    {
                        this.ReadPackets(this.InputStream, numBuffers, (uint)this.DataSize * numBuffers);
                        this.gridWidth = maxGridWidth;
                        num1 -= (uint)this.DataSize * num3;
                        num2 -= num3;
                        flag = true;
                    }
                    else if ((int)num2 == (int)num3)
                    {
                        this.ReadPackets(this.InputStream, numBuffers, num1 + (uint)this.DataSize * num4);
                        this.gridWidth = maxGridWidth;
                        num1 = 0U;
                        num2 = 0U;
                        flag = true;
                    }
                }
                if (!flag)
                {
                    switch (num2)
                    {
                        case 1:
                            this.ReadPackets(this.InputStream, 2U, num1 + (uint)this.DataSize);
                            num1 = 0U;
                            num2 = 0U;
                            flag = true;
                            break;
                        case 2:
                            this.ReadPackets(this.InputStream, 3U, num1 + (uint)this.DataSize);
                            num1 = 0U;
                            num2 = 0U;
                            flag = true;
                            break;
                        default:
                            this.ReadPackets(this.InputStream, 3U, (uint)this.DataSize * 3U);
                            num1 -= (uint)this.DataSize * 2U;
                            num2 -= 2U;
                            flag = true;
                            break;
                    }
                }
                if (!flag || !this.AttemptDecode(outputStream))
                    return false;
            }
            return true;
        }

        private bool AttemptDecode(Stream outputStream)
        {
            if (this.decodebuffercount == 2U)
            {
                if (this.validbuffers[0])
                {
                    outputStream.Write(this.buffers[0], 0, (int)this.buffersize[1]);
                    return true;
                }
                if (this.validbuffers[1])
                {
                    outputStream.Write(this.buffers[1], 0, (int)this.buffersize[1]);
                    return true;
                }
            }
            else if (this.decodebuffercount == 3U)
            {
                if (this.validbuffercount >= 2)
                {
                    if (!this.validbuffers[0])
                    {
                        this.XOrBuffer(0, 1);
                        this.XOrBuffer(0, 2);
                    }
                    outputStream.Write(this.buffers[0], 0, (int)this.buffersize[0]);
                    if (!this.validbuffers[2])
                    {
                        this.XOrBuffer(2, 0);
                        this.XOrBuffer(2, 1);
                    }
                    outputStream.Write(this.buffers[2], 0, (int)this.buffersize[2]);
                    return true;
                }
            }
            else
            {
                bool flag1 = true;
                for (int index = 2 * (int)this.gridWidth; (long)index < (long)this.decodebuffercount; ++index)
                {
                    if (!this.validbuffers[index])
                        flag1 = false;
                }
                if (!flag1)
                {
                    bool flag2;
                    do
                    {
                        flag2 = false;
                        for (int index = 0; index < (int)this.gridWidth; ++index)
                            flag2 = flag2 | this.MakeValidGridRow(index) | this.MakeValidGridColumn(index);
                    }
                    while (flag2);
                }
                bool flag3 = true;
                for (int index = 2 * (int)this.gridWidth; (long)index < (long)this.decodebuffercount; ++index)
                {
                    if (!this.validbuffers[index])
                        flag3 = false;
                }
                if (flag3)
                {
                    for (int index = 2 * (int)this.gridWidth; (long)index < (long)this.decodebuffercount; ++index)
                        outputStream.Write(this.buffers[index], 0, (int)this.buffersize[index]);
                    return true;
                }
            }
            return false;
        }

        private bool MakeValidGridRow(int row)
        {
            int dest = -1;
            if (!this.validbuffers[row])
                dest = row;
            for (int col = 0; col < (int)this.gridWidth; ++col)
            {
                int playloadBufferIndex = this.GetPlayloadBufferIndex(col, row);
                if (!this.validbuffers[playloadBufferIndex])
                {
                    if (dest != -1)
                        return false;
                    dest = playloadBufferIndex;
                }
            }
            if (dest == -1)
                return false;
            this.XOrBuffer(dest, row);
            for (int col = 0; col < (int)this.gridWidth; ++col)
            {
                int playloadBufferIndex = this.GetPlayloadBufferIndex(col, row);
                if (dest != playloadBufferIndex)
                    this.XOrBuffer(dest, playloadBufferIndex);
            }
            this.validbuffers[dest] = true;
            ++this.validbuffercount;
            return true;
        }

        private bool MakeValidGridColumn(int col)
        {
            int dest = -1;
            if (!this.validbuffers[(int)this.gridWidth + col])
                dest = (int)this.gridWidth + col;
            for (int row = 0; row < (int)this.gridWidth; ++row)
            {
                int playloadBufferIndex = this.GetPlayloadBufferIndex(col, row);
                if (!this.validbuffers[playloadBufferIndex])
                {
                    if (dest != -1)
                        return false;
                    dest = playloadBufferIndex;
                }
            }
            if (dest == -1)
                return false;
            this.XOrBuffer(dest, (int)this.gridWidth + col);
            for (int row = 0; row < (int)this.gridWidth; ++row)
            {
                int playloadBufferIndex = this.GetPlayloadBufferIndex(col, row);
                if (dest != playloadBufferIndex)
                    this.XOrBuffer(dest, playloadBufferIndex);
            }
            this.validbuffers[dest] = true;
            ++this.validbuffercount;
            return true;
        }

        private void DumpLine(int blocks)
        {
            int num = 0;
            while (num < blocks)
                ++num;
        }

        private void DumpPackets()
        {
            this.DumpLine((int)this.gridWidth + 1);
            for (int row = 0; row < (int)this.gridWidth; ++row)
            {
                for (int col = 0; col < (int)this.gridWidth; ++col)
                    this.GetPlayloadBufferIndex(col, row);
                this.DumpLine((int)this.gridWidth + 1);
            }
            int num = 0;
            while (num < (int)this.gridWidth)
                ++num;
            this.DumpLine((int)this.gridWidth);
        }

        private void FecBuffer(int b1, int b2, int p1)
        {
            this.XOrBuffer(p1, b1);
            this.XOrBuffer(p1, b2);
        }

        private void FecRow(int row)
        {
            for (int col = 0; col < (int)this.gridWidth; ++col)
            {
                int playloadBufferIndex = this.GetPlayloadBufferIndex(col, row);
                this.XOrBuffer(row, playloadBufferIndex);
            }
        }

        private void FecColumn(int col)
        {
            for (int row = 0; row < (int)this.gridWidth; ++row)
            {
                int playloadBufferIndex = this.GetPlayloadBufferIndex(col, row);
                this.XOrBuffer((int)this.gridWidth + row, playloadBufferIndex);
            }
        }

        private int GetPlayloadBufferIndex(int col, int row)
        {
            return (int)this.gridWidth * 2 + row * (int)this.gridWidth + col;
        }

        [Conditional("DEBUG")]
        private void DebugMessage(string msg)
        {
            StackFrame stackFrame = new StackFrame(1, true);
            stackFrame.GetFileName();
            stackFrame.GetFileLineNumber();
        }
    }
}
