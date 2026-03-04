$dest = "$PSScriptRoot\Program.cs"
$src  = "$PSScriptRoot\Program.cs.txt"
[System.IO.File]::WriteAllText($dest, [System.IO.File]::ReadAllText($src))
Write-Host "Wrote $((Get-Content $dest).Count) lines to Program.cs"
