using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Machete.Core;

[assembly: AssemblyTitle("Machete.Core")]
[assembly: AssemblyDescription(AssemblyInfo.Description)]
[assembly: AssemblyConfiguration(AssemblyInfo.Configuration)]
[assembly: AssemblyCompany(AssemblyInfo.Company)]
[assembly: AssemblyProduct(AssemblyInfo.Product)]
[assembly: AssemblyCopyright(AssemblyInfo.Copyright)]
[assembly: AssemblyTrademark(AssemblyInfo.Trademark)]
[assembly: AssemblyCulture(AssemblyInfo.Culture)]
[assembly: ComVisible(false)]
[assembly: Guid("98a98e20-6bfb-4543-b576-3bb38a6cf9ad")]
[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.Version)]

namespace Machete.Core
{
    public static class AssemblyInfo
    {
        public const string Version = "0.4.5.2020";
        public const string Description = "Machete - A scripting runtime for .NET";
        public const string Configuration = "";
        public const string Company = "";
        public const string Product = "Machete";
        public const string Copyright = "Copyright © 2011 Matthew O'Brien";
        public const string Trademark = "";
        public const string Culture = "";
    }
}