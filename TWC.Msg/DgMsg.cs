using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TWC.Util.DataProvider;

namespace TWC.Msg
{
    public abstract class DgMsg : TWC.Msg.Msg
    {
        private LinkedList<DgMsg.Segment> segments = new LinkedList<DgMsg.Segment>();
        private uint id;
        private uint timeout;
        private DgPacket.MsgType msgType;
        private byte transmitCnt;
        private byte curTransmission;
        private LinkedListNode<DgMsg.Segment> current;
        private byte segNum;
        private uint pktNum;

        protected DgMsg(DgPacket.MsgType msgType) : this(msgType, (byte)1)
        {
        }

        protected DgMsg(DgPacket.MsgType msgType, byte transmitCnt)
        {
            this.msgType = msgType;
            this.transmitCnt = transmitCnt;
        }

        public uint Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public uint Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                this.timeout = value;
            }
        }

        public override uint MaxPacketSize
        {
            get
            {
                return 1423;
            }
        }

        public override uint CalcMsgXmitSize()
        {
            if (this.segments.Count == 0)
                return 0;
            uint num1 = 0;
            uint num2 = 0;
            uint num3 = num1 + 8U;
            foreach (DgMsg.Segment segment in this.segments)
            {
                num3 += 8U;
                num3 += segment.provider.Count;
                if (segment.type == DgMsg.Segment.Type.FileSegment)
                {
                    num3 += 8U;
                    num3 += (uint)segment.fileName.Length;
                }
                num2 += segment.provider.Count / 1405U;
                if ((int)(num3 % 1405U) != 0)
                    ++num2;
            }
            return (num3 + num2 * 18U) * (uint)this.transmitCnt;
        }

        public override uint CalcMsgPacketCount()
        {
            if (this.segments.Count == 0)
                return 0;
            LinkedListNode<DgMsg.Segment> linkedListNode = this.segments.First;
            uint num = 0;
            for (; linkedListNode != null; linkedListNode = linkedListNode.Next)
                num += this.CalcSegPacketCount(linkedListNode.Value);
            return num * (uint)this.transmitCnt;
        }

        public override void Start()
        {
            this.curTransmission = (byte)0;
            this.StartTransmission();
        }

        public override uint GetNextPacket(ref byte[] buf, uint offset)
        {
            return this.GetNextPacketFromSegment(ref buf, offset);
        }

        protected unsafe uint GetNextPacketFromSegment(ref byte[] buf, uint offset)
        {
            if (this.current == null)
                return 0;
            bool hasMsgHdr = false;
            bool hasSegHdr = false;
            bool hasFileHdr = false;
            DgMsg.Segment seg = this.current.Value;
            uint offset1 = offset;
            ushort packetSize;
            ushort payloadSize;
            this.CalcCurPacketSize(out hasMsgHdr, out hasSegHdr, out hasFileHdr, out packetSize, out payloadSize);
            DgPacket.PacketHeader pktHdr = new DgPacket.PacketHeader();
            pktHdr.hdrLen = (byte)18;
            pktHdr.pktLen = (ushort)((uint)packetSize - 18U);
            pktHdr.vers = (ushort)1;
            pktHdr.flags = seg.flags;
            pktHdr.msgId = this.Id;
            pktHdr.pktNum = this.pktNum;
            pktHdr.segNum = this.segNum;
            pktHdr.checksum = (byte)0;
            pktHdr.unused = (byte)0;
            DgPacket.MakeNetworkByteOrder(ref pktHdr);
            this.CopyTo((IntPtr)((void*)&pktHdr), ref buf, ref offset1, (byte)18);
            if (hasMsgHdr)
            {
                DgPacket.MessageHeader msgHdr = new DgPacket.MessageHeader();
                msgHdr.hdrLen = (byte)8;
                msgHdr.nSeg = (byte)this.segments.Count;
                msgHdr.msgType = this.msgType;
                msgHdr.unused = (byte)0;
                msgHdr.msgTimeOut = this.Timeout;
                DgPacket.MakeNetworkByteOrder(ref msgHdr);
                this.CopyTo((IntPtr)((void*)&msgHdr), ref buf, ref offset1, (byte)8);
            }
            if (hasSegHdr)
            {
                DgPacket.SegmentHeader segHdr = new DgPacket.SegmentHeader();
                segHdr.hdrLen = (byte)8;
                segHdr.pktCount = this.CalcSegPacketCount(seg);
                segHdr.encoding = (byte)0;
                segHdr.compression = seg.compressionType;
                segHdr.unused = (byte)0;
                DgPacket.MakeNetworkByteOrder(ref segHdr);
                this.CopyTo((IntPtr)((void*)&segHdr), ref buf, ref offset1, (byte)8);
            }
            if (hasFileHdr)
            {
                DgPacket.FileDataHeader fileHdr = new DgPacket.FileDataHeader();
                fileHdr.hdrLen = (byte)8;
                fileHdr.fSize = seg.provider.Count;
                fileHdr.nameLen = (ushort)seg.fileName.Length;
                fileHdr.unused = (byte)0;
                DgPacket.MakeNetworkByteOrder(ref fileHdr);
                this.CopyTo((IntPtr)((void*)&fileHdr), ref buf, ref offset1, (byte)8);
                string fileName = seg.fileName;
                Encoding.UTF8.GetBytes(fileName, 0, fileName.Length, buf, (int)offset1);
                offset1 += (uint)fileName.Length;
            }
            if ((int)payloadSize > 0)
            {
                seg.provider.GetNextBytes(ref buf, offset1, (uint)payloadSize);
                offset1 += (uint)payloadSize;
            }
            if (seg.provider.CountRemaining > 0U)
            {
                ++this.pktNum;
            }
            else
            {
                this.current = this.current.Next;
                ++this.segNum;
                this.pktNum = 0U;
                if (this.current == null && (int)++this.curTransmission < (int)this.transmitCnt)
                    this.StartTransmission();
            }
            return offset1 - offset;
        }

        protected uint CalcSegPacketCount(DgMsg.Segment seg)
        {
            uint count = seg.provider.Count;
            uint num = count / 1405U;
            if ((int)(count % 1405U) != 0)
                ++num;
            return num + 1U;
        }

        protected void CalcCurPacketSize(out bool hasMsgHdr, out bool hasSegHdr, out bool hasFileHdr, out ushort packetSize, out ushort payloadSize)
        {
            DgMsg.Segment segment = this.current.Value;
            hasMsgHdr = false;
            hasSegHdr = false;
            hasFileHdr = false;
            packetSize = (ushort)18;
            if ((int)this.pktNum == 0)
            {
                payloadSize = (ushort)0;
                if ((int)this.segNum == 0)
                {
                    hasMsgHdr = true;
                    packetSize += (ushort)8;
                }
                hasSegHdr = true;
                packetSize += (ushort)8;
                if (segment.type != DgMsg.Segment.Type.FileSegment)
                    return;
                hasFileHdr = true;
                packetSize += (ushort)8;
                packetSize += (ushort)segment.fileName.Length;
            }
            else
            {
                payloadSize = (ushort)Math.Min(1423U - (uint)packetSize, segment.provider.CountRemaining);
                packetSize += payloadSize;
            }
        }

        protected void AddSegment(DgMsg.Segment seg)
        {
            this.segments.AddLast(seg);
        }

        private void StartTransmission()
        {
            foreach (DgMsg.Segment segment in this.segments)
                segment.provider.Start();
            this.current = this.segments.First;
            this.segNum = (byte)0;
            this.pktNum = 0U;
        }

        private void CopyTo(IntPtr ptr, ref byte[] buf, ref uint offset, byte cnt)
        {
            Marshal.Copy(ptr, buf, (int)offset, (int)cnt);
            offset += (uint)cnt;
        }

        protected override void DisposeManaged()
        {
            foreach (DgMsg.Segment segment in this.segments)
                segment.provider.Dispose();
            this.segments.Clear();
        }

        protected class Segment
        {
            public DgMsg.Segment.Type type;
            public IDataProvider provider;
            public string fileName;
            public DgPacket.Compression compressionType;
            public DgPacket.Flags flags;

            public Segment(DgMsg.Segment.Type type, IDataProvider provider)
              : this(type, provider, DgPacket.Flags.None)
            {
            }

            public Segment(DgMsg.Segment.Type type, IDataProvider provider, DgPacket.Flags flags)
            {
                this.type = type;
                this.provider = provider;
                this.flags = flags;
            }

            public enum Type
            {
                CmdSegment,
                FileSegment,
            }
        }
    }
}
