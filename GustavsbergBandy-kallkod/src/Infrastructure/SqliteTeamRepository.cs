using System.Runtime.InteropServices;
using System.Text.Json;
using Gustavsberg.Application;
using Gustavsberg.Domain;
namespace Gustavsberg.Infrastructure;

// Windows includes SQLite. A versioned document keeps this small local app simple.
// A single SQL statement saves the complete state atomically.
public sealed class SqliteTeamRepository : ITeamRepository, IDisposable
{
    private IntPtr database;
    public SqliteTeamRepository(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        Check(sqlite3_open(path, out database));
        Execute("PRAGMA busy_timeout=5000; PRAGMA journal_mode=WAL; CREATE TABLE IF NOT EXISTS TeamState(Id INTEGER PRIMARY KEY CHECK(Id=1), Json TEXT NOT NULL);");
    }
    public TeamState Load()
    {
        Check(sqlite3_prepare_v2(database,"SELECT Json FROM TeamState WHERE Id=1",-1,out var statement,IntPtr.Zero));
        try
        {
            var result = sqlite3_step(statement);
            if (result == 101) return new TeamState();
            if (result != 100) Check(result);
            var json = Marshal.PtrToStringUTF8(sqlite3_column_text(statement,0))!;
            return JsonSerializer.Deserialize<TeamState>(json) ?? throw new InvalidDataException("Databasen är skadad.");
        }
        finally { sqlite3_finalize(statement); }
    }
    public void Save(TeamState state)
    {
        var json = JsonSerializer.Serialize(state);
        Check(sqlite3_prepare_v2(database,"INSERT INTO TeamState(Id,Json) VALUES(1,?1) ON CONFLICT(Id) DO UPDATE SET Json=excluded.Json",-1,out var statement,IntPtr.Zero));
        try
        {
            Check(sqlite3_bind_text(statement,1,json,-1,new IntPtr(-1)));
            var result = sqlite3_step(statement);
            if (result != 101) Check(result);
        }
        finally { sqlite3_finalize(statement); }
    }
    private void Execute(string sql)
    {
        var result = sqlite3_exec(database,sql,IntPtr.Zero,IntPtr.Zero,out var error);
        if (error != IntPtr.Zero) sqlite3_free(error);
        Check(result);
    }
    private void Check(int result)
    {
        if (result != 0) throw new IOException($"SQLite ({result}): {Marshal.PtrToStringUTF8(sqlite3_errmsg(database))}");
    }
    public void Dispose() { if(database != IntPtr.Zero) { sqlite3_close(database); database = IntPtr.Zero; } }
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] private static extern int sqlite3_open([MarshalAs(UnmanagedType.LPUTF8Str)] string path,out IntPtr db);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] private static extern int sqlite3_close(IntPtr db);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] private static extern IntPtr sqlite3_errmsg(IntPtr db);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] private static extern int sqlite3_exec(IntPtr db,[MarshalAs(UnmanagedType.LPUTF8Str)] string sql,IntPtr callback,IntPtr argument,out IntPtr error);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] private static extern void sqlite3_free(IntPtr value);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] private static extern int sqlite3_prepare_v2(IntPtr db,[MarshalAs(UnmanagedType.LPUTF8Str)] string sql,int length,out IntPtr statement,IntPtr tail);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] private static extern int sqlite3_bind_text(IntPtr statement,int index,[MarshalAs(UnmanagedType.LPUTF8Str)] string value,int length,IntPtr destructor);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] private static extern int sqlite3_step(IntPtr statement);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] private static extern IntPtr sqlite3_column_text(IntPtr statement,int index);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] private static extern int sqlite3_finalize(IntPtr statement);
}
