namespace Machete

open System.Reflection
open System.Runtime.InteropServices

module AssemblyInfo =
    let [<Literal>] Version = "0.1.1.1"
    let [<Literal>] Title = "Machete"
    let [<Literal>] Description = "Machete - A scripting runtime for .NET"
    let [<Literal>] Configuration = ""
    let [<Literal>] Company = ""
    let [<Literal>] Product = "Machete"
    let [<Literal>] Copyright = "Copyright © 2011 Matthew O'Brien"
    let [<Literal>] Trademark = ""
    let [<Literal>] Culture = ""
 
[<assembly: AssemblyTitle(AssemblyInfo.Title)>]
[<assembly: AssemblyDescription(AssemblyInfo.Description)>]
[<assembly: AssemblyConfiguration(AssemblyInfo.Configuration)>]
[<assembly: AssemblyCompany(AssemblyInfo.Company)>]
[<assembly: AssemblyProduct(AssemblyInfo.Product)>]
[<assembly: AssemblyCopyright(AssemblyInfo.Copyright)>]
[<assembly: AssemblyTrademark(AssemblyInfo.Trademark)>]
[<assembly: AssemblyCulture(AssemblyInfo.Culture)>] 
[<assembly: ComVisible(false)>]
[<assembly: Guid("15AC1295-21CD-4D04-BDCF-F6AF8A372FAB")>]
[<assembly: AssemblyVersion(AssemblyInfo.Version)>]
[<assembly: AssemblyFileVersion(AssemblyInfo.Version)>]
()