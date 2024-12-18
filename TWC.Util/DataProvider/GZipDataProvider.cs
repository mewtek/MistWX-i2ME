using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace TWC.Util.DataProvider
{
    public class GZipDataProvider : Disposable, IDataProvider, IDisposable
    {
        private StreamDataProvider dataStream;
        private string fileName;

        public GZipDataProvider(Stream stream)
        {
            this.CompressStream(stream);
        }

        public static GZipDataProvider MakeFromUri(string uri)
        {
            return new GZipDataProvider(new WebClient().OpenRead(uri));
        }

        private void CompressStream(Stream stream)
        {
            this.fileName = Toolbox.GetTempFileName("GZIPFile");
            FileStream fileStream = new FileStream(this.fileName, FileMode.Create, FileAccess.ReadWrite);
            GZipStream gzipStream = new GZipStream((Stream)fileStream, CompressionMode.Compress);
            Toolbox.CopyStream(stream, (Stream)gzipStream);
            this.dataStream = new StreamDataProvider((Stream)fileStream);
        }

        public uint Count
        {
            get
            {
                return this.dataStream.Count;
            }
        }

        public uint CountRemaining
        {
            get
            {
                return this.dataStream.CountRemaining;
            }
        }

        public void Start()
        {
            this.dataStream.Start();
        }

        public void GetNextBytes(ref byte[] buf, uint pos, uint count)
        {
            this.dataStream.GetNextBytes(ref buf, pos, count);
        }

        protected override void DisposeManaged()
        {
            if (this.dataStream == null)
                return;
            System.IO.File.Delete(this.fileName);
            this.dataStream.Dispose();
        }
    }
}
