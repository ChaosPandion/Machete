using System;

namespace Machete.Interfaces
{
    public interface IFunction : IObject, ICallable, IConstructable
    {
        ILexicalEnvironment Scope { get; set; }
        ReadOnlyList<string> FormalParameterList { get; set; }
        Lazy<Code> Code { get; set; }
        IObject TargetFunction { get; set; }
        IDynamic BoundThis { get; set; }
        IArgs BoundArguments { get; set; }
        bool Strict { get; set; }
        bool BindFunction { get; set; }

        bool HasInstance(IDynamic value);
    }
}