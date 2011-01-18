namespace Machete.Core
{
    public interface IConstructable
    {
        IObject Construct(IEnvironment environment, IArgs args);
    }
}