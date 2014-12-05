# Gauge CSharp Lib

`Lib` is a dll that brings in Gauge's data types to C#, and also holds the protobuf communication code. It is hosted in [Nuget](https://www.nuget.org/packages/Gauge.CSharp.Lib/).

You will need Gauge and Gauge-csharp plugin installed before installing `Lib`.

## License
This project is released under [GPLv3](http://www.gnu.org/licenses/gpl-3.0.txt). 

This project uses [protobuf-csharp-port](https://code.google.com/p/protobuf-csharp-port/) which is released under [New BSD License](http://opensource.org/licenses/BSD-3-Clause). These licenses is included in this repository [here](https://github.com/getgauge/gauge/Lib/licenses) and [here](https://github.com/getgauge/gauge/Runner/licenses), for `Lib` and `Runner` respectively.

## About
`Lib` serves two primary purposes:

- Holds definition for Datatypes and Attributes.
- Holds the definition for connecting to Gauge and exchange messages via ProtoBuf over tcp connection.

## Developer notes

### Two ways of installing `Lib`

#### From within Visual Studio
When you install `Gauge.CSharp.Lib` via Nuget Package manager, the `install` hook is triggered and `install.ps1` is triggered.

#### From CommandLine
When you initialize a new gauge project from console, i.e. `gauge --init csharp`, the Runner invokes a nuget install, which installs `Gauge.CSharp.Lib`. However nuget commandline **does not** trigger the `install` hook. 

## Sponsors

This project is sponsored by [Thoughtworks Studios](http://www.thoughtworks.com/products).