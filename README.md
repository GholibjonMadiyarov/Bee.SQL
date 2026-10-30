# Bee.SQL
A very simple way to work with a SQL Server database from C#.

One static class, `SQL`, with one method per kind of query. Every method opens its own connection, runs the query, closes the connection and returns a result object. Methods never throw: check `result.execute` and read `result.message` on failure.

## Requirements
- .NET Framework 4.0 or later
- SQL Server (uses `System.Data.SqlClient` from the .NET Framework, no extra packages)

## Quick start
```csharp
using Bee.SQL;
using System.Collections.Generic;

SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";

var result = SQL.select("SELECT id, name FROM users WHERE age > @age",
    new Dictionary<string, object> { { "@age", 18 } });

if (result.execute)
{
    foreach (var row in result.data)
        Console.WriteLine(row["id"] + " " + row["name"]);
}
else
{
    Console.WriteLine("Error: " + result.message);
}
```

## Methods

| Method | Use it for | Main result fields |
|---|---|---|
| `select` | all rows | `data` (list of rows) |
| `selectString` | all rows, values as text | `data` |
| `selectRow` | first row | `data` (one row), `read` |
| `selectRowString` | first row, values as text | `data`, `read` |
| `selectValue` | one value | `value`, `read` |
| `selectValueString` / `selectValueInteger` / `selectValueDouble` | one value as `string` / `int?` / `double?` | `value`, `read` |
| `insert(text)` | one INSERT, optionally get the new id | `insertedId`, `duplicate` |
| `insert(list)` | several INSERTs in one transaction | `insertedIds`, `queryText` |
| `update(text)` | one UPDATE | `affectedRowCount` |
| `update(list)` | several UPDATEs in one transaction | `affectedRowCount`, `queryText` |
| `delete` | one DELETE | `affectedRowCount` |
| `query(text)` | one statement of any type (SELECT, INSERT, UPDATE, DELETE, MERGE, CREATE, ALTER, DROP, ...) | `data`, `affectedRowCount`, `insertedId` |
| `query(list)` | several statements of any type in one transaction | `data`, `affectedRowCount`, `insertedIds` |
| `execute` | stored procedure | `data` |

Every result has:
- `execute` – `true` if the query ran successfully
- `message` – a short text; on failure it contains the SQL Server error

Most results also have `exception` (`true` if an error happened). `insert` and `query` results also have `duplicate`, `exceptionType` and `exceptionNumber` (the SQL Server error number, for example `2627`).

## Rules
- **Parameters** are passed as `Dictionary<string, object>` with names that start with `@`. A `null` value is sent as SQL `NULL`. Always use parameters for user input instead of building SQL text by hand.
- SQL `NULL` in results becomes `null`.
- **One statement per query.** `query(text)` and each item of `query(list)` must contain exactly one SQL statement. Only the first result is read, so an error in a second statement would not be reported.
- **Unique column names.** Give every computed column a name with `AS`, for example `SELECT COUNT(*) AS total`. Columns with the same name overwrite each other in the row dictionary.
- **New id.** Return it from the INSERT statement as the **first** column: `OUTPUT INSERTED.id` or `; SELECT SCOPE_IDENTITY()`. The id must be a number (INT or BIGINT).
- **Lists.** In `insert(list)`, `update(list)` and `query(list)`, `parameters[i]` belongs to `queries[i]`. The two lists must have the same length; use `null` for a statement without parameters. If one statement fails, nothing is saved.

## Examples

### Select
```csharp
var result = SQL.select("SELECT id, name, lastname, age FROM users");

foreach (var row in result.data)
{
    Console.WriteLine("id:" + row["id"] + ", name:" + row["name"] + ", lastname:" + row["lastname"] + ", age:" + row["age"]);
}
```

### Select one row
```csharp
var result = SQL.selectRow("SELECT id, name, age FROM users WHERE id = @id",
    new Dictionary<string, object> { { "@id", 1 } });

if (result.read)
    Console.WriteLine("name:" + result.data["name"] + ", age:" + result.data["age"]);
else
    Console.WriteLine("User not found");
```

### Select one value
```csharp
var count = SQL.selectValueInteger("SELECT COUNT(*) FROM users WHERE age > @age",
    new Dictionary<string, object> { { "@age", 18 } });

Console.WriteLine("Users: " + count.value);
```

`read` tells "no row" apart from a `NULL` value: if no row was found, `read` is `false`; if the value is `NULL`, `read` is `true` and `value` is `null`.

### Insert and get the new id
```csharp
var result = SQL.insert(
    "INSERT INTO users(name, lastname, age) OUTPUT INSERTED.id VALUES (@name, @lastname, @age)",
    new Dictionary<string, object> { { "@name", "John" }, { "@lastname", "Smith" }, { "@age", 29 } });

if (result.execute)
    Console.WriteLine("New id: " + result.insertedId);
else if (result.duplicate)
    Console.WriteLine("This user already exists");
else
    Console.WriteLine("Error: " + result.message);
```

Without `OUTPUT INSERTED.id` the row is still inserted and `insertedId` is `null`.

### Several inserts in one transaction
```csharp
var queries = new List<string>
{
    "INSERT INTO users(name, lastname, age) OUTPUT INSERTED.id VALUES (@name, @lastname, @age)",
    "INSERT INTO cities(name, description) VALUES ('Khujand', 'A city in Central Asia.')",
    "INSERT INTO cars(name, description) OUTPUT INSERTED.id VALUES (@name, @description)"
};

var parameters = new List<Dictionary<string, object>>
{
    new Dictionary<string, object> { { "@name", "John" }, { "@lastname", "Smith" }, { "@age", 29 } },
    null, // the second query has no parameters
    new Dictionary<string, object> { { "@name", "Mercedes-Benz" }, { "@description", "A German car." } }
};

var result = SQL.insert(queries, parameters);

if (result.execute)
    Console.WriteLine("Ids: " + string.Join(", ", result.insertedIds)); // for example "15, , 7"
else
    Console.WriteLine("Nothing was saved. Failed query: " + result.queryText + ". " + result.message);
```

### Update
```csharp
var result = SQL.update("UPDATE users SET age = @age WHERE id = @id",
    new Dictionary<string, object> { { "@age", 30 }, { "@id", 1 } });

if (result.execute)
    Console.WriteLine("Updated rows: " + result.affectedRowCount);
else if (!result.exception)
    Console.WriteLine("No row matched the condition");
else
    Console.WriteLine("Error: " + result.message);
```

`update(text)` returns `execute = false` when no row was changed. `delete` returns `execute = true` in that case, with `affectedRowCount = 0`.

### Delete
```csharp
var result = SQL.delete("DELETE FROM users WHERE id = @id",
    new Dictionary<string, object> { { "@id", 1 } });

Console.WriteLine("Deleted rows: " + result.affectedRowCount);
```

### Any statement with `query`
```csharp
// DDL
SQL.query("CREATE TABLE logs (id INT IDENTITY PRIMARY KEY, text NVARCHAR(200))");

// INSERT with the new id
var inserted = SQL.query("INSERT INTO logs(text) OUTPUT INSERTED.id VALUES (@text)",
    new Dictionary<string, object> { { "@text", "Started" } });
Console.WriteLine("New id: " + inserted.insertedId + ", rows: " + inserted.affectedRowCount);

// SELECT: rows are in data[0]
var selected = SQL.query("SELECT id, text FROM logs");
foreach (var row in selected.data[0])
    Console.WriteLine(row["id"] + " " + row["text"]);
```

`query(text)` runs without a transaction, so it can also run statements that SQL Server does not allow inside a transaction, such as `CREATE DATABASE` or `BACKUP`.

### Several statements of any type in one transaction
```csharp
var result = SQL.query(new List<string>
{
    "INSERT INTO orders(user_id) OUTPUT INSERTED.id VALUES (1)",
    "UPDATE users SET orders_count = orders_count + 1 WHERE id = 1",
    "SELECT orders_count FROM users WHERE id = 1"
});

if (result.execute)
{
    Console.WriteLine("Order id: " + result.insertedIds[0]);
    Console.WriteLine("Orders: " + result.data[2][0]["orders_count"]);
}
```

`data[i]` and `insertedIds[i]` belong to statement `i`. `CREATE DATABASE`, `BACKUP` and similar statements cannot run here; use `query(text)`.

### Stored procedure
```csharp
var result = SQL.execute("dbo.GetUsersByAge",
    new Dictionary<string, object> { { "@age", 18 } });

if (result.execute)
{
    foreach (var row in result.data)
        Console.WriteLine(row["id"] + " " + row["name"]);
}
else
{
    Console.WriteLine("Error: " + result.message);
}
```

## Help, Feedback, Contribute
If you have any issues or feedback, please file an issue here in GitHub. We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## License
MIT. See [LICENSE](LICENSE).
