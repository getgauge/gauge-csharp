# Gauge CSharp Lib

`Lib` is a dll that brings in Gauge's data types to C#. It is hosted in [Nuget](https://www.nuget.org/packages/Gauge.CSharp.Lib/).

You will need Gauge and Gauge-csharp plugin installed before installing `Lib`.

## Build

From the root of this repository, run (`build.sh` on MONO `build.cmd` on Windows)
 * `./build.sh Build-Lib`
 * `./build.sh RunTests-Lib`, runs nunit tests (output in `TestResults-Lib.xml`)
 * TODO package and push nuget

Use `--single-target` to skip dependencies of build target. E.g. `./build.sh RunTests-Lib --single-target`
will only run tests, expecting the assemblies to be already compiled.

## License
This project is released under [GPLv3](http://www.gnu.org/licenses/gpl-3.0.txt).

This project uses [protobuf-csharp-port](https://code.google.com/p/protobuf-csharp-port/) which is released under [New BSD License](http://opensource.org/licenses/BSD-3-Clause). These licenses is included in this repository [here](https://github.com/getgauge/gauge/Lib/licenses) and [here](https://github.com/getgauge/gauge/Runner/licenses), for `Lib` and `Runner` respectively.

## About
`Lib` holds definition for Datatypes and Attributes.

## Sponsors

This project is sponsored by [Thoughtworks Inc.](http://www.thoughtworks.com/).
