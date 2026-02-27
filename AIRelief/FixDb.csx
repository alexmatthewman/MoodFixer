#r "nuget: Microsoft.Data.Sqlite, 9.0.0"
using Microsoft.Data.Sqlite;

var db   = @"C:\Users\x\source\repos\alexmatthewman\AIRelief\AIRelief\aireliefdb.db";
var conn = new SqliteConnection($"Data Source={db}");
conn.Open();

void Exec(string sql, string label) {
    var c = conn.CreateCommand(); c.CommandText = sql;
    Console.WriteLine($"  {label}: {c.ExecuteNonQuery()} affected"); }

long Scalar(string sql) {
    var c = conn.CreateCommand(); c.CommandText = sql;
    var v = c.ExecuteScalar(); return v == null || v == DBNull.Value ? 0L : (long)v; }

Console.WriteLine("=== Step 1: Create missing tables ===");

Exec(@"CREATE TABLE IF NOT EXISTS Groups (
    ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, PlanName TEXT, Name TEXT NOT NULL,
    GroupImageUrl TEXT, PrimaryThemeColor TEXT, SecondaryThemeColor TEXT,
    NumberOfUserLicenses INTEGER NOT NULL DEFAULT 0, ExpiryDays INTEGER NOT NULL DEFAULT 0,
    QueryTimeToCompleteDays INTEGER NOT NULL DEFAULT 0, QueryFrequency INTEGER NOT NULL DEFAULT 0,
    QueryPassingGrade INTEGER NOT NULL DEFAULT 0, QueryQuestionsRandom INTEGER NOT NULL DEFAULT 0,
    QueryQuestionsFocussed INTEGER NOT NULL DEFAULT 0, CreatedDate TEXT NOT NULL, LastModifiedDate TEXT
);", "Groups");

Exec(@"CREATE TABLE IF NOT EXISTS Users (
    ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Email TEXT NOT NULL, Name TEXT NOT NULL,
    AuthLevel INTEGER NOT NULL DEFAULT 0, GroupId INTEGER, ExpiryDate TEXT,
    CreatedDate TEXT NOT NULL, LastModifiedDate TEXT,
    CONSTRAINT FK_Users_Groups_GroupId FOREIGN KEY (GroupId) REFERENCES Groups (ID) ON DELETE SET NULL
);", "Users");
Exec(@"CREATE INDEX IF NOT EXISTS IX_Users_GroupId ON Users (GroupId);", "Users_idx");

Exec(@"CREATE TABLE IF NOT EXISTS UserStatistics (
    ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, UserId INTEGER NOT NULL, LastUpdated TEXT NOT NULL,
    CognitiveReflectionAttempts INTEGER NOT NULL DEFAULT 0, CognitiveReflectionPassed INTEGER NOT NULL DEFAULT 0,
    CognitiveReflectionScore INTEGER NOT NULL DEFAULT 0, CognitiveReflectionWeightedAverage TEXT NOT NULL DEFAULT '0',
    ReadingComprehensionAttempts INTEGER NOT NULL DEFAULT 0, ReadingComprehensionPassed INTEGER NOT NULL DEFAULT 0,
    ReadingComprehensionScore INTEGER NOT NULL DEFAULT 0, ReadingComprehensionWeightedAverage TEXT NOT NULL DEFAULT '0',
    CausalReasoningAttempts INTEGER NOT NULL DEFAULT 0, CausalReasoningPassed INTEGER NOT NULL DEFAULT 0,
    CausalReasoningScore INTEGER NOT NULL DEFAULT 0, CausalReasoningWeightedAverage TEXT NOT NULL DEFAULT '0',
    MetacognitionAttempts INTEGER NOT NULL DEFAULT 0, MetacognitionPassed INTEGER NOT NULL DEFAULT 0,
    MetacognitionScore INTEGER NOT NULL DEFAULT 0, MetacognitionWeightedAverage TEXT NOT NULL DEFAULT '0',
    ShortTermMemoryAttempts INTEGER NOT NULL DEFAULT 0, ShortTermMemoryPassed INTEGER NOT NULL DEFAULT 0,
    ShortTermMemoryScore INTEGER NOT NULL DEFAULT 0, ShortTermMemoryWeightedAverage TEXT NOT NULL DEFAULT '0',
    ConfidenceCalibrationAttempts INTEGER NOT NULL DEFAULT 0, ConfidenceCalibrationPassed INTEGER NOT NULL DEFAULT 0,
    ConfidenceCalibrationScore INTEGER NOT NULL DEFAULT 0, ConfidenceCalibrationWeightedAverage TEXT NOT NULL DEFAULT '0',
    OverallWeightedAverage TEXT NOT NULL DEFAULT '0',
    CONSTRAINT FK_UserStatistics_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (ID) ON DELETE CASCADE
);", "UserStatistics");
Exec(@"CREATE UNIQUE INDEX IF NOT EXISTS IX_UserStatistics_UserId ON UserStatistics (UserId);", "UserStatistics_idx");

Console.WriteLine("\n=== Step 2: Stamp EF migrations ===");
var migrations = new[]{("20260217235446_InitialCreate","9.0.8"),("20260227003534_RemoveIdentityUserIdFromUser","9.0.8")};
foreach (var (id,ver) in migrations) {
    if (Scalar($"SELECT COUNT(*) FROM __EFMigrationsHistory WHERE MigrationId='{id}';") > 0)
        Console.WriteLine($"  Already present: {id}");
    else
        Exec($"INSERT INTO __EFMigrationsHistory (MigrationId,ProductVersion) VALUES ('{id}','{ver}');", $"Stamped {id}"); }

Console.WriteLine("\n=== Step 3: Seed questions ===");
long qCount = Scalar("SELECT COUNT(*) FROM Questions;");
if (qCount == 0) {
    Exec("INSERT INTO Questions ([order],maintext,image,correctiontext,correctionimage,Option1,Option2,Option3,Option4,CorrectAnswer) VALUES (1,'A pencil costs 50 cents more than an eraser. The total cost of both is $1. How much does the eraser cost?','q1.png','At first glance, you might think the eraser costs $0.50 since the pencil costs 50 cents more. But, if the pencil costs $0.75 and the eraser costs $0.25, together they add up to $1.00.','q1x.png','$0.50','$0.25','$0.75','$0.05','$0.25');","Q1");
    Exec("INSERT INTO Questions ([order],maintext,image,backvalue,nextvalue,correctiontext,correctionimage,Option1,Option2,Option3,Option4,CorrectAnswer) VALUES (2,'In a race, you pass the person in second place. What place are you in now?','q2.png','1','3','It''s easy to think you''re now in 1st place, but if you pass the person in second place, you''re now in 2nd, not 1st.','q2x.png','2nd','1st','3rd','4th','2nd');","Q2");
    Exec("INSERT INTO Questions ([order],maintext,backvalue,nextvalue,correctiontext,correctionimage,Option1,Option2,Option3,Option4,CorrectAnswer) VALUES (3,'If a train leaves New York at 10:00 AM and travels at 60 mph, and another train leaves the same station at 90 mph at the same time, how long until the second catches up?','2','4','This is a trick question — both trains leave from the same station at the same time, so the second train is already ahead and will never need to catch up.','q3x.png','1 hour','30 minutes','They will never meet','2 hours','They will never meet');","Q3");
    Exec("INSERT INTO Questions ([order],maintext,image,backvalue,nextvalue,correctiontext,Option1,Option2,Option3,Option4,CorrectAnswer) VALUES (4,'A car travels 30 miles in 30 minutes. What is the average speed of the car?','q4.png','3','5','The car travels 30 miles in 30 minutes (0.5 hours). Speed = 30 / 0.5 = 60 mph.','60 miles per hour','30 miles per hour','15 miles per hour','2 miles per minute','60 miles per hour');","Q4");
} else { Console.WriteLine($"  Questions already seeded ({qCount} rows) — skipped"); }

Console.WriteLine("\n=== Final state ===");
foreach (var t in new[]{"Groups","Users","Questions","UserStatistics","__EFMigrationsHistory"})
    Console.WriteLine($"  {t}: {Scalar($"SELECT COUNT(*) FROM \"{t}\";")} rows");

conn.Close();
Console.WriteLine("\nDone.");
