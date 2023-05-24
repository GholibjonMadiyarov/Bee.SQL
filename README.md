# Bee.SQL
A very simple way to work with the database

## Examples

### Select with out parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	var cs = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
    var items = SQL.select(cs, "select id, name, lastname, age from users");
	
	foreach(var item in items)
	{
		Console.WriteLine("id:" + item["id"] + ", name:" + item["name"] + ", lastname:" + item["lastname"] + ", age:" + item["age"]);
	}
}
```

### Select with parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	var cs = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var items = SQL.select(cs, "select id, name, lastname, age from users where id = @user_id", new Dictionary<string, object>{{"@user_id", 1}});
	
	foreach(var item in items)
	{
		Console.WriteLine("id:" + item["id"] + ", name:" + item["name"] + ", lastname:" + item["lastname"] + ", age:" + item["age"]);
	}
}
```