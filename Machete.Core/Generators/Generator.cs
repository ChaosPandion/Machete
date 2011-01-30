using System.Collections.Generic;

namespace Machete.Core.Generators
{
    public sealed class Generator
    {
        public Queue<GeneratorStep> Steps { get; private set; }
        public IDynamic Current { get; set; }
        public bool Complete { get; set; }
        public bool Initialized { get; set; }

        public Generator(IEnumerable<GeneratorStep> steps)
        {
            Steps = new Queue<GeneratorStep>(steps);
        }
    }
}