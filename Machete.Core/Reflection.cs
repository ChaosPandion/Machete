using System;
using System.Reflection;
using Machete.Core.Generators;

namespace Machete.Core
{
    public static class Reflection
    {
        public static class IReferenceBaseMemberInfo
        {
            public static readonly Type Type = typeof(IReferenceBase);
            public static readonly MethodInfo Get = Type.GetMethod("Get");
            public static readonly MethodInfo Set = Type.GetMethod("Set");
        }

        public static class IPropertyDescriptorMemberInfo
        {
            public static readonly Type Type = typeof(IPropertyDescriptor);
            public static readonly PropertyInfo Value = Type.GetProperty("Value");
            public static readonly PropertyInfo Writable = Type.GetProperty("Writable");
            public static readonly PropertyInfo Get = Type.GetProperty("Get");
            public static readonly PropertyInfo Set = Type.GetProperty("Set");
            public static readonly PropertyInfo Enumerable = Type.GetProperty("Enumerable");
            public static readonly PropertyInfo Configurable = Type.GetProperty("Configurable");
            public static readonly PropertyInfo IsAccessorDescriptor = Type.GetProperty("IsAccessorDescriptor");
            public static readonly PropertyInfo IsDataDescriptor = Type.GetProperty("IsDataDescriptor");
            public static readonly PropertyInfo IsGenericDescriptor = Type.GetProperty("IsGenericDescriptor");
            public static readonly PropertyInfo IsEmpty = Type.GetProperty("IsEmpty");
            public static readonly MethodInfo Copy = Type.GetMethod("Copy");
        }

        public static class IHasInstanceMemberInfo
        {
            public static readonly Type Type = typeof(IHasInstance);
            public static readonly MethodInfo HasInstance = Type.GetMethod("HasInstance");
        }

        public static class IDynamicMemberInfo
        {
            public static readonly Type Type = typeof(IDynamic);
            public static readonly PropertyInfo TypeCode = Type.GetProperty("TypeCode");
            public static readonly PropertyInfo IsPrimitive = Type.GetProperty("IsPrimitive");
            public static readonly PropertyInfo Value = Type.GetProperty("Value");
            public static readonly MethodInfo Op_Modulus = Type.GetMethod("Op_Modulus");
            public static readonly MethodInfo Op_Delete = Type.GetMethod("Op_Delete");
            public static readonly MethodInfo Op_Void = Type.GetMethod("Op_Void");
            public static readonly MethodInfo Op_Typeof = Type.GetMethod("Op_Typeof");
            public static readonly MethodInfo Op_PrefixIncrement = Type.GetMethod("Op_PrefixIncrement");
            public static readonly MethodInfo Op_PrefixDecrement = Type.GetMethod("Op_PrefixDecrement");
            public static readonly MethodInfo Op_Plus = Type.GetMethod("Op_Plus");
            public static readonly MethodInfo Op_Minus = Type.GetMethod("Op_Minus");
            public static readonly MethodInfo Op_PostfixIncrement = Type.GetMethod("Op_PostfixIncrement");
            public static readonly MethodInfo Op_PostfixDecrement = Type.GetMethod("Op_PostfixDecrement");
            public static readonly MethodInfo Op_GetProperty = Type.GetMethod("Op_GetProperty");
            public static readonly MethodInfo Op_SetProperty = Type.GetMethod("Op_SetProperty");
            public static readonly MethodInfo Op_Call = Type.GetMethod("Op_Call");
            public static readonly MethodInfo Op_Construct = Type.GetMethod("Op_Construct");
            public static readonly MethodInfo Op_Throw = Type.GetMethod("Op_Throw");
            public static readonly MethodInfo ConvertToPrimitive = Type.GetMethod("ConvertToPrimitive");
            public static readonly MethodInfo ConvertToBoolean = Type.GetMethod("ConvertToBoolean");
            public static readonly MethodInfo ConvertToNumber = Type.GetMethod("ConvertToNumber");
            public static readonly MethodInfo ConvertToString = Type.GetMethod("ConvertToString");
            public static readonly MethodInfo ConvertToObject = Type.GetMethod("ConvertToObject");
            public static readonly MethodInfo ConvertToInteger = Type.GetMethod("ConvertToInteger");
            public static readonly MethodInfo ConvertToInt32 = Type.GetMethod("ConvertToInt32");
            public static readonly MethodInfo ConvertToUInt32 = Type.GetMethod("ConvertToUInt32");
            public static readonly MethodInfo ConvertToUInt16 = Type.GetMethod("ConvertToUInt16");
            public static readonly MethodInfo Op_LogicalNot = Type.GetMethod("Op_LogicalNot");
            public static readonly MethodInfo Op_LogicalOr = Type.GetMethod("Op_LogicalOr");
            public static readonly MethodInfo Op_LogicalAnd = Type.GetMethod("Op_LogicalAnd");
            public static readonly MethodInfo Op_BitwiseNot = Type.GetMethod("Op_BitwiseNot");
            public static readonly MethodInfo Op_BitwiseOr = Type.GetMethod("Op_BitwiseOr");
            public static readonly MethodInfo Op_BitwiseXor = Type.GetMethod("Op_BitwiseXor");
            public static readonly MethodInfo Op_BitwiseAnd = Type.GetMethod("Op_BitwiseAnd");
            public static readonly MethodInfo Op_Equals = Type.GetMethod("Op_Equals");
            public static readonly MethodInfo Op_DoesNotEquals = Type.GetMethod("Op_DoesNotEquals");
            public static readonly MethodInfo Op_StrictEquals = Type.GetMethod("Op_StrictEquals");
            public static readonly MethodInfo Op_StrictDoesNotEquals = Type.GetMethod("Op_StrictDoesNotEquals");
            public static readonly MethodInfo Op_Lessthan = Type.GetMethod("Op_Lessthan");
            public static readonly MethodInfo Op_Greaterthan = Type.GetMethod("Op_Greaterthan");
            public static readonly MethodInfo Op_LessthanOrEqual = Type.GetMethod("Op_LessthanOrEqual");
            public static readonly MethodInfo Op_GreaterthanOrEqual = Type.GetMethod("Op_GreaterthanOrEqual");
            public static readonly MethodInfo Op_Instanceof = Type.GetMethod("Op_Instanceof");
            public static readonly MethodInfo Op_In = Type.GetMethod("Op_In");
            public static readonly MethodInfo Op_LeftShift = Type.GetMethod("Op_LeftShift");
            public static readonly MethodInfo Op_SignedRightShift = Type.GetMethod("Op_SignedRightShift");
            public static readonly MethodInfo Op_UnsignedRightShift = Type.GetMethod("Op_UnsignedRightShift");
            public static readonly MethodInfo Op_Addition = Type.GetMethod("Op_Addition");
            public static readonly MethodInfo Op_Subtraction = Type.GetMethod("Op_Subtraction");
            public static readonly MethodInfo Op_Multiplication = Type.GetMethod("Op_Multiplication");
            public static readonly MethodInfo Op_Division = Type.GetMethod("Op_Division");
        }

        public static class IReferenceMemberInfo
        {
            public static readonly Type Type = typeof(IReference);
            public static readonly PropertyInfo Base = Type.GetProperty("Base");
            public static readonly PropertyInfo Name = Type.GetProperty("Name");
            public static readonly PropertyInfo IsStrictReference = Type.GetProperty("IsStrictReference");
            public static readonly PropertyInfo HasPrimitiveBase = Type.GetProperty("HasPrimitiveBase");
            public static readonly PropertyInfo IsPropertyReference = Type.GetProperty("IsPropertyReference");
            public static readonly PropertyInfo IsUnresolvableReference = Type.GetProperty("IsUnresolvableReference");
        }

        public static class IEnvironmentRecordMemberInfo
        {
            public static readonly Type Type = typeof(IEnvironmentRecord);
            public static readonly MethodInfo HasBinding = Type.GetMethod("HasBinding");
            public static readonly MethodInfo CreateMutableBinding = Type.GetMethod("CreateMutableBinding");
            public static readonly MethodInfo SetMutableBinding = Type.GetMethod("SetMutableBinding");
            public static readonly MethodInfo GetBindingValue = Type.GetMethod("GetBindingValue");
            public static readonly MethodInfo DeleteBinding = Type.GetMethod("DeleteBinding");
            public static readonly MethodInfo ImplicitThisValue = Type.GetMethod("ImplicitThisValue");
        }

        public static class ICallableMemberInfo
        {
            public static readonly Type Type = typeof(ICallable);
            public static readonly MethodInfo Call = Type.GetMethod("Call");
        }

        public static class INumberMemberInfo
        {
            public static readonly Type Type = typeof(INumber);
            public static readonly PropertyInfo BaseValue = Type.GetProperty("BaseValue");
        }

        public static class IBooleanMemberInfo
        {
            public static readonly Type Type = typeof(IBoolean);
            public static readonly PropertyInfo BaseValue = Type.GetProperty("BaseValue");
        }

        public static class IExecutionContextMemberInfo
        {
            public static readonly Type Type = typeof(IExecutionContext);
            public static readonly PropertyInfo LexicalEnviroment = Type.GetProperty("LexicalEnviroment");
            public static readonly PropertyInfo VariableEnviroment = Type.GetProperty("VariableEnviroment");
            public static readonly PropertyInfo ThisBinding = Type.GetProperty("ThisBinding");
            public static readonly PropertyInfo Strict = Type.GetProperty("Strict");
            public static readonly PropertyInfo CurrentFunction = Type.GetProperty("CurrentFunction");
        }

        public static class IDeclarativeEnvironmentRecordMemberInfo
        {
            public static readonly Type Type = typeof(IDeclarativeEnvironmentRecord);
            public static readonly MethodInfo CreateImmutableBinding = Type.GetMethod("CreateImmutableBinding");
            public static readonly MethodInfo InitializeImmutableBinding = Type.GetMethod("InitializeImmutableBinding");
        }

        public static class IUndefinedMemberInfo
        {
            public static readonly Type Type = typeof(IUndefined);
        }

        public static class ILexicalEnvironmentMemberInfo
        {
            public static readonly Type Type = typeof(ILexicalEnvironment);
            public static readonly PropertyInfo Record = Type.GetProperty("Record");
            public static readonly PropertyInfo Parent = Type.GetProperty("Parent");
            public static readonly MethodInfo GetIdentifierReference = Type.GetMethod("GetIdentifierReference");
            public static readonly MethodInfo NewDeclarativeEnvironment = Type.GetMethod("NewDeclarativeEnvironment");
            public static readonly MethodInfo NewObjectEnvironment = Type.GetMethod("NewObjectEnvironment");
        }

        public static class IObjectMemberInfo
        {
            public static readonly Type Type = typeof(IObject);
            public static readonly PropertyInfo Prototype = Type.GetProperty("Prototype");
            public static readonly PropertyInfo Class = Type.GetProperty("Class");
            public static readonly PropertyInfo Extensible = Type.GetProperty("Extensible");
            public static readonly MethodInfo GetOwnProperty = Type.GetMethod("GetOwnProperty");
            public static readonly MethodInfo GetProperty = Type.GetMethod("GetProperty");
            public static readonly MethodInfo Get = Type.GetMethod("Get");
            public static readonly MethodInfo Put = Type.GetMethod("Put");
            public static readonly MethodInfo CanPut = Type.GetMethod("CanPut");
            public static readonly MethodInfo HasProperty = Type.GetMethod("HasProperty");
            public static readonly MethodInfo Delete = Type.GetMethod("Delete");
            public static readonly MethodInfo DefaultValue = Type.GetMethod("DefaultValue");
            public static readonly MethodInfo DefineOwnProperty = Type.GetMethod("DefineOwnProperty");
            public static readonly MethodInfo Initialize = Type.GetMethod("Initialize");
        }

        public static class IConstructableMemberInfo
        {
            public static readonly Type Type = typeof(IConstructable);
            public static readonly MethodInfo Construct = Type.GetMethod("Construct");
        }

        public static class IFunctionMemberInfo
        {
            public static readonly Type Type = typeof(IFunction);
            public static readonly PropertyInfo Scope = Type.GetProperty("Scope");
            public static readonly PropertyInfo FormalParameterList = Type.GetProperty("FormalParameterList");
            public static readonly PropertyInfo Code = Type.GetProperty("Code");
            public static readonly PropertyInfo TargetFunction = Type.GetProperty("TargetFunction");
            public static readonly PropertyInfo BoundThis = Type.GetProperty("BoundThis");
            public static readonly PropertyInfo BoundArguments = Type.GetProperty("BoundArguments");
            public static readonly PropertyInfo Strict = Type.GetProperty("Strict");
            public static readonly PropertyInfo BindFunction = Type.GetProperty("BindFunction");
        }

        public static class IBuiltinFunctionMemberInfo
        {
            public static readonly Type Type = typeof(IBuiltinFunction);
        }

        public static class IPrimitiveWrapperMemberInfo
        {
            public static readonly Type Type = typeof(IPrimitiveWrapper);
            public static readonly PropertyInfo PrimitiveValue = Type.GetProperty("PrimitiveValue");
        }

        public static class IEnvironmentMemberInfo
        {
            public static readonly Type Type = typeof(IEnvironment);
            public static readonly PropertyInfo Output = Type.GetProperty("Output");
            public static readonly PropertyInfo Context = Type.GetProperty("Context");
            public static readonly PropertyInfo GlobalEnvironment = Type.GetProperty("GlobalEnvironment");
            public static readonly PropertyInfo EmptyArgs = Type.GetProperty("EmptyArgs");
            public static readonly PropertyInfo Undefined = Type.GetProperty("Undefined");
            public static readonly PropertyInfo Null = Type.GetProperty("Null");
            public static readonly PropertyInfo True = Type.GetProperty("True");
            public static readonly PropertyInfo False = Type.GetProperty("False");
            public static readonly PropertyInfo GlobalObject = Type.GetProperty("GlobalObject");
            public static readonly PropertyInfo ObjectConstructor = Type.GetProperty("ObjectConstructor");
            public static readonly PropertyInfo ObjectPrototype = Type.GetProperty("ObjectPrototype");
            public static readonly PropertyInfo FunctionConstructor = Type.GetProperty("FunctionConstructor");
            public static readonly PropertyInfo FunctionPrototype = Type.GetProperty("FunctionPrototype");
            public static readonly PropertyInfo ArrayConstructor = Type.GetProperty("ArrayConstructor");
            public static readonly PropertyInfo ArrayPrototype = Type.GetProperty("ArrayPrototype");
            public static readonly PropertyInfo StringConstructor = Type.GetProperty("StringConstructor");
            public static readonly PropertyInfo StringPrototype = Type.GetProperty("StringPrototype");
            public static readonly PropertyInfo BooleanConstructor = Type.GetProperty("BooleanConstructor");
            public static readonly PropertyInfo BooleanPrototype = Type.GetProperty("BooleanPrototype");
            public static readonly PropertyInfo NumberConstructor = Type.GetProperty("NumberConstructor");
            public static readonly PropertyInfo NumberPrototype = Type.GetProperty("NumberPrototype");
            public static readonly PropertyInfo MathObject = Type.GetProperty("MathObject");
            public static readonly PropertyInfo DateConstructor = Type.GetProperty("DateConstructor");
            public static readonly PropertyInfo DatePrototype = Type.GetProperty("DatePrototype");
            public static readonly PropertyInfo RegExpConstructor = Type.GetProperty("RegExpConstructor");
            public static readonly PropertyInfo RegExpPrototype = Type.GetProperty("RegExpPrototype");
            public static readonly PropertyInfo ErrorConstructor = Type.GetProperty("ErrorConstructor");
            public static readonly PropertyInfo ErrorPrototype = Type.GetProperty("ErrorPrototype");
            public static readonly PropertyInfo EvalErrorConstructor = Type.GetProperty("EvalErrorConstructor");
            public static readonly PropertyInfo EvalErrorPrototype = Type.GetProperty("EvalErrorPrototype");
            public static readonly PropertyInfo RangeErrorConstructor = Type.GetProperty("RangeErrorConstructor");
            public static readonly PropertyInfo RangeErrorPrototype = Type.GetProperty("RangeErrorPrototype");
            public static readonly PropertyInfo ReferenceErrorConstructor = Type.GetProperty("ReferenceErrorConstructor");
            public static readonly PropertyInfo ReferenceErrorPrototype = Type.GetProperty("ReferenceErrorPrototype");
            public static readonly PropertyInfo SyntaxErrorConstructor = Type.GetProperty("SyntaxErrorConstructor");
            public static readonly PropertyInfo SyntaxErrorPrototype = Type.GetProperty("SyntaxErrorPrototype");
            public static readonly PropertyInfo TypeErrorConstructor = Type.GetProperty("TypeErrorConstructor");
            public static readonly PropertyInfo TypeErrorPrototype = Type.GetProperty("TypeErrorPrototype");
            public static readonly PropertyInfo UriErrorConstructor = Type.GetProperty("UriErrorConstructor");
            public static readonly PropertyInfo UriErrorPrototype = Type.GetProperty("UriErrorPrototype");
            public static readonly PropertyInfo JsonObject = Type.GetProperty("JsonObject");
            public static readonly PropertyInfo ThrowTypeErrorFunction = Type.GetProperty("ThrowTypeErrorFunction");
            public static readonly MethodInfo CreateRangeError = Type.GetMethod("CreateRangeError");
            public static readonly MethodInfo CreateReferenceError = Type.GetMethod("CreateReferenceError");
            public static readonly MethodInfo CreateSyntaxError = Type.GetMethod("CreateSyntaxError");
            public static readonly MethodInfo CreateTypeError = Type.GetMethod("CreateTypeError");
            public static readonly MethodInfo CreateUriError = Type.GetMethod("CreateUriError");
            public static readonly MethodInfo CreateIterableFromGenerator = Type.GetMethod("CreateIterableFromGenerator");
            public static readonly MethodInfo CombineGeneratorWithIterator = Type.GetMethod("CombineGeneratorWithIterator");
            public static readonly MethodInfo CreateFunction = Type.GetMethod("CreateFunction");
            public static readonly MethodInfo CreateGenericDescriptor = Type.GetMethod("CreateGenericDescriptor");
            public static readonly MethodInfo CreateDataDescriptor = Type.GetMethod("CreateDataDescriptor");
            public static readonly MethodInfo CreateAccessorDescriptor = Type.GetMethod("CreateAccessorDescriptor");
            public static readonly MethodInfo EnterContext = Type.GetMethod("EnterContext");
            public static readonly MethodInfo ThrowRuntimeException = Type.GetMethod("ThrowRuntimeException");
            public static readonly MethodInfo CheckObjectCoercible = Type.GetMethod("CheckObjectCoercible");
            public static readonly MethodInfo Instanceof = Type.GetMethod("Instanceof");
            public static readonly MethodInfo Execute = Type.GetMethod("Execute");
            public static readonly MethodInfo BindFunctionDeclarations = Type.GetMethod("BindFunctionDeclarations");
            public static readonly MethodInfo BindVariableDeclarations = Type.GetMethod("BindVariableDeclarations");
            public static readonly MethodInfo CreateReference = Type.GetMethod("CreateReference");
            public static readonly MethodInfo CreateBoolean = Type.GetMethod("CreateBoolean");
            public static readonly MethodInfo CreateString = Type.GetMethod("CreateString");
            public static readonly MethodInfo CreateNumber = Type.GetMethod("CreateNumber");
            public static readonly MethodInfo CreateArgs = Type.GetMethod("CreateArgs");
            public static readonly MethodInfo ConcatArgs = Type.GetMethod("ConcatArgs");
            public static readonly MethodInfo FromPropertyDescriptor = Type.GetMethod("FromPropertyDescriptor");
            public static readonly MethodInfo ToPropertyDescriptor = Type.GetMethod("ToPropertyDescriptor");
            public static readonly MethodInfo CreateObjectBuilder = Type.GetMethod("CreateObjectBuilder");
            public static readonly MethodInfo CreateArray = Type.GetMethod("CreateArray");
            public static readonly MethodInfo CreateObject = Type.GetMethod("CreateObject");
            public static readonly MethodInfo CreateRegExp = Type.GetMethod("CreateRegExp");
            public static readonly MethodInfo CreateError = Type.GetMethod("CreateError");
            public static readonly MethodInfo CreateEvalError = Type.GetMethod("CreateEvalError");
            public static readonly MethodInfo ForeachLoop = Type.GetMethod("ForeachLoop");
        }

        public static class IArgsMemberInfo
        {
            public static readonly Type Type = typeof(IArgs);
            public static readonly PropertyInfo Item = Type.GetProperty("Item");
            public static readonly PropertyInfo Count = Type.GetProperty("Count");
            public static readonly PropertyInfo IsEmpty = Type.GetProperty("IsEmpty");
        }

        public static class IStringMemberInfo
        {
            public static readonly Type Type = typeof(IString);
            public static readonly PropertyInfo BaseValue = Type.GetProperty("BaseValue");
        }

        public static class INullMemberInfo
        {
            public static readonly Type Type = typeof(INull);
        }

        public static class IObjectEnvironmentRecordMemberInfo
        {
            public static readonly Type Type = typeof(IObjectEnvironmentRecord);
        }

        public static class IObjectBuilderMemberInfo
        {
            public static readonly Type Type = typeof(IObjectBuilder);
            public static readonly MethodInfo SetAttributes = Type.GetMethod("SetAttributes");
            public static readonly MethodInfo AppendDataProperty = Type.GetMethod("AppendDataProperty");
            public static readonly MethodInfo AppendAccessorProperty = Type.GetMethod("AppendAccessorProperty");
            public static readonly MethodInfo ToObject = Type.GetMethod("ToObject");
        }

        public static class GeneratorMemberInfo
        {
            public static readonly Type Type = typeof(Generator);
            public static readonly ConstructorInfo Constructor = Type.GetConstructor(new[] { typeof(GeneratorSteps) });
            public static readonly PropertyInfo Steps = Type.GetProperty("Steps");
            public static readonly PropertyInfo Current = Type.GetProperty("Current");
            public static readonly PropertyInfo Complete = Type.GetProperty("Complete");
            public static readonly PropertyInfo Initialized = Type.GetProperty("Initialized");
        }
    }
}