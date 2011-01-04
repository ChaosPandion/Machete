using System;
using System.Collections.Concurrent;

namespace Machete.Interfaces
{
    public sealed class Output : IDisposable
    {
        private readonly BlockingCollection<string> _messages = new BlockingCollection<string>();

        public void Write(string value)
        {
            _messages.TryAdd(value ?? string.Empty);
        }

        public string Take()
        {
            return _messages.Take();
        }

        public bool TryTake(out string result)
        {
            return _messages.TryTake(out result);
        }

        public bool TryTake(out string result, int timeout)
        {
            return _messages.TryTake(out result, timeout);
        }

        public void Dispose()
        {
            _messages.Dispose();
        }
    }
}