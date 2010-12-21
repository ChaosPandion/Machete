namespace Machete.Interfaces
{
    public class MacheteRuntimeException : MacheteException
    {
        private readonly IDynamic _thrown;


        public override string Message
        {
            get { 
                return _thrown.ToString(); }
        }

        public MacheteRuntimeException(IDynamic thrown)
        {
            _thrown = thrown;
        }


        public override string ToString()
        {
            return _thrown.ToString();
        }
    }
}