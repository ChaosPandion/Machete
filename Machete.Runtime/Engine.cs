using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace Machete.Runtime
{
    public sealed class Engine
    {
        internal static readonly ThreadLocal<Engine> Instance = new ThreadLocal<Engine>();
        private readonly ConcurrentQueue<Tuple<string, BlockingCollection<object>>> _messageQueue;
        private readonly Thread _backingThread;


        private Engine()
        {
            _messageQueue = new ConcurrentQueue<Tuple<string, BlockingCollection<object>>>();
            _backingThread = new Thread(ProcessScripts);
            _backingThread.IsBackground = true;
            _backingThread.Start();
        }


        public object ExecuteScript(string script)
        {
            using (var channel = new BlockingCollection<object>())
            {
                var message = Tuple.Create(script, channel);
                _messageQueue.Enqueue(message);
                return channel.Take();
            }  
        }

        private void ProcessScripts()
        {
            Instance.Value = this;

            Tuple<string, BlockingCollection<object>> message;
            while (_messageQueue.TryDequeue(out message))
            {
                ProcessScript(message);
            }
        }

        private void ProcessScript(Tuple<string, BlockingCollection<object>> message)
        {

        }
    }
}
