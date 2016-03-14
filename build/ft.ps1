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

# Build, Test & Package everything
& "$(Split-Path $MyInvocation.MyCommand.Path)\package.ps1"

$env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine")
$gauge="$($env:ProgramFiles)\gauge\bin\gauge.exe"
&$gauge --install xml-report
&$gauge --install html-report
&$gauge --install java

& "$(Split-Path $MyInvocation.MyCommand.Path)\install.ps1" -force $true -gauge $gauge

if(Test-Path .\gauge-tests)
{
    Remove-Item -force -recurse .\gauge-tests | Out-Null
}
git clone --branch=0.3.2 --depth=1 https://github.com/getgauge/gauge-tests | out-null

cd .\gauge-tests
if ($env:GAUGE_PARALLEL -ne "false") {
    &$gauge --env=ci-csharp specs
}else {
    &$gauge --env=ci-csharp -p specs
}
cd ..