param([string]$ProjectDir = "$PSScriptRoot")

$binDir    = Join-Path $ProjectDir "bin\Debug\net9.0"
$sqliteDll = Join-Path $binDir "Microsoft.Data.Sqlite.dll"
$coreDll   = Join-Path $binDir "SQLitePCLRaw.core.dll"
$provDll   = Join-Path $binDir "SQLitePCLRaw.provider.e_sqlite3.dll"
$dbPath    = Join-Path $ProjectDir "aireliefdb.db"
$sqlFile   = Join-Path $ProjectDir "SeedQuestions.sql"

# Load assemblies
[System.Reflection.Assembly]::LoadFile($coreDll)   | Out-Null
[System.Reflection.Assembly]::LoadFile($provDll)   | Out-Null
[System.Reflection.Assembly]::LoadFile($sqliteDll) | Out-Null

# Initialise native SQLite provider
[SQLitePCL.SQLite3Provider_e_sqlite3]::Init()

$conn = New-Object Microsoft.Data.Sqlite.SqliteConnection("Data Source=$dbPath")
$conn.Open()

# Count before
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COUNT(*) FROM Questions;"
$before = $cmd.ExecuteScalar()
Write-Host "Questions before: $before"

# Run each statement from the SQL file
$sqlText = Get-Content $sqlFile -Raw
# Split on statement boundaries (semicolons), skip blanks/comments
$statements = $sqlText -split ";" | Where-Object { ($_ -replace "--[^\n]*","").Trim() -ne "" }

foreach ($stmt in $statements) {
    $s = ($stmt -replace "--[^\n]*","").Trim()
    if ($s -eq "") { continue }
    $c = $conn.CreateCommand()
    $c.CommandText = $s
    try {
        $c.ExecuteNonQuery() | Out-Null
    } catch {
        Write-Warning "Failed: $_"
    }
}

# Count after
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = "SELECT COUNT(*) FROM Questions;"
$after = $cmd2.ExecuteScalar()
Write-Host "Questions after:  $after"
Write-Host "Rows inserted:    $($after - $before)"

# Show the questions
$cmd3 = $conn.CreateCommand()
$cmd3.CommandText = "SELECT ID, [order], maintext, CorrectAnswer FROM Questions ORDER BY [order];"
$reader = $cmd3.ExecuteReader()
Write-Host ""
Write-Host "ID  Order  CorrectAnswer          MainText"
Write-Host ("=" * 90)
while ($reader.Read()) {
    $id   = $reader.GetValue(0)
    $ord  = $reader.GetValue(1)
    $ca   = $reader.GetValue(3)
    $mt   = ($reader.GetValue(2).ToString())[0..59] -join ""
    Write-Host ("{0,-4}{1,-7}{2,-25}{3}" -f $id, $ord, $ca, $mt)
}
$reader.Close()
$conn.Close()
