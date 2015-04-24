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

& "$(Split-Path $MyInvocation.MyCommand.Path)\build.ps1" -buildWithTest $true

$nunit = "$($pwd)\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe"

if(!(Test-Path $nunit))
{
    throw "Nunit runner not found in $pwd"
}
&$nunit "$($pwd)\artifacts\gauge-csharp\bin\Gauge.CSharp.Runner.UnitTests.dll" /xml:"$($pwd)\artifacts\gauge-csharp\bin\gauge.csharp.runner.unittests.xml"

# Hack to break on exit code. Powershell does not seem to propogate the exit code from test failures.
if($LastExitCode -ne 0)
{
    throw "Test execution failed."
}
