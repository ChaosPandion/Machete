using System;
using System.Collections.Generic;

namespace Machete.Interfaces
{
    public interface IEnvironment
    {
        IExecutionContext Context { get; }
        IUndefined Undefined { get; }
        INull Null { get; }
        IBoolean BooleanTrue { get; }
        IBoolean BooleanFalse { get; }
        IObject GlobalObject { get; }
        IArgs EmptyArgs { get; }

        IReference CreateReference(string name, IReferenceBase @base, bool strict);
        IBoolean CreateBoolean(bool value);
        IString CreateString(string value);
        INumber CreateNumber(double value);

        IArgs CreateArgs(IDynamic value);
        IArgs CreateArgs(IEnumerable<IDynamic> values);
        IArgs ConcatArgs(IArgs first, IArgs second);

        IObject CreateArray();
        IObject CreateObject();
        IObject CreateReferenceError();
        IObject CreateTypeError();
        IObject CreateSyntaxError();
        IObject CreateFunction(string[] formalParameterList, bool strict, Lazy<Code> code);

        IExecutionContext EnterContext();
    }
}