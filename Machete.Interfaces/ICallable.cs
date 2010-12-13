namespace Machete.Interfaces
{
    public interface ICallable
    {
        IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args);
    }
}