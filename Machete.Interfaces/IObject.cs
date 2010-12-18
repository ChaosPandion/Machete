using System.Collections.Generic;

namespace Machete.Interfaces
{
    public interface IObject : IDynamic, IReferenceBase, IEnumerable<string>
    {
        IObject Prototype { get; set; }
        string Class { get; set; }
        bool Extensible { get; set; }

        IPropertyDescriptor GetOwnProperty(string name);
        IPropertyDescriptor GetProperty(string name);
        IDynamic Get(string name);
        void Put(string name, IDynamic value, bool strict);
        bool CanPut(string name);
        bool HasProperty(string name);
        bool Delete(string name, bool strict);
        IDynamic DefaultValue(string hint);
        bool DefineOwnProperty(string name, IPropertyDescriptor value, bool strict);
    }
}