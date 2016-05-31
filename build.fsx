// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open System
open System.IO
#if MONO
#else
#load "packages/build/SourceLink.Fake/tools/Fake.fsx"
open SourceLink
#endif

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docs/tools/generate.fsx"

// The name of the project
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "gauge-csharp"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "Gauge csharp runner"

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "Project has no description; update build.fsx"

// List of author names (for NuGet package)
let authors = [ "Update Author in build.fsx" ]

// Tags for your project (for NuGet package)
let tags = ""

// Read additional information from the release notes document
let releaseRunner = LoadReleaseNotes "CHANGELOG.md"
let libRelease = LoadReleaseNotes "Lib/CHANGELOG.md"
let coreRelease = LoadReleaseNotes "Core/CHANGELOG.md"

// Pattern specifying assemblies to be tested using NUnit
let testAssemblies = "artifacts/gauge-csharp/*tests/*Test*.dll"
let testAssembliesLib = "artifacts/gauge-csharp-lib/tests/*Test*.dll"
let testAssembliesRunner = "artifacts/gauge-csharp/tests/*Test*.dll"
let itestAssembliesRunner = "artifacts/gauge-csharp/itests/*IntegrationTest*.dll"
let testReportOutput = "artifacts/gauge-csharp/bin/gauge.csharp.runner.unittests.xml"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted
let gitOwner = "Update GitHome in build.fsx"
let gitHome = "https://github.com/" + gitOwner

// The name of the project on GitHub
let gitName = "gauge-csharp"

// The url for the raw files hosted
let gitRaw = environVarOrDefault "gitRaw" "https://raw.github.com/Update GitHome in build.fsx"

// Helper active pattern for project types
let (|Fsproj|Csproj|Vbproj|Shproj|) (projFileName:string) =
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | f when f.EndsWith("shproj") -> Shproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)

let artifactsDir f =
    match f with
    | path when (System.IO.Path.GetFileNameWithoutExtension path).Equals("Gauge.CSharp.Lib") -> "gauge-csharp-lib/bin"
    | path when (System.IO.Path.GetFileNameWithoutExtension path).Equals("Gauge.CSharp.Lib.UnitTests") -> "gauge-csharp-lib/tests"
    | path when (System.IO.Path.GetFileNameWithoutExtension path).Equals("Gauge.CSharp.Core") -> "gauge-csharp-core/bin"
    | path when (System.IO.Path.GetFileNameWithoutExtension path).Equals("Gauge.CSharp.Runner") -> "gauge-csharp/bin"
    | path when (System.IO.Path.GetFileNameWithoutExtension path).Equals("Gauge.CSharp.Runner.UnitTests") -> "gauge-csharp/tests"
    | path when (System.IO.Path.GetFileNameWithoutExtension path).Equals("Gauge.CSharp.Runner.IntegrationTests") -> "gauge-csharp/itests"
    | _                           -> failwith (sprintf "Unknown project %s. Where should its artifacts be copied to?" f)

// Copies binaries from default VS location to artifacts/ folder
// But keeps a subdirectory structure
// - gauge-csharp-lib  - Gauge.CSharp.Lib with referenced core
// - gauge-csharp-core - Gauge.CSharp.Core only
// - gauge-csharp      - Gauge.CSharp.Runner with referenced core and lib
Target "CopyBinaries" (fun _ ->
    !! "**/*.??proj"
    -- "**/*.shproj"
    -- "**/IntegrationTestSample.csproj"
    -- "**/Gauge.Spec.csproj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) </> "bin/Release", "artifacts" </> (artifactsDir f)))
    |>  Seq.iter (fun (fromDir, toDir) -> CopyDir toDir fromDir (fun _ -> true))
    // copy the IntegrationTestSample.dll with test suites
    CopyFile "artifacts/gauge-csharp/itests" "IntegrationTestSample/gauge-bin/IntegrationTestSample.dll"
    // and do NOT copy its old Lib reference, it must be loaded by sandbox
    // CopyFile "artifacts/gauge-csharp/itests" "IntegrationTestSample/gauge-bin/Gauge.CSharp.Lib.dll"
)

// --------------------------------------------------------------------------------------
// Generate AssemblyInfo.cs

let commonAttributes = [          
          Attribute.Configuration("")
          Attribute.Company("ThoughtWorks Inc.")
          Attribute.Product("Gauge.CSharp.Core")
          Attribute.Copyright("Copyright ©  2016")
          Attribute.Trademark("")
          Attribute.Culture("")
          Attribute.ComVisible(true) ]

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo-Core" (fun _ ->
    let coreAttributes =
        [ Attribute.Title("Gauge.CSharp.Core")
          Attribute.Guid("db098a05-ce23-4b6d-a124-4d125bc89f57")
          Attribute.Description("[INTERNAL GAUGE USE ONLY] Communicate with Gauge Core")
          Attribute.Version coreRelease.AssemblyVersion
          Attribute.FileVersion coreRelease.AssemblyVersion ] @ commonAttributes

    CreateCSharpAssemblyInfo (("Core" </> "Properties") </> "AssemblyInfo.cs") coreAttributes
)

Target "AssemblyInfo-Lib" (fun _ ->
    let libAttributes =
        [ Attribute.Title("Gauge.CSharp.Lib")
          Attribute.Guid("21677428-1b2a-4a9b-9865-f72209fb8f1c")
          Attribute.Description("C# support for Gauge. http://getgauge.io")
          Attribute.Version libRelease.AssemblyVersion
          Attribute.FileVersion libRelease.AssemblyVersion ] @ commonAttributes

    CreateCSharpAssemblyInfo (("Lib" </> "Properties") </> "AssemblyInfo.cs") libAttributes
)

Target "AssemblyInfo-Runner" (fun _ ->
    let libAttributes =
        [ Attribute.Title("Gauge.CSharp.Runner")
          Attribute.Guid("b80cc90b-dd04-445b-825e-51a42f3cadaf")
          Attribute.Description("C# spec for Gauge. http://getgauge.io")
          Attribute.Version releaseRunner.AssemblyVersion
          Attribute.FileVersion releaseRunner.AssemblyVersion ] @ commonAttributes

    CreateCSharpAssemblyInfo (("Runner" </> "Properties") </> "AssemblyInfo.cs") libAttributes
)

// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    CleanDirs ["bin"; "artifacts"; "temp"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

let buildSln solutionFile =
    !! solutionFile
    #if MONO
        |> MSBuildReleaseExt "" [ ("DefineConstants","MONO") ] "Rebuild"
    #else
        |> MSBuildRelease "" "Rebuild"
    #endif
        |> ignore

Target "Build-Lib" (fun _ ->
    buildSln "Gauge.CSharp.Lib.sln"
)

Target "Build-Core" (fun _ ->
    buildSln "Gauge.CSharp.Core.sln"
)

Target "Build-Runner" (fun _ ->
    buildSln "Gauge.CSharp.sln"
)

// --------------------------------------------------------------------------------------
// Zip distribution

Target "Skel" (fun _ ->
    CopyDir "artifacts/gauge-csharp/skel" "Gauge.Project.Skel/" (fun _ -> true)
    CopyFile "artifacts/gauge-csharp/csharp.json" "Runner/csharp.json"
)

// version from runner's changelog
let version = releaseRunner.AssemblyVersion

Target "Zip" (fun _ ->
    !! ("artifacts/gauge-csharp/**/*")
    -- ("artifacts/gauge-csharp/tests/*")
    -- ("artifacts/gauge-csharp/itests/*")
    -- ("**/*.zip")
        |> Zip "artifacts/gauge-csharp/" (sprintf @"artifacts/gauge-csharp/gauge-csharp-%s.zip" version)
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target "RunTests-Lib" (fun _ ->
    !! testAssembliesLib
    |> NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 5.
            OutputFile = "TestResults-Lib.xml" })
)

Target "RunTests-Runner" (fun _ ->
    !! testAssembliesRunner
    |> NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 5.
            OutputFile = "TestResults-Runner.xml" })
)

Target "RunITests-Runner" (fun _ ->
    !! itestAssembliesRunner
    |> NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 5.
            OutputFile = "ITestResults-Runner.xml" })
)

Target "RunTests-All" (fun _ ->
    !! testAssemblies
    |> NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 5.
            OutputFile = "ITestResults-Runner.xml" })
)

#if MONO
#else
// --------------------------------------------------------------------------------------
// SourceLink allows Source Indexing on the PDB generated by the compiler, this allows
// the ability to step through the source code of external libraries http://ctaggart.github.io/SourceLink/

Target "SourceLink" (fun _ ->
    let baseUrl = sprintf "%s/%s/{0}/%%var2%%" gitRaw project
    !! "src/**/*.??proj"
    -- "src/**/*.shproj"
    |> Seq.iter (fun projFile ->
        let proj = VsProj.LoadRelease projFile
        SourceLink.Index proj.CompilesNotLinked proj.OutputFilePdb __SOURCE_DIRECTORY__ baseUrl
    )
)

#endif

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target "NuGet-Core" (fun _ ->
    Paket.Pack(fun p ->
        { p with
            TemplateFile = "Core/paket.template"
            OutputPath = "artifacts/gauge-csharp-core"
            Version = coreRelease.NugetVersion
            ReleaseNotes = toLines coreRelease.Notes})
)

Target "NuGet-Lib" (fun _ ->
    Paket.Pack(fun p ->
        { p with
            TemplateFile = "Lib/paket.template"
            OutputPath = "artifacts/gauge-csharp-lib"
            Version = libRelease.NugetVersion
            ReleaseNotes = toLines libRelease.Notes})
)

Target "Package" DoNothing
Target "Build" DoNothing
Target "RunTests" DoNothing

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"AssemblyInfo-Core"
  ==> "Build-Core"

"AssemblyInfo-Lib"
  ==> "Build-Lib"

"AssemblyInfo-Runner"
  ==> "Build-Runner"

"Build-Lib"
  ==> "Build-Core"
  ==> "Build-Runner"
  ==> "Build"
  ==> "CopyBinaries"

"CopyBinaries"
  ==> "RunTests-Lib"
"CopyBinaries"
  ==> "RunTests-Runner"
"CopyBinaries"
  ==> "RunITests-Runner"

"RunTests-All"
  ==> "RunTests"

"Clean"
  ==> "CopyBinaries"
  ==> "RunTests"
  ==> "All"

"CopyBinaries"
  ==> "Skel"
  ==> "Zip"


"Zip"
  ==> "Package"
"NuGet-Core"
  ==> "Package"
"NuGet-Lib"
  ==> "Package"

RunTargetOrDefault "All"
