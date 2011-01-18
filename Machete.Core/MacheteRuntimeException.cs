namespace Machete.Core
{
    public class MacheteRuntimeException : MacheteException
    {
        public IDynamic Thrown { get; private set; }

        public override string Message
        {
            get { return Thrown.ToString(); }
        }

        public MacheteRuntimeException(IDynamic thrown)
        {
            Thrown = thrown;
        }


        public override string ToString()
        {
            return Thrown.ToString();
        }
    }
}