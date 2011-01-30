using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Machete.Core.Generators
{
    public sealed class GeneratorSteps : IEnumerable<GeneratorStep>
    {
        private readonly Queue<GeneratorStep> _queue;

        public int Count 
        {
            get { return _queue.Count; }
        }

        public GeneratorSteps(IEnumerable<GeneratorStep> items)
        {
            _queue = new Queue<GeneratorStep>(items);
        }

        public GeneratorSteps(IEnumerable<GeneratorStep> first, IEnumerable<GeneratorStep> last)
        {
            _queue = new Queue<GeneratorStep>(first.Concat(last));
        }

        public GeneratorStep Next()
        {
            return _queue.Dequeue();
        }

        public IEnumerator<GeneratorStep> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }
    }
}