namespace Machete.Compiler
 
module Reflection =

    open System
    open System.Collections
    open System.Collections.Generic
    open Machete.Interfaces

    module IEnvironment = 
        let t = typeof<Machete.Interfaces.IEnvironment>
        let get_Context = t.GetMethod "get_Context"
        let get_Undefined = t.GetMethod "get_Undefined"
        let get_Null = t.GetMethod "get_Null"
        let get_EmptyArgs = t.GetMethod "get_EmptyArgs"
        let createBoolean = t.GetMethod "CreateBoolean"
        let createString = t.GetMethod "CreateString"
        let createNumber = t.GetMethod "CreateNumber"
        let concatArgs = t.GetMethod "ConcatArgs"
        let createArgsSingle = t.GetMethod ("CreateArgs", [| typeof<Machete.Interfaces.IDynamic> |])
        let createArgsMany = t.GetMethod ("CreateArgs", [| typeof<System.Collections.Generic.IEnumerable<Machete.Interfaces.IDynamic>> |])
        let createDeclarativeEnvironmentRecord = t.GetMethod "CreateDeclarativeEnvironmentRecord"
        let createObjectEnvironmentRecord = t.GetMethod "CreateObjectEnvironmentRecord"
        let createArray = t.GetMethod "CreateArray"
        let createObject = t.GetMethod "CreateObject"
        let createArguments = t.GetMethod "CreateArguments"
        let createRegExp = t.GetMethod "CreateRegExp"

        let createReference = t.GetMethod "CreateReference"

        let createFunction1 = t.GetMethod ("CreateFunction", [| typeof<ReadOnlyList<string>>; typeof<bool>; typeof<Lazy<Code>> |])
        let createFunction2 = t.GetMethod ("CreateFunction", [| typeof<ReadOnlyList<string>>; typeof<bool>; typeof<Lazy<Code>>; typeof<ILexicalEnvironment> |])
        
        let createDataDescriptor1 = t.GetMethod ("CreateDataDescriptor", [| typeof<IDynamic> |])
        let createDataDescriptor2 = t.GetMethod ("CreateDataDescriptor", [| typeof<IDynamic>; typeof<Nullable<bool>> |])
        let createDataDescriptor3 = t.GetMethod ("CreateDataDescriptor", [| typeof<IDynamic>; typeof<Nullable<bool>>; typeof<Nullable<bool>> |])
        let createDataDescriptor4 = t.GetMethod ("CreateDataDescriptor", [| typeof<IDynamic>; typeof<Nullable<bool>>; typeof<Nullable<bool>>; typeof<Nullable<bool>> |])
        
        let createAccessorDescriptor1 = t.GetMethod ("CreateAccessorDescriptor", [| typeof<IDynamic>; typeof<IDynamic> |])
        let createAccessorDescriptor2 = t.GetMethod ("CreateAccessorDescriptor", [| typeof<IDynamic>; typeof<IDynamic>; typeof<Nullable<bool>> |])
        let createAccessorDescriptor3 = t.GetMethod ("CreateAccessorDescriptor", [| typeof<IDynamic>; typeof<IDynamic>; typeof<Nullable<bool>>; typeof<Nullable<bool>> |])
        
        let throwRuntimeException = t.GetMethod "ThrowRuntimeException"

        let createSyntaxError = t.GetMethod "CreateSyntaxError"
        
        let enterGlobalCode = t.GetMethod "EnterGlobalCode"
        let enterEvalCode = t.GetMethod "EnterEvalCode"
        let enterFunctionCode = t.GetMethod "EnterFunctionCode"

    module MacheteRuntimeException =
        let t = typeof<MacheteRuntimeException>
        let get_Thrown = t.GetMethod "get_Thrown"

    module IExecutionContext =
        let t = typeof<Machete.Interfaces.IExecutionContext>
        let get_LexicalEnviroment = t.GetMethod "get_LexicalEnviroment"
        let set_LexicalEnviroment = t.GetMethod "set_LexicalEnviroment"
        let get_VariableEnviroment = t.GetMethod "get_VariableEnviroment"
        let set_VariableEnviroment = t.GetMethod "set_VariableEnviroment"
        let get_ThisBinding = t.GetMethod "get_ThisBinding"
        let set_ThisBinding = t.GetMethod "set_ThisBinding"

    module ILexicalEnvironment =
        let t = typeof<Machete.Interfaces.ILexicalEnvironment>
        let get_Record = t.GetMethod "get_Record"
        let get_Parent = t.GetMethod "get_Parent"
        let getIdentifierReference = t.GetMethod "GetIdentifierReference"    
        let newDeclarativeEnvironment = t.GetMethod "NewDeclarativeEnvironment"  
        let newObjectEnvironment = t.GetMethod "NewObjectEnvironment" 

    module IDynamic = 
        let t = typeof<Machete.Interfaces.IDynamic>
        let get_Value = t.GetMethod "get_Value"
        let set_Value = t.GetMethod "set_Value"
        let op_Void = t.GetMethod "Op_Void"
        let op_Typeof = t.GetMethod "Op_Typeof"
        let op_PrefixIncrement = t.GetMethod "Op_PrefixIncrement"
        let op_PrefixDecrement = t.GetMethod "Op_PrefixDecrement"
        let op_Plus = t.GetMethod "Op_Plus"
        let op_Minus = t.GetMethod "Op_Minus"
        let op_PostfixIncrement = t.GetMethod "Op_PostfixIncrement"
        let op_PostfixDecrement = t.GetMethod "Op_PostfixDecrement"
        let op_GetProperty = t.GetMethod "Op_GetProperty"
        let op_SetProperty = t.GetMethod "Op_SetProperty"
        let op_Call = t.GetMethod "Op_Call"
        let op_Construct = t.GetMethod "Op_Construct"
        let op_Throw = t.GetMethod "Op_Throw"
        let convertToPrimitive = t.GetMethod "ConvertToPrimitive"
        let convertToBoolean = t.GetMethod "ConvertToBoolean"
        let convertToNumber = t.GetMethod "ConvertToNumber"
        let convertToString = t.GetMethod "ConvertToString"
        let convertToObject = t.GetMethod "ConvertToObject"
        let convertToInteger = t.GetMethod "ConvertToInteger"
        let convertToInt32 = t.GetMethod "ConvertToInt32"
        let convertToUInt32 = t.GetMethod "ConvertToUInt32"
        let convertToUInt16 = t.GetMethod "ConvertToUInt16"
        let get_TypeCode = t.GetMethod "get_TypeCode"
        let get_IsPrimitive = t.GetMethod "get_IsPrimitive"
        let op_LogicalNot = t.GetMethod "Op_LogicalNot"
        let op_LogicalOr = t.GetMethod "Op_LogicalOr"
        let op_LogicalAnd = t.GetMethod "Op_LogicalAnd"
        let op_BitwiseNot = t.GetMethod "Op_BitwiseNot"
        let op_BitwiseOr = t.GetMethod "Op_BitwiseOr"
        let op_BitwiseXor = t.GetMethod "Op_BitwiseXor"
        let op_BitwiseAnd = t.GetMethod "Op_BitwiseAnd"
        let op_Equals = t.GetMethod "Op_Equals"
        let op_DoesNotEquals = t.GetMethod "Op_DoesNotEquals"
        let op_StrictEquals = t.GetMethod "Op_StrictEquals"
        let op_StrictDoesNotEquals = t.GetMethod "Op_StrictDoesNotEquals"
        let op_Lessthan = t.GetMethod "Op_Lessthan"
        let op_Greaterthan = t.GetMethod "Op_Greaterthan"
        let op_LessthanOrEqual = t.GetMethod "Op_LessthanOrEqual"
        let op_GreaterthanOrEqual = t.GetMethod "Op_GreaterthanOrEqual"
        let op_Instanceof = t.GetMethod "Op_Instanceof"
        let op_In = t.GetMethod "Op_In"
        let op_LeftShift = t.GetMethod "Op_LeftShift"
        let op_SignedRightShift = t.GetMethod "Op_SignedRightShift"
        let op_UnsignedRightShift = t.GetMethod "Op_UnsignedRightShift"
        let op_Addition = t.GetMethod "Op_Addition"
        let op_Subtraction = t.GetMethod "Op_Subtraction"
        let op_Multiplication = t.GetMethod "Op_Multiplication"
        let op_Division = t.GetMethod "Op_Division"
        let op_Modulus = t.GetMethod "Op_Modulus"
        let op_Delete = t.GetMethod "Op_Delete"

    module IUndefined = 
        let t = typeof<Machete.Interfaces.IUndefined>


    module INull = 
        let t = typeof<Machete.Interfaces.INull>


    module IBoolean = 
        let t = typeof<Machete.Interfaces.IBoolean>
        let get_BaseValue = t.GetMethod "get_BaseValue"

    module IString = 
        let t = typeof<Machete.Interfaces.IString>
        let get_BaseValue = t.GetMethod "get_BaseValue"

    module INumber = 
        let t = typeof<Machete.Interfaces.INumber>
        let get_BaseValue = t.GetMethod "get_BaseValue"

    module IObject = 
        let t = typeof<Machete.Interfaces.IObject>
        let get_Prototype = t.GetMethod "get_Prototype"
        let set_Prototype = t.GetMethod "set_Prototype"
        let get_Class = t.GetMethod "get_Class"
        let set_Class = t.GetMethod "set_Class"
        let get_Extensible = t.GetMethod "get_Extensible"
        let set_Extensible = t.GetMethod "set_Extensible"
        let getOwnProperty = t.GetMethod "GetOwnProperty"
        let getProperty = t.GetMethod "GetProperty"
        let get = t.GetMethod "Get"
        let put = t.GetMethod "Put"
        let hasProperty = t.GetMethod "HasProperty"
        let delete = t.GetMethod "Delete"
        let defaultValue = t.GetMethod "DefaultValue"
        let defineOwnProperty = t.GetMethod "DefineOwnProperty"

    module IArgs = 
        let t = typeof<Machete.Interfaces.IArgs>
        let get_Item = t.GetMethod "get_Item"
        let get_Count = t.GetMethod "get_Count"
        let get_IsEmpty = t.GetMethod "get_IsEmpty"

    module IPropertyDescriptor = 
        let t = typeof<Machete.Interfaces.IPropertyDescriptor>
        let get_Value = t.GetMethod "get_Value"
        let set_Value = t.GetMethod "set_Value"
        let get_Writable = t.GetMethod "get_Writable"
        let set_Writable = t.GetMethod "set_Writable"
        let get_Get = t.GetMethod "get_Get"
        let set_Get = t.GetMethod "set_Get"
        let get_Set = t.GetMethod "get_Set"
        let set_Set = t.GetMethod "set_Set"
        let get_Enumerable = t.GetMethod "get_Enumerable"
        let set_Enumerable = t.GetMethod "set_Enumerable"
        let get_Configurable = t.GetMethod "get_Configurable"
        let set_Configurable = t.GetMethod "set_Configurable"
        let get_IsAccessorDescriptor = t.GetMethod "get_IsAccessorDescriptor"
        let get_IsDataDescriptor = t.GetMethod "get_IsDataDescriptor"
        let get_IsGenericDescriptor = t.GetMethod "get_IsGenericDescriptor"
        let get_IsEmpty = t.GetMethod "get_IsEmpty"

    module IEnumerableString =
        let t = typeof<IEnumerable<string>>
        let getEnumerator = t.GetMethod "GetEnumerator" 
        
    module IEnumeratorString =
        let t = typeof<IEnumerator<string>>
        let get_Current = t.GetMethod "get_Current" 

    module IEnumerator =
        let t = typeof<IEnumerator>
        let moveNext = t.GetMethod "MoveNext"
        let reset = t.GetMethod "Reset"
        
    module IDisposable =
        let t = typeof<IDisposable>
        let dispose = t.GetMethod "Dispose"

    module IReferenceBase = 
        let t = typeof<Machete.Interfaces.IReferenceBase>
        let get = t.GetMethod "Get"
        let set = t.GetMethod "Set"

    module IEnvironmentRecord = 
        let t = typeof<Machete.Interfaces.IEnvironmentRecord>
        let hasBinding = t.GetMethod "HasBinding"
        let createMutableBinding = t.GetMethod "CreateMutableBinding"
        let setMutableBinding = t.GetMethod "SetMutableBinding"
        let getBindingValue = t.GetMethod "GetBindingValue"
        let deleteBinding = t.GetMethod "DeleteBinding"
        let implicitThisValue = t.GetMethod "ImplicitThisValue"

    module IDeclarativeEnvironmentRecord = 
        let t = typeof<Machete.Interfaces.IDeclarativeEnvironmentRecord>
        let createImmutableBinding = t.GetMethod "CreateImmutableBinding"
        let initializeImmutableBinding = t.GetMethod "InitializeImmutableBinding"

    module IObjectEnvironmentRecord = 
        let t = typeof<Machete.Interfaces.IObjectEnvironmentRecord>


    module ICallable = 
        let t = typeof<Machete.Interfaces.ICallable>
        let call = t.GetMethod "Call"

    module IConstructable = 
        let t = typeof<Machete.Interfaces.IConstructable>
        let construct = t.GetMethod "Construct"

    module IPrimitiveWrapper = 
        let t = typeof<Machete.Interfaces.IPrimitiveWrapper>
        let get_PrimitiveValue = t.GetMethod "get_PrimitiveValue"
        let set_PrimitiveValue = t.GetMethod "set_PrimitiveValue"
