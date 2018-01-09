// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.OpenCoverHelper
open Fake.Testing
open System
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

// Pattern specifying assemblies to be tested using NUnit
let testAssembliesRunner = "artifacts/gauge-csharp/tests/*Test*.dll"
let itestAssembliesRunner = "artifacts/gauge-csharp/itests/*IntegrationTest*.dll"

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

let Run = fun (command, args, wd) ->
    trace (sprintf "Running %s %s in WD: %s" command args wd)

    let result = ExecProcess (fun info ->
        info.FileName <- command
        info.WorkingDirectory <- wd
        info.Arguments <- args) (TimeSpan.FromMinutes 30.0)
    if result <> 0 then failwithf "%s %s exited with error %d" command args result

let InvokeMvn = fun (args) ->
    if isMono then
        Run("mvn", args, "gauge-tests")
    else
        Run("mvn.cmd", args, "gauge-tests")

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
    -- "**/packages/**/*.csproj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) </> "bin/Release", "artifacts" </> (artifactsDir f)))
    |>  Seq.iter (fun (fromDir, toDir) -> CopyDir toDir fromDir (fun _ -> true))
    // copy the IntegrationTestSample.dll with test suites
    CopyFile "artifacts/gauge-csharp/itests" "IntegrationTestSample/gauge-bin/IntegrationTestSample.dll"
    // and do NOT copy its old Lib reference, it must be loaded by sandbox
    // CopyFile "artifacts/gauge-csharp/itests" "IntegrationTestSample/gauge-bin/Gauge.CSharp.Lib.dll"
)

// In CI agent, with clean workspace, we need to fetch previosuly built assemblies to gauge-bin
Target "ITest-Setup" (fun _ ->
    CopyFile "IntegrationTestSample/gauge-bin/IntegrationTestSample.dll" "artifacts/gauge-csharp/itests/IntegrationTestSample.dll" 
    // And the old Lib
    CopyFile "IntegrationTestSample/gauge-bin/Gauge.CSharp.Lib.dll" "IntegrationTestSample/Lib/Gauge.CSharp.Lib.dll"
)

// --------------------------------------------------------------------------------------
// Generate AssemblyInfo.cs

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo-Runner" (fun _ ->
    let runnerAttributes =
        [ Attribute.Title("Gauge.CSharp.Runner")
          Attribute.Guid("b80cc90b-dd04-445b-825e-51a42f3cadaf")
          Attribute.Company("ThoughtWorks Inc.")
          Attribute.Product("Gauge.CSharp.Core")
          Attribute.Copyright("Copyright © ThoughtWorks Inc. 2016")
          Attribute.Description("C# spec for Gauge. http://getgauge.io")
          Attribute.Version releaseRunner.AssemblyVersion
          Attribute.FileVersion releaseRunner.AssemblyVersion ]

    CreateCSharpAssemblyInfo (("Runner" </> "Properties") </> "AssemblyInfo.cs") runnerAttributes   
)

// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    CleanDirs ["bin"; "artifacts"; "temp"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

let buildSln solutionFile =
    solutionFile
        |> build (fun defaults ->
        { defaults with
            Verbosity = Some Minimal
            Targets = [ "Rebuild" ]
            Properties = [ ("Configuration", "Release")
                        #if MONO
                            ;("DefineConstants","MONO")
                        #endif
                        ] })
        |> ignore

Target "Build-Lib" (fun _ ->
    DotNetCli.Build(fun p ->
        { p with
            Project = "Gauge.CSharp.Lib.sln"
        })
)

Target "Build-Core" (fun _ ->
    DotNetCli.Build(fun p ->
        { p with
            Project = "Gauge.CSharp.Core.sln"
        })
)

Target "Build-Runner" (fun _ ->
    buildSln "Gauge.CSharp.sln"
)

// --------------------------------------------------------------------------------------
// Zip distribution

Target "Skel" (fun _ ->
    CopyDir "artifacts/gauge-csharp/skel" "Gauge.Project.Skel/" (fun _ -> true)
    CopyFile "artifacts/gauge-csharp/csharp.json" "Runner/csharp.json"
    CopyFile "artifacts/gauge-csharp/run.sh" "Runner/run.sh"
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
    DotNetCli.Test(fun p ->
        { p with
            WorkingDir = "./Lib.UnitTests"
        })
)

Target "RunTests-Runner" (fun _ ->
    !! testAssembliesRunner
    |> NUnit3 (fun p ->
        { p with
            ShadowCopy = false
            TimeOut = TimeSpan.FromMinutes 5.
            ResultSpecs = ["TestResults-Runner.xml"] })
)

Target "RunITests-Runner" (fun _ ->
    !! itestAssembliesRunner
    |> NUnit3 (fun p ->
        { p with
            ShadowCopy = false
            TimeOut = TimeSpan.FromMinutes 5.
            ResultSpecs = ["ITestResults-Runner.xml"] })
)

Target "RunTests-All" (fun _ ->
    !! "artifacts/gauge-csharp/*tests/*Test*.dll"
    -- "**/*IntegrationTestSample.dll"
    |> NUnit3 (fun p ->
        { p with
            ShadowCopy = false
            TimeOut = TimeSpan.FromMinutes 5.
            ResultSpecs = ["TestResults.xml"] })
)

// --------------------------------------------------------------------------------------
// Run the unit tests WITH COVERAGE using test runner

Target "RunTests-Coverage" (fun _ ->
    let assembliesToTest = (" ", (!!"artifacts/gauge-csharp/*tests/*Test*.dll" --"**/*IntegrationTestSample.dll")) |> System.String.Join
    let coverageDir = "artifacts/coverage"
    CreateDir coverageDir
    OpenCover (fun p -> 
        { p with 
            ExePath = "./packages/test/OpenCover/tools/OpenCover.Console.exe"
            TestRunnerExePath = "./packages/test/NUnit.ConsoleRunner/tools/nunit3-console.exe"
            Output = coverageDir + "/results.xml"
            Register = RegisterUser
            Filter = "+[*]* -[*.*Tests*]* -[*IntegrationTestSample*]*"
        })
        ("--noheader --shadowcopy=false --timeout=300000 --framework=net-4.5 --result=" + coverageDir + "/nunit-results.xml " + assembliesToTest)

    trace "Generate OpenCover report"
    Run("packages/test/ReportGenerator/tools/ReportGenerator.exe", (sprintf "%s/results.xml %s/html" coverageDir coverageDir), ".")
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
    DotNetCli.Pack(fun p ->
        { p with
            OutputPath = "artifacts/gauge-csharp-core"
            Project = "Core/Gauge.CSharp.Core.csproj"
        })
)

Target "NuGet-Lib" (fun _ ->
    DotNetCli.Pack(fun p ->
        { p with
            OutputPath = "artifacts/gauge-csharp-lib"
            Project = "Lib/Gauge.CSharp.Lib.csproj"
        })
)

Target "Install" (fun _ ->
    Run("gauge", "install csharp -f " + (sprintf @"artifacts/gauge-csharp/gauge-csharp-%s.zip" version), ".") 
)

Target "Uninstall" (fun _ ->
    Run("gauge", (sprintf @"uninstall csharp --version %s" version), ".") 
)

Target "RemoveTests" (fun _ ->
    CleanDir "gauge-tests"
)

Target "FetchTests" (fun _ ->
    let branch = environVarOrDefault "GAUGE_TEST_BRANCH" "master"
    Repository.cloneSingleBranch "" "https://github.com/getgauge/gauge-tests --depth=1" branch "gauge-tests"
)

Target "FunctionalTests" (fun _ ->
    Run("gauge", "install", "gauge-tests") 
    InvokeMvn "test-compile gauge:execute -Denv=ci-csharp -Dtags=\"csharp\""
)

Target "FunctionalTestsP" (fun _ ->
    let tags = environVarOrDefault "GAUGE_TEST_TAGS" "csharp"
    Run("gauge", "install", "gauge-tests") 
    InvokeMvn (sprintf "test -Denv=ci-csharp -Dtags=\"%s\" -Dflags=\"--simple-console\"" tags)
)

Target "FunctionalTestsUnimplemented" (fun _ ->
    InvokeMvn "test-compile gauge:execute -Denv=ci-csharp -Dtags=\"unimplemented\""
)

Target "FunctionalTestsPUnimplemented" (fun _ ->
    InvokeMvn "test -Denv=ci-csharp -Dtags=\"unimplemented\""
)

Target "GaugePluginInstall" (fun _ ->
    Run("gauge", "install", "gauge-tests")
)

Target "ForceInstall" DoNothing
Target "Package" DoNothing
Target "Build" DoNothing
Target "RunTests" DoNothing
Target "SetupFT" DoNothing

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing
Target "BuildAndPackage" DoNothing

Target "BuildInstallFT" DoNothing

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
 
"CopyBinaries"
  ==> "RunTests-All"
  ==> "RunTests"

"CopyBinaries"
  ==> "RunTests-Coverage"

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

"RunTests-Lib"
  ==> "RunTests-All"

"RunTests-All"
  ==> "BuildAndPackage"

"Package"
  ==> "BuildAndPackage"

"Uninstall"
  ==> "ForceInstall"

"Install"
  ==> "ForceInstall"

"RemoveTests"
  ==> "SetupFT"

"FetchTests"
  ==> "SetupFT"

"GaugePluginInstall"
  ==> "SetupFT"

"SetupFT"
  ==> "FunctionalTests"

"SetupFT"
  ==> "FunctionalTestsP"

"SetupFT"
  ==> "FunctionalTestsUnimplemented"

"SetupFT"
  ==> "FunctionalTestsPUnimplemented"

"BuildAndPackage"
  ==> "BuildInstallFT"

"ForceInstall"
  ==> "BuildInstallFT"

"FunctionalTestsP"
  ==> "BuildInstallFT"

RunTargetOrDefault "All"
