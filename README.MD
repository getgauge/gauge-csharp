# Gauge-csharp

|Windows|Linux / OS X (Mono)|
|-------|------------|
|[![Build status](https://ci.appveyor.com/api/projects/status/0ydasrwa2t2jh6fy?svg=true)](https://ci.appveyor.com/project/getgauge/gauge-csharp)|[![Build Status](https://travis-ci.org/getgauge/gauge-csharp.svg?branch=master)](https://travis-ci.org/getgauge/gauge-csharp)|

 [ ![Download Nightly](https://api.bintray.com/packages/gauge/gauge-csharp/Nightly/images/download.svg) ](https://bintray.com/gauge/gauge-csharp/Nightly/_latestVersion)


This project adds C# [language plugin](https://docs.gauge.org/plugins.html#language-reporting-plugins) for [gauge](http://getgauge.io).

The plugin is authored in [C#](https://en.wikipedia.org/wiki/C_Sharp_(programming_language)).

## Getting started

### Pre-requisite

- [Install Gauge](https://docs.gauge.org/installing.html#installation)

### Installation

```
gauge install csharp
```

### Create a gauge-csharp project
```
gauge init csharp
```

### Run tests
```
gauge run specs
```

### Alternate Installation options

#### Install specific version
* Installing specific version
```
gauge install csharp --version 0.10.1
```

### Offline installation
* Download the plugin from [Releases](https://github.com/getgauge/gauge-csharp/releases)
```
gauge install csharp --file gauge-csharp-0.10.1.zip
```

#### Nightly installation

To install csharp nightly, download the latest nightly from [here](https://bintray.com/gauge/gauge-csharp/Nightly).

Once you have the downloaded nightly gauge-csharp-version.nightly-yyyy-mm-dd.zip, install using:

    gauge install csharp -f gauge-csharp-version.nightly-yyyy-mm-dd.zip

#### Build from Source

##### Development Environment Setup

###### Windows

- Use [BoxStarter](http://boxstarter.org/) to setup Windows box.
  - Install Windows
  - Hit http://bit.ly/20pMrXY from Internet Explorer/Edge.
  - Install Visual Studio and Resharper separately (You could consider installing from an ISO).

###### Linux

For building and unit testing you only need mono.

##### Terminologies
###### Gauge Lib
`Lib` is a dll that brings in Gauge's data types to C#. It is hosted in [Nuget](https://www.nuget.org/packages/Gauge.CSharp.Lib/).

Read more about `Lib` [here](https://github.com/getgauge/gauge-csharp/tree/master/Lib).

###### Gauge Core
`Core` is a dll that brings in Gauge's API and connection to C#, and also holds the protobuf communication code. It is hosted in [Nuget](https://www.nuget.org/packages/Gauge.CSharp.Core/), and is not listed in the search results. This package is used by `Lib` and `Runner` only.

Read more about `Core` [here](https://github.com/getgauge/gauge-csharp/tree/master/Core).

###### Gauge CSharp Runner
`Runner` is an executable that is invoked by Gauge Core. The `Runner` acts a bridge between C# test code and Gauge's API.

Read more about `Runner` [here](https://github.com/getgauge/gauge-csharp/tree/master/Runner).

###### Gauge-Proto
This is a submodule of the repository `https://github.com/getgauge/gauge-proto`. This repository holds the `.proto` files that act as contracts between Gauge and the plugins. This submodule needs to be fetched to generate the protobuf classes.

##### Setup

###### Lib
The Lib is a reference that you can add to your test project, as you would do with any Nuget package.

You will need Gauge and Gauge-csharp plugin installed before installing `Lib`.

###### Build

On mono:
```
./build.sh CopyBinaries
```

On Windows
```
build.cmd CopyBinaries
```

####### Regenerate the API messages (protocol buffer api changes)

Update the submodule and run the `gen-proto` command:

    git submodule update
    run.bat gen-proto

###### Tests

####### Unit/Integration

On mono:
```
./build.sh RunTests
```

On Windows
```
build.cmd RunTests
```

####### Functional Tests

For running functional tests
 * gauge
 * java 1.8
 * maven

Choose from the below targets (whichever is applicable)

|Target|Purpose|
|------|-------|
|`FunctionalTests`| Runs all functional tests without `unimplemented` tag|
|`FunctionalTestsP`| Same as `FunctionalTests` but runs in parallel|
|`FunctionalTestsUnimplemented`| Runs all functional tests with `unimplemented` tag, ideally these should fail|
|`FunctionalTestsPUnimplemented`| Same as `FunctionalTestsUnimplemented` but runs in parallel|
|`BuildInstallFT`| Builds, runs Unit and Integration tests, installs Gauge-CSharp from artifact, and triggers `FunctionalTestsP`|

On mono:
```
./build.sh <target>
```

On Windows:
```
build.cmd <target>
```

###### Package

On mono:
```
./build.sh Package
```

On Windows:
```
build.cmd Package
```

All artifacts are genereated in `.\artifacts` folder.

 * The plugin is zipped to `artifacts/gauge-csharp/gauge-csharp-<runner-version>.zip`,
 where `<runner-version>` is from top of `CHANGELOG.md` file.
 * The Lib Nuget package is put at `artifacts/gauge-csharp-lib/Gauge.CSharp.Lib.<lib-version>.nupkg`,
 where `<lib-version>` is from top of `Lib/CHANGELOG.md` file.
 * The Core Nuget package is put at `artifacts/gauge-csharp-core/Gauge.CSharp.Core.<core-version>.nupkg`,
 where `<core-version>` is from top of `Core/CHANGELOG.md` file.

###### Zip

Zip distribution in `artifacts/gauge-csharp/gauge-csharp-<runner-version>.zip` can be created with
On mono:
```
./build.sh Zip
```

On Windows
```
build.cmd Zip
```

###### Install the plugin

To install a local version of the plugin, choose from the below targets (whichever is applicable)

|Target|Purpose|
|------|-------|
|`Install`| Installs Runner from generated Artifact|
|`ForceInstall`| Same as `Install`, but removes conflicting version if already installed|


On mono:
```
./build.sh <target>
```

On Windows:
```
build.cmd <target>
```

## License

![GNU Public License version 3.0](http://www.gnu.org/graphics/gplv3-127x51.png)
Gauge-csharp is released under [GNU Public License version 3.0](http://www.gnu.org/licenses/gpl-3.0.txt)

## Copyright

Copyright 2015 ThoughtWorks, Inc.
