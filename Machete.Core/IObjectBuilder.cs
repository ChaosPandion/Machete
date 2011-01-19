namespace Machete.Core
{
    public interface IObjectBuilder
    {
        IObjectBuilder SetAttributes(bool? writable, bool? enumerable, bool? configurable);
        IObjectBuilder AppendDataProperty(string name, IDynamic value);
        IObjectBuilder AppendAccessorProperty(string name, IDynamic get, IDynamic set);
        IObject ToObject();
    }
}