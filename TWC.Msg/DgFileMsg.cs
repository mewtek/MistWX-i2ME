using System.IO;
using TWC.Util.DataProvider;

namespace TWC.Msg
{
    public class DgFileMsg : DgMsg
    {
        private DgMsg.Segment interestSeg = new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, (IDataProvider)new NullDataProvider());
        private DgMsg.Segment preSeg = new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, (IDataProvider)new NullDataProvider());
        private DgMsg.Segment fileSeg = new DgMsg.Segment(DgMsg.Segment.Type.FileSegment, (IDataProvider)new NullDataProvider());
        private DgMsg.Segment postSeg = new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, (IDataProvider)new NullDataProvider());

        public DgFileMsg() : base(DgPacket.MsgType.File)
        {
            this.AddSegment(this.interestSeg);
            this.AddSegment(this.preSeg);
            this.AddSegment(this.fileSeg);
            this.AddSegment(this.postSeg);
        }

        public string DestinationFileName
        {
            get
            {
                return this.fileSeg.fileName;
            }
            set
            {
                this.fileSeg.fileName = value;
            }
        }

        public void AddSegment(DgFileMsg.SegmentId segId, IDataProvider provider, DgPacket.Compression compressionType)
        {
            switch (segId)
            {
                case DgFileMsg.SegmentId.Interest:
                    this.interestSeg.provider = provider;
                    this.interestSeg.compressionType = compressionType;
                    break;
                case DgFileMsg.SegmentId.Pre:
                    this.preSeg.provider = provider;
                    this.preSeg.compressionType = compressionType;
                    break;
                case DgFileMsg.SegmentId.File:
                    this.fileSeg.provider = provider;
                    this.fileSeg.compressionType = compressionType;
                    break;
                case DgFileMsg.SegmentId.Post:
                    this.postSeg.provider = provider;
                    this.postSeg.compressionType = compressionType;
                    break;
            }
        }

        public void AddSegment(DgFileMsg.SegmentId segId, string s)
        {
            this.AddSegment(segId, (IDataProvider)new StringDataProvider(s), DgPacket.Compression.None);
        }

        public void AddSegment(DgFileMsg.SegmentId segId, Stream stream)
        {
            this.AddSegment(segId, stream, DgPacket.Compression.None);
        }

        public void AddSegment(DgFileMsg.SegmentId segId, Stream stream, DgPacket.Compression compressionType)
        {
            switch (compressionType)
            {
                case DgPacket.Compression.None:
                    this.AddSegment(segId, (IDataProvider)new StreamDataProvider(stream), compressionType);
                    break;
                case DgPacket.Compression.GZip:
                    this.AddSegment(segId, (IDataProvider)new GZipDataProvider(stream), compressionType);
                    break;
            }
        }

        public void AddSegmentFromUri(DgFileMsg.SegmentId segId, string uri)
        {
            this.AddSegmentFromUri(segId, uri, DgPacket.Compression.None);
        }

        public void AddSegmentFromUri(DgFileMsg.SegmentId segId, string uri, DgPacket.Compression compressionType)
        {
            switch (compressionType)
            {
                case DgPacket.Compression.None:
                    this.AddSegment(segId, (IDataProvider)StreamDataProvider.MakeFromUri(uri), compressionType);
                    break;
                case DgPacket.Compression.GZip:
                    this.AddSegment(segId, (IDataProvider)GZipDataProvider.MakeFromUri(uri), compressionType);
                    break;
            }
        }

        public enum SegmentId
        {
            Interest,
            Pre,
            File,
            Post,
        }
    }
}
