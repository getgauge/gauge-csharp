# Gauge CSharp Runner

`Runner` is an executable that is invoked by Gauge Core. The `Runner` acts a bridge between C# test code and Gauge's API.

## License
This project is released under [GPLv3](http://www.gnu.org/licenses/gpl-3.0.txt). 

This project uses [protobuf-csharp-port](https://code.google.com/p/protobuf-csharp-port/) which is released under [New BSD License](http://opensource.org/licenses/BSD-3-Clause). These licenses is included in this repository [here](https://github.com/getgauge/gauge/Lib/licenses) and [here](https://github.com/getgauge/gauge/Runner/licenses), for `Lib` and `Runner` respectively.

## About
`Runner` serves the below purposes

- Scan assemblies referencing `Gauge.CSharp.Lib.dll` to get list of `Step` methods.
- Holds definition of message processors for various messages passed by Gauge.
- Parse messages given by Gauge Core and invoke the respective processor.
- Execute methods in the target test project when respective step is processed.
- Execute hooks at various stages.
- initialize a new Gauge project (Copy skeleton files from [here](https://github.com/getgauge/gauge-csharp/Gauge.Project.Skel/)). Install dependencies via Nuget.

## Developer notes

### Two phases of execution

#### Setup
This phase is invoked when gauge is invoked with a `--init` flag. The purpose of this phase is to copy over skeleton files to the target directory and install the dependencies.

The convention is that the ProjectName and Default Namespace is the same as the Target folder name.

#### Start
This phase is invoked when gauge is executing specs. Gauge launches the runner as a child process and then orchestrates the execution via messages that the Runner processes.

## Sponsors

This project is sponsored by [Thoughtworks Inc.](http://www.thoughtworks.com/).