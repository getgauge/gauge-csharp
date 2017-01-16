# Copyright 2015 ThoughtWorks, Inc.

# This file is part of Gauge-CSharp.

# Gauge-CSharp is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.

# Gauge-CSharp is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.

# You should have received a copy of the GNU General Public License
# along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.
$protocEx
$grpcEx

if ((gci Env:PROCESSOR_ARCHITECTURE).value  -match 64){
    $protocEx = gci .\packages\Grpc.Tools\tools\windows_x64 -Filter protoc.exe -recurse
    $grpcEx=gci .\packages\Grpc.Tools\tools\windows_x64 -Filter grpc_csharp_plugin.exe -recurse
}
else {
    $protocEx = gci .\packages\Grpc.Tools\tools\windows_x86 -Filter protoc.exe -recurse
    $grpcEx=gci .\packages\Grpc.Tools\tools\windows_x86 -Filter grpc_csharp_plugin.exe -recurse        
}

$protoc="$($protocEx.FullName)"

$grpc="$($grpcEx.FullName)"

Write-Host "Generating Proto Classes.."

gci ".\gauge-proto" -Filter "*.proto" | %{
    Write-Host "Generating classes for $_"
    &$protoc @('-I.\gauge-proto', '--csharp_out=.\Core', '--grpc_out=.\Core', ".\gauge-proto\$_", "--plugin=protoc-gen-grpc=$grpc")
}

Write-Host "Done!"
