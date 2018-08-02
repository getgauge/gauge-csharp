#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.DotNet.Testing
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.Runtime
open Fake.Tools.Git
// Read additional information from the release notes document
let releaseRunner = ReleaseNotes.load "CHANGELOG.md"

// Pattern specifying assemblies to be tested using NUnit
let testAssembliesRunner = "artifacts/gauge-csharp/tests/*Test*.dll"
let itestAssembliesRunner = "artifacts/gauge-csharp/itests/*IntegrationTest*.dll"

let artifactsDir f =
    match f with
    | path when (System.IO.Path.GetFileNameWithoutExtension path).Equals("Gauge.CSharp.Runner") -> "gauge-csharp/bin"
    | path when (System.IO.Path.GetFileNameWithoutExtension path).Equals("Gauge.CSharp.Runner.UnitTests") -> "gauge-csharp/tests"
    | path when (System.IO.Path.GetFileNameWithoutExtension path).Equals("Gauge.CSharp.Runner.IntegrationTests") -> "gauge-csharp/itests"
    | _                           -> failwith (sprintf "Unknown project %s. Where should its artifacts be copied to?" f)

let Run = fun (command, args, wd) ->
    Trace.trace (sprintf "Running %s %s in WD: %s" command args wd)

    let result = Process.execSimple (fun info ->
        { info with 
            FileName = command
            WorkingDirectory = wd
            Arguments = args}) (System.TimeSpan.FromMinutes 30.0)
    if result <> 0 then failwithf "%s %s exited with error %d" command args result

let InvokeMvn = fun (args) ->
    if Environment.isWindows then
        Run("mvn.cmd", args, "gauge-tests")
    else
        Run("mvn", args, "gauge-tests")

// Copies binaries from default VS location to artifacts/ folder
// But keeps a subdirectory structure
// - gauge-csharp      - Gauge.CSharp.Runner with referenced core and lib
Target.create "CopyBinaries" (fun _ ->
    !! "**/*.??proj"
    -- "**/IntegrationTestSample.csproj"
    -- "**/Gauge.Spec.csproj"
    -- "**/packages/**/*.csproj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) </> "bin/Release", "artifacts" </> (artifactsDir f)))
    |>  Seq.iter (fun (fromDir, toDir) -> Shell.copyDir toDir fromDir (fun _ -> true))
    // copy the IntegrationTestSample.dll with test suites
    Shell.copyFile "artifacts/gauge-csharp/itests" "IntegrationTestSample/gauge_bin/IntegrationTestSample.dll"
    // and do NOT copy its old Lib reference, it must be loaded by sandbox
    // CopyFile "artifacts/gauge-csharp/itests" "IntegrationTestSample/gauge_bin/Gauge.CSharp.Lib.dll"
)

// In CI agent, with clean workspace, we need to fetch previosuly built assemblies to gauge_bin
Target.create "ITest-Setup" (fun _ ->
    Shell.copyFile "IntegrationTestSample/gauge_bin/IntegrationTestSample.dll" "artifacts/gauge-csharp/itests/IntegrationTestSample.dll" 
    // And the old Lib
    Shell.copyFile "IntegrationTestSample/gauge_bin/Gauge.CSharp.Lib.dll" "IntegrationTestSample/Lib/Gauge.CSharp.Lib.dll"
)

// --------------------------------------------------------------------------------------
// Generate AssemblyInfo.cs

// Generate assembly info files with the right version & up-to-date information
Target.create "AssemblyInfo" (fun _ ->
    let runnerAttributes =
        [ AssemblyInfo.Title("Gauge.CSharp.Runner")
          AssemblyInfo.Guid("b80cc90b-dd04-445b-825e-51a42f3cadaf")
          AssemblyInfo.Company("ThoughtWorks Inc.")
          AssemblyInfo.Product("Gauge.CSharp.Runner")
          AssemblyInfo.Copyright("Copyright © ThoughtWorks Inc. 2016")
          AssemblyInfo.Description("C# spec for Gauge. http://getgauge.io")
          AssemblyInfo.Version releaseRunner.AssemblyVersion
          AssemblyInfo.FileVersion releaseRunner.AssemblyVersion ]

    AssemblyInfoFile.createCSharp (("Runner" </> "Properties") </> "AssemblyInfo.cs") runnerAttributes   
)

// --------------------------------------------------------------------------------------
// Clean build results

Target.create "Clean" (fun _ ->
    Shell.cleanDirs ["obj"; "bin"; "artifacts"; "temp"]
)

Target.create "Build" (fun _ ->
    "Gauge.CSharp.sln" |> MSBuild.build (fun defaults ->
    { defaults with
        Verbosity = Some Minimal
        Targets = [ "Rebuild" ]
        Properties = [ ("Configuration", "Release")] 
    }) |> ignore
)

// --------------------------------------------------------------------------------------
// Zip distribution

Target.create "Skel" (fun _ ->
    Shell.copyDir "artifacts/gauge-csharp/skel" "Gauge.Project.Skel/" (fun _ -> true)
    Shell.copyFile "artifacts/gauge-csharp/csharp.json" "Runner/csharp.json"
    Shell.copyFile "artifacts/gauge-csharp/run.sh" "Runner/run.sh"
)

// version from runner's changelog
let version = releaseRunner.AssemblyVersion

let zip = fun _ ->
    if Environment.isWindows then
        !! ("artifacts/gauge-csharp/**/*")
        -- ("artifacts/gauge-csharp/tests/**/*")
        -- ("artifacts/gauge-csharp/itests/**/*")
        -- ("**/*.zip")
        |> Zip.zip "artifacts/gauge-csharp/" (sprintf @"artifacts/gauge-csharp/gauge-csharp-%s.zip" version)
    else
        // Zip.zip messes up file permmissions in the archive. https://github.com/fsharp/FAKE/issues/2019
        Run("zip", (sprintf @"-r gauge-csharp-%s.zip . -x tests/**/* itests/**/* **/*.zip" version), "artifacts/gauge-csharp")


Target.create "Package" (zip)
Target.create "Zip" (zip)
// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target.create "Test-Unit" (fun _ ->
    !! testAssembliesRunner
    |> Testing.NUnit3.run (fun p ->
        { p with
            ShadowCopy = false
            TimeOut = System.TimeSpan.FromMinutes 5.
            ResultSpecs = ["UnitTestResults.xml"] })
)

Target.create "Test-Integration" (fun _ ->
    !! itestAssembliesRunner
    -- "**/*IntegrationTestSample.dll"
    |> Testing.NUnit3.run (fun p ->
        { p with
            ShadowCopy = false
            TimeOut = System.TimeSpan.FromMinutes 5.
            ResultSpecs = ["IntegrationTestResults.xml"] })
)

Target.create "Test" (fun _ ->
    !! "artifacts/gauge-csharp/*tests/*Test*.dll"
    -- "**/*IntegrationTestSample.dll"
    |> Testing.NUnit3.run (fun p ->
        { p with
            ShadowCopy = false
            TimeOut = System.TimeSpan.FromMinutes 5.
            ResultSpecs = ["TestResults.xml"] })
)

// --------------------------------------------------------------------------------------
// Run the unit tests WITH COVERAGE using test runner

Target.create "Coverage" (fun _ ->
    let assembliesToTest = (" ", (!!"artifacts/gauge-csharp/*tests/*Test*.dll" --"**/*IntegrationTestSample.dll")) |> System.String.Join
    let coverageDir = "artifacts/coverage"
    Directory.create coverageDir
    Testing.OpenCover.run (fun p -> 
        { p with 
            ExePath = "./packages/test/OpenCover/tools/OpenCover.Console.exe"
            TestRunnerExePath = "./packages/test/NUnit.ConsoleRunner/tools/nunit3-console.exe"
            Output = coverageDir + "/results.xml"
            Register = Testing.OpenCover.RegisterType.RegisterUser
            Filter = "+[*]* -[*.*Tests*]* -[*IntegrationTestSample*]*"
        })
        ("--noheader --shadowcopy=false --timeout=300000 --framework=net-4.5 --result=" + coverageDir + "/nunit-results.xml " + assembliesToTest)

    Trace.trace "Generate OpenCover report"
    Run("packages/test/ReportGenerator/tools/ReportGenerator.exe", (sprintf "%s/results.xml %s/html" coverageDir coverageDir), ".")
)

Target.create "Install" (fun _ ->
    Run("gauge", "install csharp -f " + (sprintf @"artifacts/gauge-csharp/gauge-csharp-%s.zip" version), ".") 
)

Target.create "Uninstall" (fun _ ->
    Run("gauge", (sprintf @"uninstall csharp --version %s" version), ".") 
)

Target.create "RemoveTests" (fun _ ->
    Shell.cleanDir "gauge-tests"
)

Target.create "FetchTests" (fun _ ->
    let branch = Environment.environVarOrDefault "GAUGE_TEST_BRANCH" "master"
    Repository.cloneSingleBranch "" "https://github.com/getgauge/gauge-tests --depth=1" branch "gauge-tests"
)

Target.create "FunctionalTests" (fun _ ->
    Run("gauge", "install", "gauge-tests") 
    InvokeMvn "test-compile gauge:execute -Denv=ci-csharp -Dtags=\"csharp\""
)

Target.create "FunctionalTestsP" (fun _ ->
    let tags = Environment.environVarOrDefault "GAUGE_TEST_TAGS" "csharp"
    Run("gauge", "install", "gauge-tests") 
    InvokeMvn (sprintf "test -Denv=ci-csharp -Dtags=\"%s\" -Dflags=\"--simple-console\"" tags)
)

Target.create "GaugePluginInstall" (fun _ ->
    Run("gauge", "install", "gauge-tests")
)

Target.create "ForceInstall" ignore
Target.create "SetupFT" ignore
Target.create "BuildAndPackage" ignore
Target.create "BuildInstallFT" ignore

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build -t <Target>' to override
Target.create "Default" ignore

"Clean"
  ==> "Build"

"AssemblyInfo"
  ==> "Build"

"Build"
  ==> "CopyBinaries"

"Coverage"
  <== ["CopyBinaries"]

"Skel"
  ==> "Package"

"Skel"
  ==> "Zip"

"CopyBinaries"
  ==> "Test"

"CopyBinaries"
  ==> "Test-Unit"

"CopyBinaries"
  ==> "Test-Integration"

"BuildAndPackage"
  <== ["Test"; "Package"]

"BuildAndPackage"
  ==> "Install"

"Uninstall"
  ==> "Install"
  ==> "ForceInstall"

"ForceInstall"
    ==> "RemoveTests"
    ==> "FetchTests"
    ==> "GaugePluginInstall"
    ==> "SetupFT"

"SetupFT"
  ==> "FunctionalTests"

"Package"
  ==> "ForceInstall"

"CopyBinaries"
  ==> "Package"
  ==> "ForceInstall"
  ==> "SetupFT"
  ==> "FunctionalTestsP"
  ==> "BuildInstallFT"

"Default"
  <== ["Clean"; "Test";]

Target.runOrDefault "Default"
