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
    public sealed class HGeneratorIterator : HIteratorBase
    {
        public Generator Generator { get; private set; }
        public ReadOnlyList<string> VariableDeclarations { get; private set; }
        public ILexicalEnvironment Scope { get; private set; }

        public HGeneratorIterator(IEnvironment environment, Generator generator, ReadOnlyList<string> variableDeclarations, ILexicalEnvironment scope)
            : base(environment)
        {
            Generator = generator;
            VariableDeclarations = variableDeclarations;
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
            environment.Context.LexicalEnviroment = Scope;
            environment.Context.VariableEnviroment = Scope;
            if (!Generator.Initialized)
            {
                environment.BindVariableDeclarations(VariableDeclarations, true, true);
                Generator.Initialized = true;
            }
            var step = Generator.Steps.Dequeue();
            var iterated = step(environment, Generator);
            Generator.Complete = !iterated || Generator.Steps.Count == 0;
            return environment.CreateBoolean(iterated);
        }
    }
}
