#!/usr/bin/env powershell
$ErrorActionPreference = 'Stop'

function exec($_cmd) {
    write-host " > $_cmd $args" -ForegroundColor cyan
    & $_cmd @args
    if ($LASTEXITCODE -ne 0) {
        throw 'Command failed'
    }
}

Remove-Item artifacts/ -Recurse -ErrorAction Ignore
Remove-Item Source/RagelTask/obj/ -Recurse -ErrorAction Ignore

exec dotnet restore ./Source/RagelTask/
exec dotnet pack -c Release ./Source/RagelTask/
