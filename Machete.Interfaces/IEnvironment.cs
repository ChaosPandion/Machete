﻿using System.Collections.Generic;

namespace Machete.Interfaces
{
    public interface IEnvironment
    {
        IExecutionContext Context { get; }
        IUndefined Undefined { get; }
        INull Null { get; }

        IBoolean CreateBoolean(bool value);
        IString CreateString(string value);
        INumber CreateNumber(double value);
        IArgs CreateArgs(IEnumerable<IDynamic> values);
        IObject CreateArray();
        IObject CreateObject();
        IObject CreateReferenceError();
        IObject CreateTypeError();
        IObject CreateSyntaxError();
    }
}