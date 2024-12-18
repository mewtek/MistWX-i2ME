using System;
using System.Text;

namespace TWC.Util.DataProvider
{
    public class StringDataProvider : Disposable, IDataProvider, IDisposable
    {
        private string s;
        private uint offset;

        public StringDataProvider(string s)
        {
            this.s = s;
        }

        public uint Count
        {
            get
            {
                return (uint)this.s.Length;
            }
        }

        public uint CountRemaining
        {
            get
            {
                return (uint)this.s.Length - this.offset;
            }
        }

        public void Start()
        {
            this.offset = 0U;
        }

        public void GetNextBytes(ref byte[] buf, uint pos, uint count)
        {
            Encoding.GetEncoding("iso-8859-1").GetBytes(this.s, (int)this.offset, (int)count, buf, (int)pos);
            this.offset += count;
        }
    }
}
