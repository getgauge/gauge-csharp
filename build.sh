#!/usr/bin/env bash

set -eu
set -o pipefail

PAKET_BOOTSTRAPPER_EXE=.paket/paket.bootstrapper.exe
PAKET_EXE=.paket/paket.exe
OS=${OS:-"unknown"}

function run() {
  if [[ "$OS" != "Windows_NT" ]]
  then
    mono "$@"
  else
    "$@"
  fi
}


run $PAKET_BOOTSTRAPPER_EXE
run $PAKET_EXE restore

if ! [ -e $HOME/.dotnet/tools/fake ] 
then
  dotnet tool install fake-cli -g --version 5.*
fi

fake -s run build.fsx "$@"