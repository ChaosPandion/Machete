using System;
using System.Runtime.Serialization;

namespace Machete.Core
{
    public class MacheteException : Exception
    {
        public MacheteException()
        {

        }

        public MacheteException(string message)
            : base(message)
        {

        }

        public MacheteException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public MacheteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}