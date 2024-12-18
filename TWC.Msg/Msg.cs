using System;
using System.Collections;
using System.Collections.Generic;
using TWC.Util;

namespace TWC.Msg
{
    public abstract class Msg : Disposable, IEnumerable<byte[]>, IEnumerable
    {
        private byte[] tempBuff;

        public Msg()
        {
            this.tempBuff = new byte[(int)this.MaxPacketSize];
        }

        public abstract uint MaxPacketSize { get; }

        public abstract uint CalcMsgXmitSize();

        public abstract uint CalcMsgPacketCount();

        public abstract void Start();

        public abstract uint GetNextPacket(ref byte[] buf, uint offset);

        public byte[] GetNextPacket()
        {
            uint nextPacket = this.GetNextPacket(ref this.tempBuff, 0U);
            if ((int)nextPacket == 0)
                return (byte[])null;
            byte[] numArray = new byte[(int)nextPacket];
            Array.Copy((Array)this.tempBuff, (Array)numArray, (long)nextPacket);
            return numArray;
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            this.Start();
            for (byte[] res = this.GetNextPacket(); res != null; res = this.GetNextPacket())
                yield return res;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)null;
        }
    }
}
