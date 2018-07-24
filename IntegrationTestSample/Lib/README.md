This directory is a placeholder.

The purpose of this checked in dll `Gauge.CSharp.Lib.dll` is just one -
It serves as input for a test that ensures that `Gauge.CSharp.Runner` works
when using a Lib version that is different from the Lib that the user's Gauge Csharp uses.
If you notice the version of the checked-in Gauge.CSharp.Lib.dll, it would be `0.7.999`.

If the IntegrationTestSample's `Gauge.CSharp.Lib` needs update that has to be done manually
by building the lib with version `0.7.999`.

The version is `0.7.999` is because the runner has a constraint of minimum lib version as `0.7.*`.
If runner upgrades the minum required version, this special lib version has to be updated accordinglly.