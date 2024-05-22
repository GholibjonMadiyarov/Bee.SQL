# Bee.SQL
A very simple way to work with the database

## Examples

### Select with out parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	var connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
    var result = SQL.select(connectionString, "select id, name, lastname, age from users");
	
	foreach(var row in result.data)
	{
		Console.WriteLine("id:" + row["id"] + ", name:" + row["name"] + ", lastname:" + row["lastname"] + ", age:" + row["age"]);
	}
}
```

### Select with parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	var connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.select(connectionString, "select id, name, lastname, age from users where id = @user_id", new Dictionary<string, object>{{"@user_id", 1}});
	
	foreach(var row in result.data)
	{
		Console.WriteLine("id:" + row["id"] + ", name:" + row["name"] + ", lastname:" + row["lastname"] + ", age:" + row["age"]);
	}
}
```

### Select row with out parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	var connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
    var result = SQL.selectRow(connectionString, "select id, name, lastname, age from users");
	
	Console.WriteLine("id:" + result.data["id"] + ", name:" + result.data["name"] + ", lastname:" + result.data["lastname"] + ", age:" + result.data["age"]);
}
```

### Select row with parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	var connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.selectRow(connectionString, "select id, name, lastname, age from users where id = @user_id", new Dictionary<string, object>{{"@user_id", 1}});
	
	Console.WriteLine("id:" + result.data["id"] + ", name:" + result.data["name"] + ", lastname:" + result.data["lastname"] + ", age:" + result.data["age"]);
}
```

### Select value
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	var connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.selectValue(connectionString, "select id from users where id = @user_id", new Dictionary<string, object>{{"@user_id", 1}});
	
	Console.WriteLine("id:" + result.value);
}
```

### Sample result check
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	var connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.select(connectionString, "select id, name, lastname, age from users");
	
	if(result.execute)
	{
		foreach(var row in result.data)
		{
			Console.WriteLine("id:" + row["id"] + ", name:" + row["name"] + ", lastname:" + row["lastname"] + ", age:" + row["age"]);
		}
	}
	else
	{
		Console.WriteLine("No result! " + result.message);
	}
}
```

### Insert example
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	var connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.query(connectionString, "insert into users(name, lastname, age) values('Gholibjon', 'Madiyarov', 29)");
	
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

### Insert example with parameters
```csharp
using Bee.SQL;

static void Main(string[] args)
{
	var connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	var result = SQL.query(connectionString, "insert into users(name, lastname, age) values(@name, @lastname, @age)", new Dictionary<string, object>{{"@name", "Gholibjon"}, {"@lastname", "Madiyarov"}, {"@age", 29}});
	
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
	var connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	
	var queries = new List<string>(){
		"insert into users(name, lastname, age) values('Gholibjon', 'Madiyarov', 29)",
		"insert into cities(name, description) values('Hujand', 'This is one of the most civilized and hospitable cities in Central Asia.')",
		"insert into cars(name, description) values('Mercedes Benz', 'One of the most perfect and friendly cars in the world.')"
	};
	
	var result = SQL.query(connectionString, queries);
	
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
	var connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	
	var queries = new List<string>(){
		"insert into users(name, lastname, age) values(@name, @lastname, @age)",
		"insert into cities(name, description) values(@name, @description)",
		"insert into cars(name, description) values(@name, @description)"
	};
	
	var parameters = new List<Dictionary<string, object>>{
		new Dictionary<string, object>{{"@name", "Gholibjon"}, {"@lastname", "Madiyaov"}, {"@age", 29}},
		new Dictionary<string, object>{{"@name", "Hujand"}, {"@description", "This is one of the most civilized and hospitable cities in Central Asia."}},
		new Dictionary<string, object>{{"@name", "Mercedes Benz"}, {"@description", "One of the most perfect and friendly cars in the world."}},
	};
	
	var result = SQL.query(connectionString, queries, parameters);
	
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
	var connectionString = "Server=127.0.0.1;Database=db_name;User Id=db_user;Password=db_password;Connection Timeout=15";
	
	var queries = new List<string>(){
		"insert into users(name, lastname, age) values(@name, @lastname, @age)",
		"insert into cities(name, description) values('Hujand', 'This is one of the most civilized and hospitable cities in Central Asia.')",
		"insert into cars(name, description) values(@name, @description)"
	};
	
	var parameters = new List<Dictionary<string, object>>{
		new Dictionary<string, object>{{"@name", "Gholibjon"}, {"@lastname", "Madiyaov"}, {"@age", 29}},
		null,
		new Dictionary<string, object>{{"@name", "Mercedes Benz"}, {"@description", "One of the most perfect and friendly cars in the world."}},
	};
	
	var result = SQL.query(connectionString, queries, parameters);
	
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