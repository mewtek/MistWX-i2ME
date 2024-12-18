using System.IO;
using TWC.Util.DataProvider;

namespace TWC.Msg
{
    public class DgCmdMsg : DgMsg
    {
        public DgCmdMsg() : base(DgPacket.MsgType.Command)
        {
        }

        public DgCmdMsg(string[] segs) : base(DgPacket.MsgType.Command)
        {
            foreach (string seg in segs)
                this.AddSegment(seg);
        }

        public DgCmdMsg(IDataProvider[] segs) : base(DgPacket.MsgType.Command)
        {
            foreach (IDataProvider seg in segs)
                this.AddSegment(seg);
        }

        public void AddSegment(IDataProvider dp)
        {
            this.AddSegment(new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, dp));
        }

        public void AddSegment(string str)
        {
            this.AddSegment((IDataProvider)new StringDataProvider(str));
        }

        public void AddSegment(Stream stream)
        {
            this.AddSegment((IDataProvider)new StreamDataProvider(stream));
        }

        public void AddSegmentFromUri(string uri)
        {
            this.AddSegment((IDataProvider)StreamDataProvider.MakeFromUri(uri));
        }
    }
}
