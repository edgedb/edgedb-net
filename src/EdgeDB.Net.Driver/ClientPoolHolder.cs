using System;
using System.Collections.Concurrent;

namespace EdgeDB
{
    internal sealed class ClientPoolHolder
    {
        private readonly SemaphoreSlim _resizeWaiter;
        private readonly List<PoolHandle> _handles;
        private readonly object _handleCollectionLock;
        private SemaphoreSlim _poolClientWaiter;
        private int _size;

        public ClientPoolHolder(int initialSize)
        {
            _size = initialSize;
            _poolClientWaiter = new(_size, _size);
            _handles = new();
            _resizeWaiter = new(1,1);
            _handleCollectionLock = new();
        }

        public async Task ResizeAsync(int newSize)
        {
            await _resizeWaiter.WaitAsync();

            try
            {
                if (_size == newSize)
                    return;

                _size = newSize;

                _poolClientWaiter = new(_size, _size);
            }
            finally
            {
                _resizeWaiter.Release();
            }
        }

        public async Task<IDisposable> GetPoolHandleAsync(CancellationToken token)
        {
            // wait for an open handle
            var localWaiter = _poolClientWaiter;
            await localWaiter.WaitAsync(token).ConfigureAwait(false);
            await _resizeWaiter.WaitAsync(token).ConfigureAwait(false);

            try
            {
                var handle = new PoolHandle();
                handle.SetReleaseAction(() =>
                {
                    localWaiter.Release();
                    lock (_handleCollectionLock)
                    {
                        _handles.Remove(handle);
                    }
                });
                _handles.Add(handle);
                return handle;
            }
            finally
            {
                _resizeWaiter.Release();
            }
        }

        private class PoolHandle : IDisposable
        {
            public bool HasReleased { get; private set; }

            private readonly object _lock;
            private Action _release;

            public PoolHandle()
            {
                // TODO: resize might have to lock the handle?
                _lock = new();
                _release = () => { };
                HasReleased = false;
            }

            public void SetReleaseAction(Action newDelegate)
            {
                lock (_lock)
                {
                    if (HasReleased)
                        return;

                    _release = newDelegate;
                }
            }

            public void Dispose()
            {
                lock (_lock)
                {
                    if (!HasReleased)
                        _release();
                    HasReleased = true;
                }
            }
        }
    }
}

