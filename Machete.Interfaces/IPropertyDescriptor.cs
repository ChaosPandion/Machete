namespace Machete.Interfaces
{
    public interface IPropertyDescriptor
    {
        IDynamic Value { get; set; }
        bool? Writable { get; set; }
        IDynamic Get { get; set; }
        IDynamic Set { get; set; }
        bool? Enumerable { get; set; }
        bool? Configurable { get; set; }
        bool IsAccessorDescriptor { get; }
        bool IsDataDescriptor { get; }
        bool IsGenericDescriptor { get; }
        bool IsEmpty { get; }
    }
}