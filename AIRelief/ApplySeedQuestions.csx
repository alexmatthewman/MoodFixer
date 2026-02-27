#r "nuget: Microsoft.Data.Sqlite, 9.0.0"
using Microsoft.Data.Sqlite;
using System.IO;

var dbPath  = @"C:\Users\x\source\repos\alexmatthewman\AIRelief\AIRelief\aireliefdb.db";
var sqlFile = @"C:\Users\x\source\repos\alexmatthewman\AIRelief\AIRelief\SeedQuestions.sql";

var connection = new SqliteConnection($"Data Source={dbPath}");
connection.Open();

// Count before
var cmdBefore = connection.CreateCommand();
cmdBefore.CommandText = "SELECT COUNT(*) FROM Questions;";
Console.WriteLine($"Questions before: {cmdBefore.ExecuteScalar()}");

// Split and run each INSERT statement
var sql = File.ReadAllText(sqlFile);
var statements = sql.Split(';');
int executed = 0;
foreach (var raw in statements)
{
    // Strip single-line comments and whitespace
    var lines = raw.Split('\n')
                   .Select(l => System.Text.RegularExpressions.Regex.Replace(l, @"--.*", "").Trim())
                   .Where(l => l.Length > 0);
    var stmt = string.Join(" ", lines).Trim();
    if (stmt.Length == 0) continue;

    var cmd = connection.CreateCommand();
    cmd.CommandText = stmt;
    try
    {
        int rows = cmd.ExecuteNonQuery();
        Console.WriteLine($"  Statement {++executed}: {rows} row(s) affected.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ERROR on statement {++executed}: {ex.Message}");
    }
}

// Count after
var cmdAfter = connection.CreateCommand();
cmdAfter.CommandText = "SELECT COUNT(*) FROM Questions;";
Console.WriteLine($"Questions after:  {cmdAfter.ExecuteScalar()}");

// Show final list
Console.WriteLine();
Console.WriteLine($"{"ID",-4} {"Category",-25} {"CorrectAnswer",-25} {"MainText (first 60 chars)"}");
Console.WriteLine(new string('=', 115));
var cmdList = connection.CreateCommand();
cmdList.CommandText = "SELECT ID, Category, maintext, CorrectAnswer FROM Questions ORDER BY ID;";
var reader = cmdList.ExecuteReader();
while (reader.Read())
{
    var id  = reader.GetValue(0);
    var cat = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString();
    var mt  = reader.GetString(2);
    var ca  = reader.GetString(3);
    Console.WriteLine($"{id,-4} {cat,-25} {ca,-25} {(mt.Length > 60 ? mt[..60] : mt)}");
}
reader.Close();
connection.Close();
