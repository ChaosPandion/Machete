using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Core.Generators;
using Machete.Core;
using Machete.Runtime.NativeObjects.BuiltinObjects;

namespace Machete.Runtime.HostObjects.Iterables
{
    public sealed class HGeneratorIterator : HIterator
    {
        public Generator Generator { get; private set; }
        public ILexicalEnvironment Scope { get; private set; }

        public HGeneratorIterator(IEnvironment environment, Generator generator, ILexicalEnvironment scope)
            : base(environment)
        {
            Generator = generator;
            Scope = scope;
        }

        public override IDynamic Current(IEnvironment environment, IArgs args)
        {
            if (!Generator.Initialized)
                throw environment.CreateTypeError("");
            return Generator.Current;
        }

        public override IDynamic Next(IEnvironment environment, IArgs args)
        {
            if (Generator.Complete)
                return environment.CreateBoolean(false);
            var step = Generator.Steps.Dequeue();
            var old = environment.Context.LexicalEnviroment;
            environment.Context.LexicalEnviroment = Scope;
            var iterated = step(environment, Generator);
            Generator.Complete = !iterated || Generator.Steps.Count == 0;
            environment.Context.LexicalEnviroment = old;
            Generator.Initialized = true;
            return environment.CreateBoolean(iterated);
        }
    }
}
