# Bee.SQL

A lightweight static wrapper over System.Data.SqlClient for SQL Server. One call — one result object, no exceptions thrown.

## Requirements

- .NET Framework 4.0+
- SQL Server (System.Data.SqlClient from the .NET Framework, no extra packages)

## Setup

```csharp
using Bee.SQL;

SQL.connectionString = "Server=127.0.0.1;Database=Company;User Id=db_user;Password=db_password;Connection Timeout=15";
SQL.commandTimeout = 30; // seconds per query, 0 = no limit (default 30)
```

`Connection Timeout` in the connection string limits connecting to the server; `commandTimeout` limits running a query. A query that runs longer returns `execute = false` with "Execution Timeout Expired", and a list of queries is rolled back.

## Methods

| Method | Purpose | Main result |
|---|---|---|
| `select` | All rows | `data` — `List<Dictionary<string, object>>` |
| `selectString` | All rows, values as strings | `data` — `List<Dictionary<string, string>>` |
| `selectRow` | First row | `data`, `read` |
| `selectRowString` | First row, values as strings | `data`, `read` |
| `selectValue` | First value | `value` (`object`), `read` |
| `selectValueString` | First value as string | `value` (`string`), `read` |
| `selectValueInteger` | First value as int | `value` (`int?`), `read` |
| `selectValueDouble` | First value as double | `value` (`double?`), `read` |
| `insert` | INSERT | `insertedId`, `insertedIds`, `duplicate` |
| `update` | UPDATE | `affectedRowCount`, `exception` |
| `delete` | DELETE | `affectedRowCount`, `exception` |
| `query` | Any SQL | `data`, `affectedRowCount`, `insertedId` / `insertedIds`, `duplicate` |
| `execute` | Stored procedure | `data` |

Every result has `execute` (success), `message` and `stackTrace`.
`read` is `false` when the query returned no rows. For a NULL value `selectValue` and `selectValueString` return `value = null, read = true`, while `selectValueInteger` and `selectValueDouble` return `value = null, read = false`.
`duplicate` is `true` on a UNIQUE or PRIMARY KEY violation. `insert` and `query` also return `exceptionNumber` — the SQL Server error number (for example `2627`).

`update` and `delete` return `execute = false` when no rows match. Use `exception` to tell the cases apart:

| Case | `execute` | `exception` | `message` |
|---|---|---|---|
| Rows changed | `true` | `false` | Request completed successfully |
| No rows matched | `false` | `false` | No rows were changed |
| SQL error | `false` | `true` | Request failed. … |

Methods that take a list of queries (`insert`, `update`, `query`) run them in one transaction. Parameters are passed in the same order; an item may be `null`. All queries are rolled back if:

- a query fails, or
- an INSERT, UPDATE, DELETE or MERGE changes no rows.

In both cases `execute` is `false`, and `queryText` shows the query that failed or changed nothing (`insert` and `update` also return its `parameter`).

```csharp
var u = SQL.update(queries, parameters);

if (!u.execute)
    Console.WriteLine(u.message + ": " + u.queryText);
```

## Examples

### Select

```csharp
var users = SQL.select("select id, name from users where age > @age",
    new Dictionary<string, object> { { "@age", 18 } });

if (users.execute)
{
    foreach (var row in users.data)
        Console.WriteLine(row["id"] + " " + row["name"]);
}
else
{
    Console.WriteLine(users.message);
}
```

### Single row and value

```csharp
var user = SQL.selectRow("select * from users where id = @id",
    new Dictionary<string, object> { { "@id", 1 } });

if (user.read)
    Console.WriteLine(user.data["name"]);

int? count = SQL.selectValueInteger("select count(*) from users").value;
```

### Insert

The query is sent as is, nothing is added to it. To get the new id, return it with `OUTPUT INSERTED.id`: `insertedId` is the first column of the first returned row. Without `OUTPUT`, `insertedId` is `null`.

```csharp
var insert = SQL.insert("insert into users(name, age) output inserted.id values(@name, @age)",
    new Dictionary<string, object> { { "@name", "Gholibjon" }, { "@age", 29 } });

if (insert.execute)
    Console.WriteLine("New id: " + insert.insertedId);
else if (insert.duplicate)
    Console.WriteLine("Already exists");
```

### Insert in a transaction

```csharp
var queries = new List<string>
{
    "insert into users(name) output inserted.id values(@name)",
    "insert into cities(name) output inserted.id values('Khujand')"
};

var parameters = new List<Dictionary<string, object>>
{
    new Dictionary<string, object> { { "@name", "Gholibjon" } },
    null
};

var insert = SQL.insert(queries, parameters);
// insert.insertedIds[i] — id added by query i (or null)
// insert.insertedId     — the last added id
```

### Update and delete

```csharp
var update = SQL.update("update users set age = @age where id = @id",
    new Dictionary<string, object> { { "@age", 30 }, { "@id", 1 } });

var delete = SQL.delete("delete from users where id = @id",
    new Dictionary<string, object> { { "@id", 1 } });

if (update.execute)
    Console.WriteLine("Updated: " + update.affectedRowCount);
else if (!update.exception)
    Console.WriteLine("Nothing to update");
else
    Console.WriteLine(update.message);
```

### Query

Runs one statement of any type. Rows are in `data[0]`. For a query that starts with `INSERT`, `insertedId` is the first column of the first `OUTPUT` row:

```csharp
var q = SQL.query("insert into users(name) output inserted.id, inserted.created values(@name)",
    new Dictionary<string, object> { { "@name", "Ali" } });

if (q.execute)
    Console.WriteLine("New id: " + q.insertedId + ", created: " + q.data[0][0]["created"]);
```

Several statements in one transaction. Index `i` refers to query `i`: `data[i]` holds its rows, `insertedIds[i]` its new id (or `null`):

```csharp
var q = SQL.query(new List<string>
{
    "update users set age = age + 1",
    "insert into logs(text) output inserted.id values('age updated')",
    "select count(*) as total from users"
});

Console.WriteLine(q.affectedRowCount);       // changed rows in total
Console.WriteLine(q.insertedIds[1]);         // id from the 2nd query
Console.WriteLine(q.data[2][0]["total"]);    // result of the 3rd query
```

### Stored procedure

`data` holds the rows of the first result the procedure returns. Later results are read to the end, so an error in them gives `execute = false`. OUTPUT parameters and the return value are not read; return them with `SELECT` instead.

```csharp
var r = SQL.execute("dbo.GetUsersByAge",
    new Dictionary<string, object> { { "@age", 18 } });

if (r.execute)
{
    foreach (var row in r.data)
        Console.WriteLine(row["name"]);
}
```

## Notes

- **One statement per query.** `insert`, `update`, `delete` and `query` send the text as is, nothing is added. Pass one statement, and one statement per list item. Only the first result is returned in `data`.
- **Query type is detected by its first word.** `insertedId` and the "no rows changed" rollback in `query` work only for queries that start with `INSERT`, `UPDATE`, `DELETE` or `MERGE`. A query that starts with `WITH` or a comment runs normally, but reports `insertedId = null` and is not checked for "no rows changed".
- **New id.** `insertedId` is the first column of the first `OUTPUT` row, so put the id first: `output inserted.id, inserted.name`. It is `null` without `OUTPUT`, or when the first column is not an integer (for example a `UNIQUEIDENTIFIER` key). With `query` the key of any type is in `data[0]`. When `OUTPUT` returns several rows, `insertedId` is the first one; the rest are in `data[0]` of `query`.
- **Triggers.** SQL Server does not allow `OUTPUT` without `INTO` on a table with triggers (error 334), so `insertedId` is not available for such tables. `affectedRowCount` also counts rows changed by triggers (add `SET NOCOUNT ON` inside the trigger to avoid that).
- **Unique column names.** Give computed columns a name with `AS` (`select count(*) as total`). Columns with the same name overwrite each other.
- **`SET NOCOUNT ON`** makes `affectedRowCount` `0`. In a list it also makes an INSERT, UPDATE, DELETE or MERGE look like "no rows changed", so the transaction is rolled back. Don't use it in these queries.
- **CREATE, DROP, ALTER, EXEC and SELECT** report `affectedRowCount = 0` (EXEC reports the rows changed inside the procedure).
- **Statements not allowed in a transaction** (`CREATE DATABASE`, `ALTER DATABASE`, `BACKUP`, `RESTORE`) work with `query(text)`, but fail in a list.
- **First row only.** `selectRow`, `selectRowString` and the `selectValue*` methods use the first row, but the rest of the result is still read to the end. Add `TOP 1` to the query: on a large table this is the difference between milliseconds and seconds. An error in a later row (for example a division by zero) is not reported by these methods.
- **Missing parameter:** a parameter used in the SQL but not passed gives `execute = false`. The `@` in a parameter name may be omitted.
- **Parameter types** come from the C# value (`AddWithValue`):
  - `string` is sent as `NVARCHAR`. Store Cyrillic text in `NVARCHAR` columns; a `VARCHAR` column turns it into `?????`. On a large table, comparing an indexed `VARCHAR` column with a string parameter can be slower.
  - `null` is sent as `NVARCHAR` NULL. It works for number, text and date columns, but not for `VARBINARY`: pass `System.Data.SqlTypes.SqlBinary.Null` instead.
  - `DateTime` is sent as `DATETIME`: dates before 1753 (`DateTime.MinValue`) give `execute = false`, and the time is rounded to about 3 ms, so `=` against a `DATETIME2` column may find nothing. Compare by range (`>=` and `<`) instead.
- **`selectValueInteger`** converts any number or numeric text with `Convert.ToInt32`: decimals are rounded to even (`2.5` → `2`, `3.5` → `4`). A value outside the `int` range or non-numeric text gives `execute = false`.
- **`selectValueDouble`** converts any number (`DECIMAL`, `MONEY`, `INT`, `REAL`, `FLOAT`) or numeric text with a dot. Non-numeric text gives `execute = false`.
- **String methods** (`selectString`, `selectRowString`, `selectValueString`) use the invariant culture: `1.5`, `True`, dates as `11/03/2026 14:05:06` (month first, no milliseconds), `VARBINARY` as `System.Byte[]`. Format dates in SQL (`CONVERT(varchar, dt, 120)`) if you need another format.
- **Threads.** Every call opens its own pooled connection, so methods can be called from several threads at once. `connectionString` and `commandTimeout` are shared by all threads: set them once at startup.
