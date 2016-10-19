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

$protocEx=gci .\packages\Gauge.CSharp.Proto -Filter protoc.exe -recurse
$protoc="$($protocEx.FullName)"


$grpcEx=gci .\packages\ -Filter grpc_csharp_plugin.exe -recurse
$grpc="$($grpcEx.FullName[0])"

Write-Host "Generating Proto Classes.."

$args = @('-I.\gauge-proto', '--csharp_out=.\Core', '--grpc_out=.\Core', '.\gauge-proto\*.proto', "--plugin=protoc-gen-grpc=$grpc")

&$protoc $args

Write-Host "Done!"
