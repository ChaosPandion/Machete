using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.NativeObjects.BuiltinObjects;
using Machete.Runtime.NativeObjects;
using Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects;
using Machete.Interfaces;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public sealed class SReference : IReference
    {
        private readonly IEnvironment _environment;
        private readonly IReferenceBase _base;
        private readonly string _referencedName;
        private readonly bool _strictReference;


        public SReference(IEnvironment environment, IReferenceBase @base, string referencedName, bool strictReference)
        {
            _environment = environment;
            _base = @base;
            _referencedName = referencedName;
            _strictReference = strictReference;
        }


        public IReferenceBase Base
        {
            get { return _base; }
        }

        public string Name
        {
            get { return _referencedName; }
        }

        public bool IsStrictReference
        {
            get { return _strictReference; }
        }

        public bool HasPrimitiveBase
        {
            get { return _base is IBoolean || _base is IString || _base is INumber; }
        }

        public bool IsPropertyReference
        {
            get { return _base is IObject || HasPrimitiveBase; }
        }

        public bool IsUnresolvableReference
        {
            get { return _base is IUndefined; }
        }

        public IDynamic Value
        {
            get { return _base.Get(_referencedName, _strictReference); }
            set
            {
                StrictReferenceCondition();
                _base.Set(_referencedName, value, _strictReference); 
            }
        }

        public LanguageTypeCode TypeCode
        {
            get { return Value.TypeCode; }
        }

        public bool IsPrimitive
        {
            get { return Value.IsPrimitive; }
        }
        

        public IDynamic Op_LogicalNot()
        {
            return Value.Op_LogicalNot();
        }

        public IDynamic Op_LogicalOr(IDynamic other)
        {
            return Value.Op_LogicalOr(other);
        }

        public IDynamic Op_LogicalAnd(IDynamic other)
        {
            return Value.Op_LogicalOr(other);
        }

        public IDynamic Op_BitwiseNot()
        {
            return Value.Op_BitwiseNot();
        }

        public IDynamic Op_BitwiseOr(IDynamic other)
        {
            return Value.Op_BitwiseOr(other);
        }

        public IDynamic Op_BitwiseXor(IDynamic other)
        {
            return Value.Op_BitwiseXor(other);
        }

        public IDynamic Op_BitwiseAnd(IDynamic other)
        {
            return Value.Op_BitwiseAnd(other);
        }

        public IDynamic Op_Equals(IDynamic other)
        {
            return Value.Op_Equals(other);
        }

        public IDynamic Op_DoesNotEquals(IDynamic other)
        {
            return Value.Op_DoesNotEquals(other);
        }

        public IDynamic Op_StrictEquals(IDynamic other)
        {
            return Value.Op_StrictEquals(other);
        }

        public IDynamic Op_StrictDoesNotEquals(IDynamic other)
        {
            return Value.Op_StrictDoesNotEquals(other);
        }

        public IDynamic Op_Lessthan(IDynamic other)
        {
            return Value.Op_Lessthan(other);
        }

        public IDynamic Op_Greaterthan(IDynamic other)
        {
            return Value.Op_Greaterthan(other);
        }

        public IDynamic Op_LessthanOrEqual(IDynamic other)
        {
            return Value.Op_LessthanOrEqual(other);
        }

        public IDynamic Op_GreaterthanOrEqual(IDynamic other)
        {
            return Value.Op_GreaterthanOrEqual(other);
        }

        public IDynamic Op_Instanceof(IDynamic other)
        {
            return Value.Op_Instanceof(other);
        }

        public IDynamic Op_In(IDynamic other)
        {
            return Value.Op_In(other);
        }

        public IDynamic Op_LeftShift(IDynamic other)
        {
            return Value.Op_LeftShift(other);
        }

        public IDynamic Op_SignedRightShift(IDynamic other)
        {
            return Value.Op_SignedRightShift(other);
        }

        public IDynamic Op_UnsignedRightShift(IDynamic other)
        {
            return Value.Op_UnsignedRightShift(other);
        }

        public IDynamic Op_Addition(IDynamic other)
        {
            return Value.Op_Addition(other);
        }

        public IDynamic Op_Subtraction(IDynamic other)
        {
            return Value.Op_Subtraction(other);
        }

        public IDynamic Op_Multiplication(IDynamic other)
        {
            return Value.Op_Multiplication(other);
        }

        public IDynamic Op_Division(IDynamic other)
        {
            return Value.Op_Division(other);
        }

        public IDynamic Op_Modulus(IDynamic other)
        {
            return Value.Op_Modulus(other);
        }

        public IDynamic Op_Delete()
        {
            if (_base is LUndefined)
            {
                if (_strictReference)
                {
                    _environment.CreateSyntaxError().Op_Throw();
                    return null;
                }
                return _environment.True;
            }
            else if (_base is IEnvironmentRecord)
            {
                if (_strictReference)
                {
                    _environment.CreateSyntaxError().Op_Throw();
                    return null;
                }
                return _environment.CreateBoolean(((IEnvironmentRecord)_base).DeleteBinding(_referencedName));
            }
            else
            {
                return _environment.CreateBoolean(((IDynamic)_base).ConvertToObject().Delete(_referencedName, _strictReference));
            }
        }

        public IDynamic Op_Void()
        {
            var v = Value;
            return _environment.Undefined;
        }

        public IDynamic Op_Typeof()
        {
            return Value.Op_Typeof();
        }

        public IDynamic Op_PrefixIncrement()
        {
            StrictReferenceCondition();
            return Value = _environment.CreateNumber(Value.ConvertToNumber().BaseValue + 1.0);
        }

        public IDynamic Op_PrefixDecrement()
        {
            StrictReferenceCondition();
            return Value = _environment.CreateNumber(Value.ConvertToNumber().BaseValue - 1.0);
        }

        public IDynamic Op_Plus()
        {
            return Value.Op_Plus();
        }

        public IDynamic Op_Minus()
        {
            return Value.Op_Minus();
        }

        public IDynamic Op_PostfixIncrement()
        {
            StrictReferenceCondition();
            var oldValue = Value.ConvertToNumber();
            Value = _environment.CreateNumber(oldValue.BaseValue + 1.0);
            return oldValue;
        }

        public IDynamic Op_PostfixDecrement()
        {
            StrictReferenceCondition();
            var oldValue = Value.ConvertToNumber();
            Value = _environment.CreateNumber(oldValue.BaseValue - 1.0);
            return oldValue;
        }

        public IDynamic Op_GetProperty(IDynamic name)
        {
            return Value.Op_GetProperty(name);
        }

        public void Op_SetProperty(IDynamic name, IDynamic value)
        {
            Value.Op_SetProperty(name, value);
        }

        public IDynamic Op_Call(IArgs args)
        {
            var callable = Value as ICallable;
            if (callable == null)
            {
                _environment.CreateTypeError().Op_Throw();
                return null;
            }
            if (IsPropertyReference)
            {
                return callable.Call(_environment, (IDynamic)_base, args);
            }
            return callable.Call(_environment, ((IEnvironmentRecord)_base).ImplicitThisValue(), args);
        }

        public IObject Op_Construct(IArgs args)
        {
            return Value.Op_Construct(args);
        }

        public void Op_Throw()
        {
            Value.Op_Throw();
        }

        public IDynamic ConvertToPrimitive(string preferredType = null)
        {
            return Value.ConvertToPrimitive(preferredType);
        }

        public IBoolean ConvertToBoolean()
        {
            return Value.ConvertToBoolean();
        }

        public INumber ConvertToNumber()
        {
            return Value.ConvertToNumber();
        }

        public IString ConvertToString()
        {
            return Value.ConvertToString();
        }

        public IObject ConvertToObject()
        {
            return Value.ConvertToObject();
        }

        public INumber ConvertToInteger()
        {
            return Value.ConvertToInteger();
        }

        public INumber ConvertToInt32()
        {
            return Value.ConvertToInt32();
        }

        public INumber ConvertToUInt32()
        {
            return Value.ConvertToUInt32();
        }

        public INumber ConvertToUInt16()
        {
            return Value.ConvertToUInt16();
        }


        private void StrictReferenceCondition()
        {
            if (_strictReference && _base is IEnvironmentRecord && (_referencedName == "eval" || _referencedName == "arguments"))
            {
                _environment.CreateSyntaxError().Op_Throw();
            }
        }
    }
}