using System;

namespace TWC.Util.DataProvider
{
    public class NullDataProvider : Disposable, IDataProvider, IDisposable
    {
        public uint Count
        {
            get
            {
                return 0;
            }
        }

        public uint CountRemaining
        {
            get
            {
                return 0;
            }
        }

        public void Start()
        {
        }

        public void GetNextBytes(ref byte[] buf, uint pos, uint count)
        {
        }
    }
}
