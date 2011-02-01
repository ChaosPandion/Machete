namespace Machete.Interactive

open System.Reflection
open System.Runtime.InteropServices

module internal AssemblyInfo =
    let [<Literal>] Version = "0.1.0.1106"
 
[<assembly: AssemblyTitle("Machete.Interactive")>]
[<assembly: AssemblyDescription("An interactive REPL for the Machete scripting runtime.")>]
[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCompany("")>]
[<assembly: AssemblyProduct("Machete Interactive")>]
[<assembly: AssemblyCopyright("Copyright © 2011 Matthew O'Brien")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: ComVisible(false)>]
[<assembly: Guid("F0D50083-6F6E-49D7-9080-D8653A594378")>]
[<assembly: AssemblyVersion(AssemblyInfo.Version)>]
[<assembly: AssemblyFileVersion(AssemblyInfo.Version)>]
()