using System;
using System.IO;
using System.Net;

namespace TWC.Util.DataProvider
{
    public class StreamDataProvider : Disposable, IDataProvider, IDisposable
    {
        private Stream stream;
        private long offset;
        private long length;

        public StreamDataProvider(Stream stream)
          : this(stream, 0L, stream.Length)
        {
        }

        public StreamDataProvider(Stream stream, long offset)
          : this(stream, offset, stream.Length - offset)
        {
        }

        public StreamDataProvider(Stream stream, long offset, long length)
        {
            this.stream = stream;
            this.offset = offset;
            this.length = length;
        }

        public static StreamDataProvider MakeFromUri(string uri)
        {
            return new StreamDataProvider(new WebClient().OpenRead(uri));
        }

        public uint Count
        {
            get
            {
                return (uint)(this.length - this.offset);
            }
        }

        public uint CountRemaining
        {
            get
            {
                return (uint)(this.offset + this.length - this.stream.Position);
            }
        }

        public void Start()
        {
            this.stream.Position = this.offset;
        }

        public void GetNextBytes(ref byte[] buf, uint pos, uint count)
        {
            int num;
            for (int index = this.stream.Read(buf, (int)pos, (int)count); (long)index < (long)count; index += num)
                num = this.stream.Read(buf, (int)((long)pos + (long)index), (int)((long)count - (long)index));
        }

        protected override void DisposeManaged()
        {
            if (this.stream == null)
                return;
            this.stream.Close();
            this.stream = (Stream)null;
        }
    }
}
