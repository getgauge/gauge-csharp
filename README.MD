# C# support for Gauge

This project adds C# support for [Gauge](https://github.com/getgauge/gauge) testing. Read more about Gauge [here](http://getgauge.io/documentation/index.html). 

You can download Gauge [here](http://getgauge.io)

This repository hosts two projects, `Lib` and `Runner`. 

## License
This project is released under [GPLv3](http://www.gnu.org/licenses/gpl-3.0.txt). 

This project uses [protobuf-csharp-port](https://code.google.com/p/protobuf-csharp-port/) which is released under [New BSD License](http://opensource.org/licenses/BSD-3-Clause). These licenses is included in this repository [here](https://github.com/getgauge/gauge/Lib/licenses) and [here](https://github.com/getgauge/gauge/Runner/licenses), for `Lib` and `Runner` respectively.

## Gauge Lib
`Lib` is a dll that brings in Gauge's data types to C#, and also holds the protobuf communication code. It is hosted in [Nuget](https://www.nuget.org/packages/Gauge.CSharp.Lib/).

Read more about `Runner` [here](https://github.com/getgauge/gauge-csharp/tree/master/Lib).

## Gauge CSharp Runner
`Runner` is an executable that is invoked by Gauge Core. The `Runner` acts a bridge between C# test code and Gauge's API.

Read more about `Runner` [here](https://github.com/getgauge/gauge-csharp/tree/master/Runner).

## Developer notes

### Install

#### Runner
Installing the runner is done via Gauge Core. This means that you need Gauge installed as a pre-requisite. You can download Gauge [here](http://getgauge.io/download.html).

Once you have Gauge installed, add the csharp plugin using:

    gauge --install csharp

#### Lib
The Lib is a reference that you can add to your test project, as you would do with any Nuget package.

You will need Gauge and Gauge-csharp plugin installed before installing `Lib`.

### Build

    run.bat build

### Test

    run.bat test

### Package

    run.bat package

All artifacts are genereated in `./artifacts` folder.

The plugin is zipped to `./artifacts/gauge-csharp-<version>.zip`
The Nuget package is put at `./artigacts/gauge-csharp-nuget/`

## Sponsors

This project is sponsored by [Thoughtworks Studios](http://www.thoughtworks.com/products).