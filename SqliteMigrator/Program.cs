using Microsoft.Data.Sqlite;
using Npgsql;
using System.Data;

var sqliteCs = "Data Source=AIRelief/aireliefdb.db";
var pgCs     = "Host=localhost;Port=5432;Database=aireliefdb;Username=airelief;Password=Ne14txxx!";

using var sqlite = new SqliteConnection(sqliteCs);
sqlite.Open();
using var pg = new NpgsqlConnection(pgCs);
pg.Open();

Console.WriteLine("SQLite  : AIRelief/aireliefdb.db");
Console.WriteLine("Postgres: " + pgCs + "\n");

long PgCount(string table) {
    using var c = pg.CreateCommand();
    c.CommandText = "SELECT COUNT(*) FROM \"" + table + "\"";
    return (long)c.ExecuteScalar()!;
}
long existing = PgCount("Groups") + PgCount("AspNetUsers") + PgCount("Users") + PgCount("Questions") + PgCount("UserStatistics");
if (existing > 0) {
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("WARNING: Postgres already has " + existing + " row(s). Duplicates will be skipped (ON CONFLICT DO NOTHING).");
    Console.ResetColor();
}

// Discover columns from SQLite PRAGMA table_info
List<string> GetCols(string t) {
    var l = new List<string>();
    using var c = sqlite.CreateCommand();
    c.CommandText = "PRAGMA table_info(\"" + t + "\")";
    using var r = c.ExecuteReader();
    while (r.Read()) l.Add(r["name"].ToString()!);
    return l;
}
// Postgres boolean columns for a table
HashSet<string> PgBool(string t) {
    var s = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    using var c = pg.CreateCommand();
    c.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_name=@t AND data_type='boolean'";
    c.Parameters.AddWithValue("@t", t);
    using var r = c.ExecuteReader();
    while (r.Read()) s.Add(r.GetString(0));
    return s;
}
// Postgres timestamp with time zone columns for a table
HashSet<string> PgTs(string t) {
    var s = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    using var c = pg.CreateCommand();
    c.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_name=@t AND data_type='timestamp with time zone'";
    c.Parameters.AddWithValue("@t", t);
    using var r = c.ExecuteReader();
    while (r.Read()) s.Add(r.GetString(0));
    return s;
}

// Generic copy: discovers all columns from SQLite, maps bool/timestamptz via Postgres schema
int GenCopy(string tbl, string ord = "rowid") {
    var cols  = GetCols(tbl);
    var bools = PgBool(tbl);
    var tstzs = PgTs(tbl);
    var qc    = cols.Select(c => "\"" + c + "\"").ToList();
    var sel   = "SELECT " + string.Join(",", qc) + " FROM \"" + tbl + "\" ORDER BY \"" + ord + "\"";
    var ins   = "INSERT INTO \"" + tbl + "\"(" + string.Join(",", qc) + ") VALUES(" +
                string.Join(",", cols.Select((_, i) => "@p" + i)) + ") ON CONFLICT DO NOTHING";
    int n = 0;
    using var sc = sqlite.CreateCommand(); sc.CommandText = sel;
    using var rd = sc.ExecuteReader();
    using var tx = pg.BeginTransaction();
    while (rd.Read()) {
        using var ic = pg.CreateCommand(); ic.Transaction = tx; ic.CommandText = ins;
        for (int i = 0; i < cols.Count; i++) {
            var col = cols[i]; var raw = rd[col]; object v;
            if (raw is DBNull) { v = DBNull.Value; }
            else if (bools.Contains(col)) { v = Convert.ToBoolean(raw); }
            else if (tstzs.Contains(col)) {
                var dt = DateTime.Parse(raw.ToString()!);
                v = dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();
            } else { v = raw; }
            ic.Parameters.AddWithValue("@p" + i, v);
        }
        ic.ExecuteNonQuery(); n++;
    }
    tx.Commit();
    Console.WriteLine("  ok  " + tbl + ": " + n + " row(s)");
    return n;
}

// Manual copy for tables needing specific type coercion (Identity tables with LockoutEnd as DateTimeOffset)
static object Coerce(object v) => v is DBNull ? DBNull.Value : v;
static DateTime? ParseDt(object v) => v is DBNull ? null : DateTime.Parse(v.ToString()!);

int CopyTable(string label, string sel, string ins, Action<IDataRecord, NpgsqlCommand> bind) {
    int n = 0;
    using var sc = sqlite.CreateCommand(); sc.CommandText = sel;
    using var reader = sc.ExecuteReader();
    using var tx = pg.BeginTransaction();
    while (reader.Read()) {
        using var ic = pg.CreateCommand(); ic.Transaction = tx; ic.CommandText = ins;
        bind(reader, ic); ic.ExecuteNonQuery(); n++;
    }
    tx.Commit();
    Console.WriteLine("  ok  " + label + ": " + n + " row(s)");
    return n;
}
void SyncSeq(string table, string col = "ID") {
    using var chk = pg.CreateCommand();
    chk.CommandText = "SELECT COUNT(*) FROM \"" + table + "\"";
    if ((long)chk.ExecuteScalar()! == 0) return;
    using var c = pg.CreateCommand();
    c.CommandText = "SELECT setval(pg_get_serial_sequence('\"" + table + "\"','" + col + "'), COALESCE((SELECT MAX(\"" + col + "\") FROM \"" + table + "\"), 1))";
    c.ExecuteNonQuery();
}

Console.WriteLine("-- Groups --");
GenCopy("Groups", "ID");
SyncSeq("Groups");

Console.WriteLine("\n-- AspNetRoles --");
CopyTable("AspNetRoles",
  "SELECT Id,Name,NormalizedName,ConcurrencyStamp FROM AspNetRoles ORDER BY Id",
  "INSERT INTO \"AspNetRoles\"(\"Id\",\"Name\",\"NormalizedName\",\"ConcurrencyStamp\") VALUES(@id,@name,@norm,@stamp) ON CONFLICT(\"Id\") DO NOTHING",
  (r,cmd) => {
    cmd.Parameters.AddWithValue("@id",    Coerce(r["Id"]));
    cmd.Parameters.AddWithValue("@name",  Coerce(r["Name"]));
    cmd.Parameters.AddWithValue("@norm",  Coerce(r["NormalizedName"]));
    cmd.Parameters.AddWithValue("@stamp", Coerce(r["ConcurrencyStamp"]));
  });

Console.WriteLine("\n-- AspNetRoleClaims --");
CopyTable("AspNetRoleClaims",
  "SELECT Id,RoleId,ClaimType,ClaimValue FROM AspNetRoleClaims ORDER BY Id",
  "INSERT INTO \"AspNetRoleClaims\"(\"Id\",\"RoleId\",\"ClaimType\",\"ClaimValue\") VALUES(@id,@role,@type,@val) ON CONFLICT(\"Id\") DO NOTHING",
  (r,cmd) => {
    cmd.Parameters.AddWithValue("@id",   Coerce(r["Id"]));
    cmd.Parameters.AddWithValue("@role", Coerce(r["RoleId"]));
    cmd.Parameters.AddWithValue("@type", Coerce(r["ClaimType"]));
    cmd.Parameters.AddWithValue("@val",  Coerce(r["ClaimValue"]));
  });

Console.WriteLine("\n-- AspNetUsers --");
CopyTable("AspNetUsers",
  "SELECT Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount FROM AspNetUsers ORDER BY Id",
  "INSERT INTO \"AspNetUsers\"(\"Id\",\"UserName\",\"NormalizedUserName\",\"Email\",\"NormalizedEmail\",\"EmailConfirmed\",\"PasswordHash\",\"SecurityStamp\",\"ConcurrencyStamp\",\"PhoneNumber\",\"PhoneNumberConfirmed\",\"TwoFactorEnabled\",\"LockoutEnd\",\"LockoutEnabled\",\"AccessFailedCount\") VALUES(@id,@un,@nun,@em,@nem,@ec,@ph,@ss,@cs,@pn,@pnc,@tfa,@le,@len,@afc) ON CONFLICT(\"Id\") DO NOTHING",
  (r,cmd) => {
    cmd.Parameters.AddWithValue("@id",  Coerce(r["Id"]));
    cmd.Parameters.AddWithValue("@un",  Coerce(r["UserName"]));
    cmd.Parameters.AddWithValue("@nun", Coerce(r["NormalizedUserName"]));
    cmd.Parameters.AddWithValue("@em",  Coerce(r["Email"]));
    cmd.Parameters.AddWithValue("@nem", Coerce(r["NormalizedEmail"]));
    cmd.Parameters.AddWithValue("@ec",  Convert.ToBoolean(r["EmailConfirmed"]));
    cmd.Parameters.AddWithValue("@ph",  Coerce(r["PasswordHash"]));
    cmd.Parameters.AddWithValue("@ss",  Coerce(r["SecurityStamp"]));
    cmd.Parameters.AddWithValue("@cs",  Coerce(r["ConcurrencyStamp"]));
    cmd.Parameters.AddWithValue("@pn",  Coerce(r["PhoneNumber"]));
    cmd.Parameters.AddWithValue("@pnc", Convert.ToBoolean(r["PhoneNumberConfirmed"]));
    cmd.Parameters.AddWithValue("@tfa", Convert.ToBoolean(r["TwoFactorEnabled"]));
    var le = r["LockoutEnd"];
    cmd.Parameters.AddWithValue("@le",  le is DBNull ? DBNull.Value : (object)DateTimeOffset.Parse(le.ToString()!));
    cmd.Parameters.AddWithValue("@len", Convert.ToBoolean(r["LockoutEnabled"]));
    cmd.Parameters.AddWithValue("@afc", Convert.ToInt32(r["AccessFailedCount"]));
  });

Console.WriteLine("\n-- AspNetUserClaims --");
CopyTable("AspNetUserClaims",
  "SELECT Id,UserId,ClaimType,ClaimValue FROM AspNetUserClaims ORDER BY Id",
  "INSERT INTO \"AspNetUserClaims\"(\"Id\",\"UserId\",\"ClaimType\",\"ClaimValue\") VALUES(@id,@uid,@type,@val) ON CONFLICT(\"Id\") DO NOTHING",
  (r,cmd) => {
    cmd.Parameters.AddWithValue("@id",   Coerce(r["Id"]));
    cmd.Parameters.AddWithValue("@uid",  Coerce(r["UserId"]));
    cmd.Parameters.AddWithValue("@type", Coerce(r["ClaimType"]));
    cmd.Parameters.AddWithValue("@val",  Coerce(r["ClaimValue"]));
  });

Console.WriteLine("\n-- AspNetUserLogins --");
CopyTable("AspNetUserLogins",
  "SELECT LoginProvider,ProviderKey,ProviderDisplayName,UserId FROM AspNetUserLogins ORDER BY LoginProvider",
  "INSERT INTO \"AspNetUserLogins\"(\"LoginProvider\",\"ProviderKey\",\"ProviderDisplayName\",\"UserId\") VALUES(@lp,@pk,@pdn,@uid) ON CONFLICT(\"LoginProvider\",\"ProviderKey\") DO NOTHING",
  (r,cmd) => {
    cmd.Parameters.AddWithValue("@lp",  Coerce(r["LoginProvider"]));
    cmd.Parameters.AddWithValue("@pk",  Coerce(r["ProviderKey"]));
    cmd.Parameters.AddWithValue("@pdn", Coerce(r["ProviderDisplayName"]));
    cmd.Parameters.AddWithValue("@uid", Coerce(r["UserId"]));
  });

Console.WriteLine("\n-- AspNetUserTokens --");
CopyTable("AspNetUserTokens",
  "SELECT UserId,LoginProvider,Name,Value FROM AspNetUserTokens ORDER BY UserId",
  "INSERT INTO \"AspNetUserTokens\"(\"UserId\",\"LoginProvider\",\"Name\",\"Value\") VALUES(@uid,@lp,@name,@val) ON CONFLICT(\"UserId\",\"LoginProvider\",\"Name\") DO NOTHING",
  (r,cmd) => {
    cmd.Parameters.AddWithValue("@uid",  Coerce(r["UserId"]));
    cmd.Parameters.AddWithValue("@lp",   Coerce(r["LoginProvider"]));
    cmd.Parameters.AddWithValue("@name", Coerce(r["Name"]));
    cmd.Parameters.AddWithValue("@val",  Coerce(r["Value"]));
  });

Console.WriteLine("\n-- AspNetUserRoles --");
CopyTable("AspNetUserRoles",
  "SELECT UserId,RoleId FROM AspNetUserRoles ORDER BY UserId",
  "INSERT INTO \"AspNetUserRoles\"(\"UserId\",\"RoleId\") VALUES(@uid,@rid) ON CONFLICT(\"UserId\",\"RoleId\") DO NOTHING",
  (r,cmd) => {
    cmd.Parameters.AddWithValue("@uid", Coerce(r["UserId"]));
    cmd.Parameters.AddWithValue("@rid", Coerce(r["RoleId"]));
  });

Console.WriteLine("\n-- Users --");
GenCopy("Users", "ID");
SyncSeq("Users");

Console.WriteLine("\n-- Questions --");
GenCopy("Questions", "ID");
SyncSeq("Questions");

Console.WriteLine("\n-- UserStatistics --");
GenCopy("UserStatistics", "ID");
SyncSeq("UserStatistics");

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\nAll tables migrated successfully.");
Console.ResetColor();
