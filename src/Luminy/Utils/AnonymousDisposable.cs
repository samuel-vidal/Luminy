using System;
using System.Threading;

namespace Luminy.Utils
{
    /// <summary>
    /// A thread-safe, idempotent disposable that executes a delegate when disposed.
    /// </summary>
    public sealed class AnonymousDisposable(Action dispose) : IDisposable
    {
        private Action? dispose = dispose;

        public void Dispose() => Interlocked.Exchange(ref dispose, null)?.Invoke();
    }
}