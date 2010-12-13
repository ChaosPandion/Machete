namespace Machete.Interfaces
{
    public interface IConstructable
    {
        IObject Construct(IEnvironment environment, IArgs args);
    }
}