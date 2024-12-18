using System;

namespace TWC.Util
{
    public class Disposable : IDisposable
    {
        private bool disposed;

        ~Disposable()
        {
            this.DoDispose(false);
        }

        public void Dispose()
        {
            this.DoDispose(true);
            GC.SuppressFinalize((object)this);
        }

        private bool Disposed
        {
            get
            {
                return this.disposed;
            }
        }

        public void AssertNotDisposed()
        {
            if (this.disposed)
                throw new ObjectDisposedException((string)null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                this.DisposeManaged();
            this.DisposeUnmanaged();
        }

        protected virtual void DisposeManaged()
        {
        }

        protected virtual void DisposeUnmanaged()
        {
        }

        private void DoDispose(bool disposing)
        {
            if (this.disposed)
                return;
            this.Dispose(disposing);
            this.disposed = true;
        }
    }
}
