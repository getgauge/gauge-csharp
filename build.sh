#!/usr/bin/env bash

set -eu
set -o pipefail

TOOL_PATH=.fake
PAKET_BOOTSTRAPPER_EXE=.paket/paket.bootstrapper.exe
PAKET_EXE=.paket/paket.exe
FAKE_EXE=packages/build/FAKE/tools/FAKE.exe

$PAKET_BOOTSTRAPPER_EXE
$PAKET_EXE restore

if ! [ -e $TOOL_PATH/fake ] 
then
  dotnet tool install fake-cli --tool-path $TOOL_PATH --version 5.*
fi

$TOOL_PATH/fake -s run build.fsx "$@"