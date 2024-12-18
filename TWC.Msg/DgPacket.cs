using System.Net;
using System.Runtime.InteropServices;

namespace TWC.Msg
{
    public static class DgPacket
    {
        public const uint MAX_PACKET_SIZE = 1423;
        public const byte PACKET_HDR_SIZE = 18;
        public const byte MESSAGE_HDR_SIZE = 8;
        public const byte SEGMENT_HDR_SIZE = 8;
        public const byte FILE_HDR_SIZE = 8;
        public const uint MAX_PAYLOAD_SIZE = 1405;
        public const byte FILE_MSG_FILE_SEG_NUM = 2;

        public static void MakeHostByteOrder(ref DgPacket.PacketHeader pktHdr)
        {
            pktHdr.vers = (ushort)IPAddress.NetworkToHostOrder((short)pktHdr.vers);
            pktHdr.flags = (DgPacket.Flags)IPAddress.NetworkToHostOrder((short)pktHdr.flags);
            pktHdr.pktLen = (ushort)IPAddress.NetworkToHostOrder((short)pktHdr.pktLen);
            pktHdr.msgId = (uint)IPAddress.NetworkToHostOrder((int)pktHdr.msgId);
            pktHdr.pktNum = (uint)IPAddress.NetworkToHostOrder((int)pktHdr.pktNum);
        }

        public static void MakeHostByteOrder(ref DgPacket.MessageHeader msgHdr)
        {
            msgHdr.msgTimeOut = (uint)IPAddress.NetworkToHostOrder((int)msgHdr.msgTimeOut);
        }

        public static void MakeHostByteOrder(ref DgPacket.SegmentHeader segHdr)
        {
            segHdr.pktCount = (uint)IPAddress.NetworkToHostOrder((int)segHdr.pktCount);
        }

        public static void MakeHostByteOrder(ref DgPacket.FileDataHeader fileHdr)
        {
            fileHdr.fSize = (uint)IPAddress.NetworkToHostOrder((int)fileHdr.fSize);
            fileHdr.nameLen = (ushort)IPAddress.NetworkToHostOrder((short)fileHdr.nameLen);
        }

        public static void MakeNetworkByteOrder(ref DgPacket.PacketHeader pktHdr)
        {
            pktHdr.vers = (ushort)IPAddress.HostToNetworkOrder((short)pktHdr.vers);
            pktHdr.flags = (DgPacket.Flags)IPAddress.HostToNetworkOrder((short)pktHdr.flags);
            pktHdr.pktLen = (ushort)IPAddress.HostToNetworkOrder((short)pktHdr.pktLen);
            pktHdr.msgId = (uint)IPAddress.HostToNetworkOrder((int)pktHdr.msgId);
            pktHdr.pktNum = (uint)IPAddress.HostToNetworkOrder((int)pktHdr.pktNum);
        }

        public static void MakeNetworkByteOrder(ref DgPacket.MessageHeader msgHdr)
        {
            msgHdr.msgTimeOut = (uint)IPAddress.HostToNetworkOrder((int)msgHdr.msgTimeOut);
        }

        public static void MakeNetworkByteOrder(ref DgPacket.SegmentHeader segHdr)
        {
            segHdr.pktCount = (uint)IPAddress.HostToNetworkOrder((int)segHdr.pktCount);
        }

        public static void MakeNetworkByteOrder(ref DgPacket.FileDataHeader fileHdr)
        {
            fileHdr.fSize = (uint)IPAddress.HostToNetworkOrder((int)fileHdr.fSize);
            fileHdr.nameLen = (ushort)IPAddress.HostToNetworkOrder((short)fileHdr.nameLen);
        }

        public enum MsgType : byte
        {
            Unknown,
            Command,
            File,
            I2,
        }

        public enum Compression : byte
        {
            None,
            GZip,
        }

        public enum Flags : ushort
        {
            None,
            Completed,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PacketHeader
        {
            public byte hdrLen;
            public ushort vers;
            public DgPacket.Flags flags;
            public ushort pktLen;
            public uint msgId;
            public uint pktNum;
            public byte segNum;
            public byte checksum;
            public byte unused;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MessageHeader
        {
            public byte hdrLen;
            public byte nSeg;
            public DgPacket.MsgType msgType;
            public byte unused;
            public uint msgTimeOut;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SegmentHeader
        {
            public byte hdrLen;
            public uint pktCount;
            public byte encoding;
            public DgPacket.Compression compression;
            public byte unused;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FileDataHeader
        {
            public byte hdrLen;
            public uint fSize;
            public ushort nameLen;
            public byte unused;
        }
    }
}
