namespace Machete.Core
{
    public interface ICallable
    {
        IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args);
    }
}