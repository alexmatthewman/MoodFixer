# Clear EF Core Migration Lock from SQLite database
$dbPath = "C:\Users\x\source\repos\alexmatthewman\AIRelief\AIRelief\aireliefdb.db"

Write-Host "Clearing migration lock from $dbPath..."

# Load SQLite assembly
Add-Type -Path "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\9.0.1\Microsoft.Data.Sqlite.dll" -ErrorAction SilentlyContinue

try {
    $connectionString = "Data Source=$dbPath"
    $connection = New-Object Microsoft.Data.Sqlite.SqliteConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = "DELETE FROM __EFMigrationsLock"
    $rowsAffected = $command.ExecuteNonQuery()
    
    Write-Host "Deleted $rowsAffected lock row(s)"
    
    $connection.Close()
    Write-Host "Migration lock cleared successfully"
}
catch {
    Write-Host "Error: $_"
    Write-Host "Trying alternative method..."
    
    # Alternative: use dotnet ef directly with force flag
    Push-Location "C:\Users\x\source\repos\alexmatthewman\AIRelief\AIRelief"
    dotnet ef database drop --force --no-build
    dotnet ef database update
    Pop-Location
}
