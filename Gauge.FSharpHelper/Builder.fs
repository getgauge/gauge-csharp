module Gauge.FSharpHelper.Builder

open Fake

let buildProject projectPath gaugeBinDir =
    let setParams defaults =
            { defaults with
                Verbosity = Some(Quiet)
                Targets = ["Build"]
                Properties =
                    [
                        "Configuration", "Release"
                        "Platform", "Any CPU"
                        "OutputPath", gaugeBinDir
                    ]
             }
    build setParams projectPath