using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Core;
using Machete.Runtime.NativeObjects.BuiltinObjects;
using Machete.Core.Generators;

namespace Machete.Runtime.HostObjects.Iterables
{
    public sealed class HGeneratorIterable : HIterable
    {        
        public ReadOnlyList<GeneratorStep> Steps { get; private set; }
        public ILexicalEnvironment Scope { get; private set; }

        public HGeneratorIterable(IEnvironment environment, ReadOnlyList<GeneratorStep> steps, ILexicalEnvironment scope)
            : base(environment)
        {
            Steps = steps;
            Scope = scope;
        }

        public override IDynamic CreateIterator(IEnvironment environment, IArgs args)
        {
            var scope = Scope.NewDeclarativeEnvironment();
            var generator = new Generator(new GeneratorSteps(Steps));
            var iterator = new HGeneratorIterator(environment, generator, scope);
            return iterator;
        }
    }
}
