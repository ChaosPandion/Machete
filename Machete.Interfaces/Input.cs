using System;
using System.Collections.Concurrent;

namespace Machete.Interfaces
{
    public sealed class Input : IDisposable
    {
        private readonly BlockingCollection<string> _messages = new BlockingCollection<string>();

        public string Read()
        {
            return _messages.Take();
        }

        public string Read(int timeout)
        {
            string result;
            if (!_messages.TryTake(out result, timeout))
            {
                return null;
            }
            return result;
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