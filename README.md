# Bee.SQL
A very simple way to work with a SQL database

## Help, Feedback, Contribute
If you have any issues or feedback, please file an issue here in Github. We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## Dependencies

### .NET Core
System.Data.SqlClient 4.8.6

## Examples

### Select without parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
    	var result = SQL.select("select id, name, lastname, age from users");
	
	foreach(var row in result.data)
	{
		Console.WriteLine("id:" + row["id"] + ", name:" + row["name"] + ", lastname:" + row["lastname"] + ", age:" + row["age"]);
	}
}
```

### Select parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.select("select id, name, lastname, age from users where id = @user_id", new Dictionary<string, object>{{"@user_id", 1}});
	
	foreach(var row in result.data)
	{
		Console.WriteLine("id:" + row["id"] + ", name:" + row["name"] + ", lastname:" + row["lastname"] + ", age:" + row["age"]);
	}
}
```

### Select row without parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
    	var result = SQL.selectRow("select id, name, lastname, age from users");
	
	Console.WriteLine("id:" + result.data["id"] + ", name:" + result.data["name"] + ", lastname:" + result.data["lastname"] + ", age:" + result.data["age"]);
}
```

### Select row with parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.selectRow("select id, name, lastname, age from users where id = @user_id", new Dictionary<string, object>{{"@user_id", 1}});
	
	Console.WriteLine("id:" + result.data["id"] + ", name:" + result.data["name"] + ", lastname:" + result.data["lastname"] + ", age:" + result.data["age"]);
}
```

### Select value
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.selectValue("select id from users where id = @user_id", new Dictionary<string, object>{{"@user_id", 1}});
	
	Console.WriteLine("id:" + result.value);
}
```

### Sample result check
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.select("select id, name, lastname, age from users");

	if(!result.execute)
	{
		Console.WriteLine("No result! " + result.message);
		return;
	}

	foreach(var row in result.data)
	{
		Console.WriteLine("id:" + row["id"] + ", name:" + row["name"] + ", lastname:" + row["lastname"] + ", age:" + row["age"]);
	}
}
```

### Insert example
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.insert("insert into users(name, lastname, age) values('Gholibjon', 'Madiyarov', 29)");
	
	if(result.execute)
	{
		Console.WriteLine("Request completed successfully " + result.message);
		return;
	}

	Console.WriteLine("Request failed " + result.message);
}
```

### Insert example with parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.insert("insert into users(name, lastname, age) values(@name, @lastname, @age)", new Dictionary<string, object>{{"@name", "Gholibjon"}, {"@lastname", "Madiyarov"}, {"@age", 29}});
	
	if(result.execute)
	{
		Console.WriteLine("Request completed successfully " + result.message);
	}
	else
	{
		Console.WriteLine("Request failed " + result.message);
	}
}
```

### Insert example with transaction
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	
	var queries = new List<string>(){
		"insert into users(name, lastname, age) values('Gholibjon', 'Madiyarov', 29)",
		"insert into cities(name, description) values('Hujand', 'This is one of the most civilized and hospitable cities in Central Asia.')",
		"insert into cars(name, description) values('Mercedes Benz', 'One of the most perfect and friendly cars in the world.')"
	};
	
	var result = SQL.insert(queries);
	
	if(result.execute)
	{
		Console.WriteLine("Request completed successfully " + result.message);
	}
	else
	{
		Console.WriteLine("Request failed " + result.message);
	}
}
```

### Insert example with transaction, with parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	
	var queries = new List<string>(){
		"insert into users(name, lastname, age) values(@name, @lastname, @age)",
		"insert into cities(name, description) values(@name, @description)",
		"insert into cars(name, description) values(@name, @description)"
	};
	
	var parameters = new List<Dictionary<string, object>>{
		new Dictionary<string, object>{{"@name", "Gholibjon"}, {"@lastname", "Madiyarov"}, {"@age", 29}},
		new Dictionary<string, object>{{"@name", "Hujand"}, {"@description", "This is one of the most civilized and hospitable cities in Central Asia."}},
		new Dictionary<string, object>{{"@name", "Mercedes Benz"}, {"@description", "One of the most perfect and friendly cars in the world."}},
	};
	
	var result = SQL.insert(queries, parameters);
	
	if(result.execute)
	{
		Console.WriteLine("Request completed successfully " + result.message);
	}
	else
	{
		Console.WriteLine("Request failed " + result.message);
	}
}
```

### Insert example with transaction, with null parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	
	var queries = new List<string>(){
		"insert into users(name, lastname, age) values(@name, @lastname, @age)",
		"insert into cities(name, description) values('Hujand', 'This is one of the most civilized and hospitable cities in Central Asia.')",
		"insert into cars(name, description) values(@name, @description)"
	};
	
	var parameters = new List<Dictionary<string, object>>{
		new Dictionary<string, object>{{"@name", "Gholibjon"}, {"@lastname", "Madiyarov"}, {"@age", 29}},
		null,
		new Dictionary<string, object>{{"@name", "Mercedes Benz"}, {"@description", "One of the most perfect and friendly cars in the world."}},
	};
	
	var result = SQL.insert(queries, parameters);
	
	if(result.execute)
	{
		Console.WriteLine("Request completed successfully " + result.message);
	}
	else
	{
		Console.WriteLine("Request failed " + result.message);
	}
}
```

## Stored Procedures

### For result
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.executeResult("ProcedureName");
	
	if(result.execute)
	{
		foreach(var row in result.data)
		{
			//Console.WriteLine();
		}
	}
	else
	{
		Console.WriteLine("No result! " + result.message);
	}
}
```

### For without result
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.executeQuery("ProcedureName");
	
	if(result.execute)
	{
		Console.WriteLine("AffectedRowCount:" + result.affectedRowCount);
	}
	else
	{
		Console.WriteLine("No result! " + result.message);
	}
}
```

### With parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	SQL.connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.executeQuery("ProcedureName", new Dictionary<string, object>{{"@paramName", "ParamValue"}});
	
	if(result.execute)
	{
		Console.WriteLine("AffectedRowCount:" + result.affectedRowCount);
	}
	else
	{
		Console.WriteLine("No result! " + result.message);
	}
}
```
