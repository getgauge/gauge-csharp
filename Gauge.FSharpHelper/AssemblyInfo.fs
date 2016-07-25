namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Gauge.FSharpHelper")>]
[<assembly: GuidAttribute("6c5b61aa-27d6-11e6-b67b-9e71128cae77")>]
[<assembly: AssemblyDescriptionAttribute("F# helper to build C# gauge spec projects using Fake.Lib")>]
[<assembly: AssemblyVersionAttribute("0.9.1")>]
[<assembly: AssemblyFileVersionAttribute("0.9.1")>]
[<assembly: AssemblyConfigurationAttribute("")>]
[<assembly: AssemblyCompanyAttribute("ThoughtWorks Inc.")>]
[<assembly: AssemblyProductAttribute("Gauge.CSharp.Core")>]
[<assembly: AssemblyCopyrightAttribute("Copyright ©  2016")>]
[<assembly: AssemblyTrademarkAttribute("")>]
[<assembly: AssemblyCultureAttribute("")>]
[<assembly: ComVisibleAttribute(true)>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.9.1"
    let [<Literal>] InformationalVersion = "0.9.1"
