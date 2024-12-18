using System;
using System.IO;
using TWC.I2.MsgEncode.FEC.Encoder;
using TWC.Util;

namespace TWC.I2.MsgEncode.FEC
{
    public abstract class FecEncoder
    {
        public static string TempFolder;
        public static string FileName;
        protected byte[][] buffers;
        private FecPacketHeader header;
        private FecEncoding encoding;
        private uint filesize;
        private uint packetsSent;
        private byte minGridWidth;
        private byte maxGridWidth;
        private ushort packetSize;

        public static uint EncodeFile(
          FecEncoding encoding,
          ushort packetSize,
          byte minGridWidth,
          byte maxGridWidth,
          string fname)
        {
            using (BufferedStream bufferedStream = new BufferedStream((Stream)new FileStream(fname, FileMode.Open, FileAccess.ReadWrite)))
                return FecEncoder.Create(encoding, packetSize, minGridWidth, maxGridWidth).Encode((Stream)bufferedStream, (Stream)bufferedStream);
        }

        public static uint EncodeFile(
          FecEncoding encoding,
          ushort packetSize,
          byte minGridWidth,
          byte maxGridWidth,
          string fname,
          string tempFolder)
        {
            FecEncoder.TempFolder = tempFolder;
            FecEncoder.FileName = fname;
            using (BufferedStream bufferedStream = new BufferedStream((Stream)new FileStream(fname, FileMode.Open, FileAccess.ReadWrite)))
                return FecEncoder.Create(encoding, packetSize, minGridWidth, maxGridWidth).Encode((Stream)bufferedStream, (Stream)bufferedStream);
        }

        public static FecEncoder Create(
          FecEncoding encoding,
          ushort packetSize,
          byte minGridWidth,
          byte maxGridWidth)
        {
            switch (encoding)
            {
                case FecEncoding.None:
                    return (FecEncoder)new None(packetSize);
                case FecEncoding.Version1:
                    return (FecEncoder)new Version1(packetSize, minGridWidth, maxGridWidth);
                default:
                    return (FecEncoder)null;
            }
        }

        public ushort PacketSize
        {
            get
            {
                return this.packetSize;
            }
        }

        public ushort HeaderSize
        {
            get
            {
                return 9;
            }
        }

        public ushort DataSize
        {
            get
            {
                return (ushort)((uint)this.PacketSize - (uint)this.HeaderSize);
            }
        }

        public FecEncoding Encoding
        {
            get
            {
                return this.encoding;
            }
        }

        public byte MinGridWidth
        {
            get
            {
                return this.minGridWidth;
            }
        }

        public byte MaxGridWidth
        {
            get
            {
                return this.maxGridWidth;
            }
        }

        public uint Encode(string src, string dst)
        {
            using (Stream inputStream = (Stream)File.OpenRead(src))
            {
                using (Stream outputStream = (Stream)File.OpenWrite(dst))
                    return this.Encode(inputStream, outputStream);
            }
        }

        public uint Encode(Stream inputStream, Stream outputStream)
        {
            string tempFileName = Toolbox.GetTempFileName(FecEncoder.TempFolder, "stream");
            try
            {
                using (BufferedStream bufferedStream = new BufferedStream((Stream)new FileStream(tempFileName, FileMode.OpenOrCreate)))
                {
                    uint num1 = this.InternalEncode(inputStream, (Stream)bufferedStream);
                    bufferedStream.Seek(0L, SeekOrigin.Begin);
                    if (this.MaxGridWidth > (byte)0)
                    {
                        int num2 = ((int)this.MaxGridWidth + 1) * ((int)this.MaxGridWidth + 1) - 1;
                        int num3 = 0;
                        int num4 = 0;
                        uint num5 = 0;
                        while (num5 < num1)
                        {
                            int num6 = num4 * num2 + num3;
                            if ((long)num6 > (long)num1)
                            {
                                ++num3;
                                num4 = 0;
                                num6 = num4 * num2 + num3;
                            }
                            outputStream.Seek((long)(num6 * (int)this.PacketSize), SeekOrigin.Begin);
                            Toolbox.CopyStream((Stream)bufferedStream, outputStream, (long)this.PacketSize);
                            ++num5;
                            ++num4;
                        }
                    }
                    else
                        Toolbox.CopyStream((Stream)bufferedStream, outputStream);
                    return num1;
                }
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        protected abstract uint InternalEncode(Stream inputStream, Stream outputStream);

        protected FecEncoder(
          FecEncoding encoding,
          ushort packetSize,
          byte minGridWidth,
          byte maxGridWidth)
        {
            this.packetSize = packetSize;
            this.minGridWidth = minGridWidth;
            this.maxGridWidth = maxGridWidth;
            this.encoding = encoding;
            this.header = new FecPacketHeader();
            this.filesize = 0U;
            this.packetsSent = 0U;
            this.buffers = new byte[this.BufferCount][];
            for (int index = 0; index < this.buffers.Length; ++index)
                this.buffers[index] = new byte[(int)this.DataSize];
        }

        protected abstract int BufferCount { get; }

        protected uint PacketsSent
        {
            get
            {
                return this.packetsSent;
            }
            set
            {
                this.packetsSent = value;
            }
        }

        protected uint Filesize
        {
            get
            {
                return this.filesize;
            }
            set
            {
                this.filesize = value;
            }
        }

        protected void WritePacket(Stream outputStream, int buffer)
        {
            this.header.Set((uint)((ulong)this.packetsSent + (ulong)buffer + 1UL), this.Encoding, this.MinGridWidth, this.MaxGridWidth, this.filesize);
            this.header.Write(outputStream);
            outputStream.Write(this.buffers[buffer], 0, (int)this.DataSize);
        }

        protected void ClearBuffer(int dest)
        {
            Array.Clear((Array)this.buffers[dest], 0, this.buffers[dest].Length);
        }

        protected void XOrBuffer(int dest, int source)
        {
            for (int index = 0; index < (int)this.DataSize; ++index)
                this.buffers[dest][index] ^= this.buffers[source][index];
        }
    }
}
