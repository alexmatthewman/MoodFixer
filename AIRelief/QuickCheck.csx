#r "nuget: Microsoft.Data.Sqlite, 9.0.0"
using Microsoft.Data.Sqlite;

var db = @"C:\Users\x\source\repos\alexmatthewman\AIRelief\AIRelief\aireliefdb.db";
var conn = new SqliteConnection($"Data Source={db}");
conn.Open();

// Tables
var c = conn.CreateCommand();
c.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
var r = c.ExecuteReader();
var tables = new List<string>();
while (r.Read()) tables.Add(r.GetString(0));
r.Close();
Console.WriteLine("Tables: " + string.Join(", ", tables));

// Row counts for key tables
foreach (var t in new[]{"Users","Groups","Questions","UserStatistics","__EFMigrationsHistory"})
{
    if (!tables.Contains(t)) { Console.WriteLine($"  MISSING: {t}"); continue; }
    var c2 = conn.CreateCommand();
    c2.CommandText = $"SELECT COUNT(*) FROM \"{t}\";";
    Console.WriteLine($"  {t}: {c2.ExecuteScalar()} rows");
}

conn.Close();
