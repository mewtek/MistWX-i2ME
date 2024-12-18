using System.IO;
using TWC.Util.DataProvider;

namespace TWC.Msg
{
    public sealed class I2Msg : DgMsg
    {
        public I2Msg(string fname) : this((Stream)File.OpenRead(fname), (byte)3)
        {
        }

        public I2Msg(string fname, byte numCompletionPkts) : this((Stream)File.OpenRead(fname), numCompletionPkts)
        {
        }

        public I2Msg(Stream stream) : this(stream, (byte)0)
        {
        }

        public I2Msg(Stream stream, byte numCompletionPkts) : base(DgPacket.MsgType.I2)
        {
            this.AddSegment(new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, (IDataProvider)new StreamDataProvider(stream)));
            for (byte index = 0; (int)index < (int)numCompletionPkts; ++index)
                this.AddSegment(new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, (IDataProvider)new StringDataProvider("This is a test"), DgPacket.Flags.Completed));
        }
    }
}
