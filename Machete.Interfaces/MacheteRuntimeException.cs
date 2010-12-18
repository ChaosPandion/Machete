namespace Machete.Interfaces
{
    public class MacheteRuntimeException : MacheteException
    {
        public IDynamic Thrown { get; private set; }


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