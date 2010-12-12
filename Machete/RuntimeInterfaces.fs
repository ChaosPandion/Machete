namespace Machete

open System

type IEnvironment = interface
        abstract member Undefined : IUndefined with get
        abstract member Null : INull with get
        abstract member Context : IExecutionContext with get
        abstract member CreateBoolean : bool -> IBoolean
        abstract member CreateString : string -> IString
        abstract member CreateNumber : double -> INumber
        abstract member CreateDeclarativeEnvironmentRecord : unit -> IDeclarativeEnvironmentRecord
        abstract member CreateObjectEnvironmentRecord : unit -> IObjectEnvironmentRecord
        abstract member ConstructArgs : seq<IDynamic> -> IArgs
        abstract member ConstructArray : unit -> IObject
        abstract member ConstructObject : unit -> IObject
        abstract member ConstructReferenceError : unit -> IObject
        abstract member ConstructTypeError : unit -> IObject
        abstract member ConstructSyntaxError : unit -> IObject
    end

and IExecutionContext = interface
        abstract member LexicalEnviroment : IEnvironmentRecord with get, set
        abstract member VariableEnviroment : ILexicalEnvironment with get, set
        abstract member ThisBinding : IEnvironment with get, set
    end

and ILexicalEnvironment = interface
        abstract member Record : IEnvironmentRecord with get
        abstract member Parent : ILexicalEnvironment with get
        abstract member GetIdentifierReference : string -> bool -> IDynamic
    end

and IDynamic = interface
        abstract member TypeCode : TypeCode with get 
        abstract member IsPrimitive : bool with get 
        abstract member Value : IDynamic with get, set
        abstract member Op_LogicalNot : unit -> IDynamic 
        abstract member Op_LogicalOr : IDynamic -> IDynamic 
        abstract member Op_LogicalAnd : IDynamic -> IDynamic    
        abstract member Op_BitwiseNot : unit -> IDynamic 
        abstract member Op_BitwiseOr : IDynamic -> IDynamic 
        abstract member Op_BitwiseXor : IDynamic -> IDynamic 
        abstract member Op_BitwiseAnd : IDynamic -> IDynamic     
        abstract member Op_Equals : IDynamic -> IDynamic 
        abstract member Op_DoesNotEquals : IDynamic -> IDynamic 
        abstract member Op_StrictEquals : IDynamic -> IDynamic 
        abstract member Op_StrictDoesNotEquals : IDynamic -> IDynamic      
        abstract member Op_Lessthan : IDynamic -> IDynamic 
        abstract member Op_Greaterthan : IDynamic -> IDynamic 
        abstract member Op_LessthanOrEqual : IDynamic -> IDynamic 
        abstract member Op_GreaterthanOrEqual : IDynamic -> IDynamic
        abstract member Op_Instanceof : IDynamic -> IDynamic
        abstract member Op_In : IDynamic -> IDynamic 
        abstract member Op_LeftShift : IDynamic -> IDynamic
        abstract member Op_SignedRightShift : IDynamic -> IDynamic
        abstract member Op_UnsignedRightShift : IDynamic -> IDynamic
        abstract member Op_Addition : IDynamic -> IDynamic
        abstract member Op_Subtraction : IDynamic -> IDynamic
        abstract member Op_Multiplication : IDynamic -> IDynamic
        abstract member Op_Division : IDynamic -> IDynamic
        abstract member Op_Modulus : IDynamic -> IDynamic
        abstract member Op_Delete : IDynamic -> IDynamic
        abstract member Op_Void : IDynamic -> IDynamic
        abstract member Op_Typeof : IDynamic -> IDynamic
        abstract member Op_PrefixIncrement : IDynamic -> IDynamic
        abstract member Op_PrefixDecrement : IDynamic -> IDynamic
        abstract member Op_Plus : IDynamic -> IDynamic
        abstract member Op_Minus : IDynamic -> IDynamic
        abstract member Op_PostfixIncrement : IDynamic -> IDynamic
        abstract member Op_PostfixDecrement : IDynamic -> IDynamic    
        abstract member Op_GetProperty : IDynamic -> IDynamic
        abstract member Op_SetProperty : IDynamic -> IDynamic -> IDynamic
        abstract member Op_Call : IArgs -> IDynamic
        abstract member Op_Construct : IArgs -> IDynamic
        abstract member Op_Throw : unit -> unit
        abstract member ConvertToPrimitive : string -> IDynamic
        abstract member ConvertToBoolean : unit -> IBoolean
        abstract member ConvertToNumber : unit -> INumber
        abstract member ConvertToString : unit -> IString
        abstract member ConvertToObject : unit -> IObject
        abstract member ConvertToInteger : unit -> INumber
        abstract member ConvertToInt32 : unit -> INumber
        abstract member ConvertToUInt32 : unit -> INumber
        abstract member ConvertToUInt16 : unit -> INumber
    end

and IUndefined = interface
        inherit IDynamic
    end

and INull = interface
        inherit IDynamic
    end

and IBoolean = interface
        inherit IDynamic 
        abstract member Base : bool with get
    end

and IString = interface
        inherit IDynamic 
        abstract member Base : string with get
    end

and INumber = interface
        inherit IDynamic 
        abstract member Base : double with get
    end

and IObject = interface
        inherit IDynamic 
        abstract member Prototype : IObject with get, set 
        abstract member Class : string with get, set 
        abstract member Extensible : bool with get, set 
        abstract member GetOwnProperty : string -> IPropertyDescriptor
        abstract member GetProperty : string -> IPropertyDescriptor
        abstract member Get : string -> IDynamic
        abstract member Put : string -> IDynamic -> bool -> unit
        abstract member HasProperty : string -> bool
        abstract member Delete : string -> bool -> bool
        abstract member DefaultValue : string -> IDynamic
        abstract member DefineOwnProperty : string -> IPropertyDescriptor -> bool -> bool
    end

and IArgs = interface
        inherit seq<IDynamic> 
        abstract member Item : int -> IDynamic with get 
        abstract member Count : int with get 
        abstract member IsEmpty : bool with get
    end

and IPropertyDescriptor = interface 
        abstract member Value : IDynamic with get, set 
        abstract member Writable : Nullable<bool> with get, set 
        abstract member Get : IDynamic with get, set   
        abstract member Set : IDynamic with get, set  
        abstract member Enumerable : Nullable<bool> with get, set   
        abstract member Configurable : Nullable<bool> with get, set   
        abstract member IsAccessorDescriptor : bool with get  
        abstract member IsDataDescriptor : bool with get    
        abstract member IsGenericDescriptor : bool with get    
        abstract member IsEmpty : bool with get
    end

and IReferenceBase = interface 
        abstract member Get : string -> bool -> IDynamic 
        abstract member Set : string -> IDynamic -> bool 
    end

and IReference = interface
        inherit IDynamic 
        abstract member Base : IReferenceBase with get
        abstract member Name : string with get
        abstract member IsStrictReference : bool with get
        abstract member HasPrimitiveBase : bool with get
        abstract member IsPropertyReference : bool with get
        abstract member IsUnresolvableReference : bool with get
    end

and IEnvironmentRecord = interface
        inherit IReferenceBase
        abstract member HasBinding : string -> bool 
        abstract member CreateMutableBinding : string -> bool -> unit
        abstract member SetMutableBinding : string -> IDynamic -> bool -> unit  
        abstract member GetBindingValue : string -> bool -> IDynamic  
        abstract member DeleteBinding : string-> bool   
        abstract member ImplicitThisValue : unit -> IDynamic 
    end

and IDeclarativeEnvironmentRecord = interface
        inherit IEnvironmentRecord
        abstract member CreateImmutableBinding : string -> unit  
        abstract member InitializeImmutableBinding : string -> IDynamic -> unit 
    end

and IObjectEnvironmentRecord = interface
        inherit IEnvironmentRecord
    end

and ICallable = interface
        abstract member Call : IEnvironment -> IDynamic -> IArgs -> IDynamic 
    end

and IConstructable = interface
        abstract member Construct : IEnvironment -> IArgs -> IObject 
    end

and IPrimitiveWrapper = interface
        abstract member PrimitiveValue : IDynamic with get, set
    end

and TypeCode =
| Undefined = 0
| Null = 1
| Boolean = 2
| String = 3
| Number = 4
| Object = 5

