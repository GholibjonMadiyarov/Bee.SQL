using Bee.SQL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;

namespace Bee.SQL
{
    /// <summary>Static methods for working with a SQL Server database. Methods do not throw: errors are returned in the result object.</summary>
    public class SQL
    {
        /// <summary>Connection string used by all methods.</summary>
        public static string connectionString = "Server=127.0.0.1;Database=Test;User Id=TestUser;Password=TestPassword;Connection Timeout=15";

        /// <summary>Log file path used by <see cref="version()"/>. Null disables logging.</summary>
        public static string logPath = null;

        /// <summary>Command timeout in seconds for all methods. 0 means no limit. Default is 30.</summary>
        public static int commandTimeout = 30;

        /// <summary>Writes the library version to a log file.</summary>
        /// <param name="path">Log file path. If null, nothing is written.</param>
        public static void version(string path = null)
        {
            if (path != null)
            {
                Log.info(path, "Version:" + Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString());
            }
        }

        /// <summary>Writes the library version to <c>logPath</c>. If <c>logPath</c> is null, nothing is written.</summary>
        public static void version()
        {
            if (logPath != null)
            {
                Log.info(logPath, "Version:" + Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString());
            }
        }

        /// <summary>Runs a SELECT query and returns all rows.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>data</c>: rows as "column → value"; NULL becomes null.</returns>
        public static Select select(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                var rows = new List<Dictionary<string, object>>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }

                                rows.Add(row);
                            }
                        }
                    }
                }

                return new Select { execute = true, message = "Request completed successfully", queryText = queryText, data = rows };
            }
            catch (Exception e)
            {
                return new Select { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true, queryText = queryText };
            }
        }

        /// <summary>Runs a SELECT query and returns all rows with values converted to strings.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>data</c>: rows as "column → string" (invariant culture: <c>1.5</c>, not <c>1,5</c>; dates as <c>MM/dd/yyyy HH:mm:ss</c>); NULL becomes null.</returns>
        public static SelectString selectString(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                var rows = new List<Dictionary<string, string>>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = new Dictionary<string, string>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture);
                                }

                                rows.Add(row);
                            }
                        }
                    }
                }

                return new SelectString { execute = true, message = "Request completed successfully", query = queryText, data = rows };
            }
            catch (Exception e)
            {
                return new SelectString { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, query = queryText };
            }
        }

        /// <summary>Runs a SELECT query and returns the first row. Add <c>TOP 1</c> to the query: other rows are still read to the end.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>data</c>: first row as "column → value". <c>read</c> is false if there are no rows.</returns>
        public static SelectRow selectRow(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                var row = new Dictionary<string, object>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }

                                return new SelectRow { execute = true, message = "Request completed successfully", data = row, read = true };
                            }
                        }
                    }
                }

                return new SelectRow { execute = true, message = "The request was successful, but no result was returned", data = row, read = false };
            }
            catch (Exception e)
            {
                return new SelectRow { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true };
            }
        }

        /// <summary>Runs a SELECT query and returns the first row with values converted to strings. Add <c>TOP 1</c> to the query.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>data</c>: first row as "column → string" (invariant culture). <c>read</c> is false if there are no rows.</returns>
        public static SelectRowString selectRowString(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                var row = new Dictionary<string, string>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture);
                                }

                                return new SelectRowString { execute = true, message = "Request completed successfully", data = row, read = true };
                            }
                        }
                    }
                }

                return new SelectRowString { execute = true, message = "The request was successful, but no result was returned", data = row, read = false };
            }
            catch (Exception e)
            {
                return new SelectRowString { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true };
            }
        }

        /// <summary>Runs a query and returns the first column of the first row. Add <c>TOP 1</c> to the query.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>value</c>: the value, or null for NULL (<c>read</c> is true). <c>read</c> is false if there are no rows.</returns>
        public static SelectValue selectValue(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new SelectValue { execute = true, message = "Request completed successfully", value = reader.IsDBNull(0) ? null : reader.GetValue(0), read = true };
                            }
                        }
                    }
                }

                return new SelectValue { execute = true, message = "The request was successful, but no result was returned", read = false };
            }
            catch (Exception e)
            {
                return new SelectValue { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true };
            }
        }

        /// <summary>Runs a query and returns the first column of the first row as a string. Add <c>TOP 1</c> to the query.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>value</c>: the value as string (invariant culture), or null for NULL (<c>read</c> is true). <c>read</c> is false if there are no rows.</returns>
        public static SelectValueString selectValueString(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new SelectValueString { execute = true, message = "Request completed successfully", value = reader.IsDBNull(0) ? null : Convert.ToString(reader.GetValue(0), CultureInfo.InvariantCulture), read = true };
                            }
                        }
                    }
                }

                return new SelectValueString { execute = true, message = "The request was successful, but no result was returned", read = false };
            }
            catch (Exception e)
            {
                return new SelectValueString { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true };
            }
        }

        /// <summary>Runs a query and returns the first column of the first row as an int. Add <c>TOP 1</c> to the query.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>value</c>: the value converted with Convert.ToInt32 (any number type or numeric text; 2.5 → 2, 3.5 → 4). NULL or no rows: <c>value</c> is null and <c>read</c> is false. A value outside the int range or non-numeric text returns <c>execute</c> = false.</returns>
        public static SelectValueInteger selectValueInteger(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;
    
                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if(reader.IsDBNull(0))
                                {
                                    return new SelectValueInteger { execute = true, message = "The request was successful, but no result was returned", value = null, read = false };
                                }
                                
                                return new SelectValueInteger { execute = true, message = "Request completed successfully", value = Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture), read = true };
                            }
                        }
                    }
                }

                return new SelectValueInteger { execute = true, message = "The request was successful, but no result was returned", read = false };
            }
            catch (Exception e)
            {
                return new SelectValueInteger { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true };
            }
        }

        /// <summary>Runs a query and returns the first column of the first row as a double. Add <c>TOP 1</c> to the query.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>value</c>: the value converted with Convert.ToDouble (any number type or numeric text with a dot). NULL or no rows: <c>value</c> is null and <c>read</c> is false. Non-numeric text returns <c>execute</c> = false.</returns>
        public static SelectValueDouble selectValueDouble(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if(reader.IsDBNull(0))
                                {
                                    return new SelectValueDouble { execute = true, message = "The request was successful, but no result was returned", value = null, read = false };
                                }

                                return new SelectValueDouble { execute = true, message = "Request completed successfully", value = Convert.ToDouble(reader.GetValue(0), CultureInfo.InvariantCulture), read = true };
                            }
                        }
                    }
                }

                return new SelectValueDouble { execute = true, message = "The request was successful, but no result was returned", read = false };
            }
            catch (Exception e)
            {
                return new SelectValueDouble { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true };
            }
        }

        /// <summary>Runs one INSERT statement. The text is sent as is.</summary>
        /// <param name="queryText">One INSERT statement. To get the new id, add <c>OUTPUT INSERTED.id</c>.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>insertedId</c>: first column of the first OUTPUT row if it is an integer, otherwise null. <c>duplicate</c> is true on a UNIQUE or PRIMARY KEY violation.</returns>
        public static Insert insert(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                long? insertedId = null;
                int count;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.FieldCount > 0 && reader.Read())
                            {
                                object value = reader.GetValue(0);

                                if (value is long || value is int || value is short || value is byte || value is decimal)
                                {
                                    insertedId = Convert.ToInt64(value);
                                }
                            }

                            while (reader.NextResult()) { }

                            count = reader.RecordsAffected;
                        }
                    }
                }

                return new Insert
                {
                    execute = true,
                    message = "Request completed successfully",
                    insertedId = insertedId,
                    insertedIds = count != 0 ? new List<long?> { insertedId } : new List<long?>()
                };
            }
            catch (SqlException e)
            {
                return new Insert { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, duplicate = e.Number == 2601 || e.Number == 2627, exception = true, exceptionType = e.GetType().Name, exceptionNumber = e.Number, queryText = queryText, parameter = parameters };
            }
            catch (Exception e)
            {
                return new Insert { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true, exceptionType = e.GetType().Name, queryText = queryText, parameter = parameters };
            }
        }

        /// <summary>Runs several INSERT statements in one transaction. If one fails or inserts no rows, all are rolled back.</summary>
        /// <param name="queryTexts">INSERT statements, run in order. Each item is one statement, sent as is.</param>
        /// <param name="parameters">Parameters for each query, in the same order. An item may be null.</param>
        /// <returns><c>insertedIds[i]</c>: first column of the first OUTPUT row of query i if it is an integer, otherwise null. <c>insertedId</c>: the last one. <c>duplicate</c> is true on a UNIQUE or PRIMARY KEY violation. If a query inserts no rows or fails, <c>execute</c> is false and <c>queryText</c>/<c>parameter</c> show that query.</returns>
        public static Insert insert(List<string> queryTexts, List<Dictionary<string, object>> parameters = null)
        {
            if (queryTexts == null || queryTexts.Count == 0)
            {
                return new Insert { execute = false, message = "No queries provided" };
            }

            if (parameters != null && parameters.Count != queryTexts.Count)
            {
                return new Insert { execute = false, message = "The number of parameters does not match the number of queries" };
            }

            int index = 0;

            try
            {
                long? insertedId = null;
                var insertedIds = new List<long?>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            for (index = 0; index < queryTexts.Count; index++)
                            {
                                using (SqlCommand command = new SqlCommand(queryTexts[index], connection, transaction))
                                {
                                    command.CommandTimeout = commandTimeout;

                                    if (parameters != null && parameters[index] != null)
                                    {
                                        foreach (var parameter in parameters[index])
                                        {
                                            command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                                        }
                                    }

                                    long? id = null;
                                    int count;

                                    using (SqlDataReader reader = command.ExecuteReader())
                                    {
                                        if (reader.FieldCount > 0 && reader.Read())
                                        {
                                            object value = reader.GetValue(0);

                                            if (value is long || value is int || value is short || value is byte || value is decimal)
                                            {
                                                id = Convert.ToInt64(value);
                                            }
                                        }

                                        while (reader.NextResult()) { }

                                        count = reader.RecordsAffected;
                                    }

                                    if (count <= 0)
                                    {
                                        transaction.Rollback();

                                        return new Insert
                                        {
                                            execute = false,
                                            message = "Transaction canceled. Query " + (index + 1) + " did not change any rows",
                                            queryText = queryTexts[index],
                                            parameter = parameters != null ? parameters[index] : null
                                        };
                                    }

                                    insertedId = id;
                                    insertedIds.Add(id);
                                }
                            }

                            transaction.Commit();

                            return new Insert { execute = true, message = "Request completed successfully", insertedId = insertedId, insertedIds = insertedIds };
                        }
                        catch
                        {
                            try { transaction.Rollback(); } catch { }

                            throw;
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                return new Insert
                {
                    execute = false,
                    message = "Transaction canceled. " + e.Message,
                    stackTrace = e.StackTrace,
                    duplicate = e.Number == 2601 || e.Number == 2627,
                    exception = true,
                    exceptionType = e.GetType().Name,
                    exceptionNumber = e.Number,
                    queryText = index < queryTexts.Count ? queryTexts[index] : null,
                    parameter = parameters != null && index < parameters.Count ? parameters[index] : null
                };
            }
            catch (Exception e)
            {
                return new Insert
                {
                    execute = false,
                    message = "Transaction canceled. " + e.Message,
                    stackTrace = e.StackTrace,
                    exception = true,
                    exceptionType = e.GetType().Name,
                    queryText = index < queryTexts.Count ? queryTexts[index] : null,
                    parameter = parameters != null && index < parameters.Count ? parameters[index] : null
                };
            }
        }

        /// <summary>Runs one UPDATE statement.</summary>
        /// <param name="queryText">One UPDATE statement, sent as is.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>affectedRowCount</c>: number of changed rows. If no rows match, <c>execute</c> is false, <c>exception</c> is false and <c>message</c> is "No rows were changed". On an SQL error <c>exception</c> is true.</returns>
        public static Update update(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;    

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        int affectedRowCount = command.ExecuteNonQuery();

                        if (affectedRowCount > 0)
                        {
                            return new Update { execute = true, message = "Request completed successfully", affectedRowCount = affectedRowCount };
                        }

                        return new Update { execute = false, message = "No rows were changed", queryText = queryText, parameter = parameters };
                    }
                }
            }
            catch (Exception e)
            {
                return new Update { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true, queryText = queryText, parameter = parameters };
            }
        }

        /// <summary>Runs several UPDATE statements in one transaction. If one fails or changes no rows, all are rolled back.</summary>
        /// <param name="queryTexts">UPDATE statements, run in order. Each item is one statement, sent as is.</param>
        /// <param name="parameters">Parameters for each query, in the same order. An item may be null.</param>
        /// <returns><c>affectedRowCount</c>: total number of changed rows. If a query changes no rows or fails, <c>execute</c> is false and <c>queryText</c>/<c>parameter</c> show that query.</returns>
        public static Update update(List<string> queryTexts, List<Dictionary<string, object>> parameters = null)
        {
            if (queryTexts == null || queryTexts.Count == 0)
            {
                return new Update { execute = false, message = "No queries provided" };
            }

            if (parameters != null && parameters.Count != queryTexts.Count)
            {
                return new Update { execute = false, message = "The number of parameters does not match the number of queries" };
            }

            int index = 0;

            try
            {
                int affectedRowCount = 0;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            for (index = 0; index < queryTexts.Count; index++)
                            {
                                using (SqlCommand command = new SqlCommand(queryTexts[index], connection, transaction))
                                {
                                    command.CommandTimeout = commandTimeout;

                                    if (parameters != null && parameters[index] != null)
                                    {
                                        foreach (var parameter in parameters[index])
                                        {
                                            command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                                        }
                                    }

                                    int count = command.ExecuteNonQuery();

                                    if (count <= 0)
                                    {
                                        transaction.Rollback();

                                        return new Update
                                        {
                                            execute = false,
                                            message = "Transaction canceled. Query " + (index + 1) + " did not change any rows",
                                            queryText = queryTexts[index],
                                            parameter = parameters != null ? parameters[index] : null
                                        };
                                    }

                                    affectedRowCount += count;
                                }
                            }

                            transaction.Commit();

                            return new Update { execute = true, message = "Request completed successfully", affectedRowCount = affectedRowCount };
                        }
                        catch
                        {
                            try { transaction.Rollback(); } catch { }

                            throw;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Update
                {
                    execute = false,
                    message = "Transaction canceled. " + e.Message,
                    stackTrace = e.StackTrace,
                    exception = true,
                    queryText = index < queryTexts.Count ? queryTexts[index] : null,
                    parameter = parameters != null && index < parameters.Count ? parameters[index] : null
                };
            }
        }

        /// <summary>Runs one DELETE statement.</summary>
        /// <param name="queryText">One DELETE statement, sent as is.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>affectedRowCount</c>: number of deleted rows. If no rows match, <c>execute</c> is false, <c>exception</c> is false and <c>message</c> is "No rows were changed". On an SQL error <c>exception</c> is true.</returns>
        public static Delete delete(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        int affectedRowCount = command.ExecuteNonQuery();

                        if (affectedRowCount > 0)
                        {
                            return new Delete { execute = true, message = "Request completed successfully", affectedRowCount = affectedRowCount };
                        }

                        return new Delete { execute = false, message = "No rows were changed" };
                    }
                }
            }
            catch (Exception e)
            {
                return new Delete { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true };
            }
        }

        /// <summary>Runs one SQL statement of any type: SELECT, INSERT, UPDATE, DELETE, MERGE, EXEC, CREATE and others.</summary>
        /// <param name="queryText">One SQL statement, sent as is.</param>
        /// <param name="parameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>data[0]</c>: returned rows (SELECT or OUTPUT). <c>affectedRowCount</c>: number of rows changed by INSERT, UPDATE, DELETE or MERGE (0 for other statements and with SET NOCOUNT ON). <c>insertedId</c>: for a query that starts with INSERT, the first column of the first OUTPUT row if it is an integer, otherwise null. <c>duplicate</c> is true on a UNIQUE or PRIMARY KEY violation.</returns>
        public static Query query(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                var rows = new List<Dictionary<string, object>>();
                int affectedRowCount;
                long? insertedId = null;

                bool insert = queryText != null && queryText.TrimStart().StartsWith("INSERT", StringComparison.OrdinalIgnoreCase);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(queryText, connection))
                    {
                        command.CommandTimeout = commandTimeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.FieldCount > 0 && reader.Read())
                            {
                                var row = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }

                                if (insert && rows.Count == 0)
                                {
                                    object value = reader.GetValue(0);

                                    if (value is long || value is int || value is short || value is byte || value is decimal)
                                    {
                                        insertedId = Convert.ToInt64(value);
                                    }
                                }

                                rows.Add(row);
                            }

                            while (reader.NextResult()) { }

                            affectedRowCount = Math.Max(reader.RecordsAffected, 0);
                        }
                    }
                }

                return new Query
                {
                    execute = true,
                    message = "Request completed successfully",
                    queryText = queryText,
                    data = new List<List<Dictionary<string, object>>> { rows },
                    affectedRowCount = affectedRowCount,
                    insertedId = insertedId
                };
            }
            catch (SqlException e)
            {
                return new Query { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, duplicate = e.Number == 2601 || e.Number == 2627, exception = true, exceptionType = e.GetType().Name, exceptionNumber = e.Number, queryText = queryText };
            }
            catch (Exception e)
            {
                return new Query { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true, exceptionType = e.GetType().Name, queryText = queryText };
            }
        }

        /// <summary>Runs several SQL statements of any type in one transaction. If one fails, or a query that starts with INSERT, UPDATE, DELETE or MERGE changes no rows, all are rolled back.</summary>
        /// <param name="queryTexts">SQL statements, run in order. Each item is one statement, sent as is.</param>
        /// <param name="parameters">Parameters for each query, in the same order. An item may be null.</param>
        /// <returns><c>data[i]</c>: rows returned by query i. <c>affectedRowCount</c>: total number of changed rows. <c>insertedIds[i]</c>: for an INSERT, the first column of its first OUTPUT row if it is an integer, otherwise null. <c>insertedId</c>: the value of the last INSERT. <c>duplicate</c> is true on a UNIQUE or PRIMARY KEY violation. If a query changes no rows or fails, <c>execute</c> is false and <c>queryText</c> shows that query.</returns>
        public static Query query(List<string> queryTexts, List<Dictionary<string, object>> parameters = null)
        {
            if (queryTexts == null || queryTexts.Count == 0)
            {
                return new Query { execute = false, message = "No queries provided" };
            }

            if (parameters != null && parameters.Count != queryTexts.Count)
            {
                return new Query { execute = false, message = "The number of parameters does not match the number of queries" };
            }

            int index = 0;

            try
            {
                var data = new List<List<Dictionary<string, object>>>();
                var insertedIds = new List<long?>();
                int affectedRowCount = 0;
                long? insertedId = null;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            for (index = 0; index < queryTexts.Count; index++)
                            {
                                string text = (queryTexts[index] ?? "").TrimStart().ToUpperInvariant();

                                bool insert = text.StartsWith("INSERT");
                                bool change = insert || text.StartsWith("UPDATE") || text.StartsWith("DELETE") || text.StartsWith("MERGE");

                                using (SqlCommand command = new SqlCommand(queryTexts[index], connection, transaction))
                                {
                                    command.CommandTimeout = commandTimeout;

                                    if (parameters != null && parameters[index] != null)
                                    {
                                        foreach (var parameter in parameters[index])
                                        {
                                            command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                                        }
                                    }

                                    var rows = new List<Dictionary<string, object>>();
                                    long? id = null;
                                    int count;

                                    using (SqlDataReader reader = command.ExecuteReader())
                                    {
                                        while (reader.FieldCount > 0 && reader.Read())
                                        {
                                            var row = new Dictionary<string, object>();

                                            for (int i = 0; i < reader.FieldCount; i++)
                                            {
                                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                            }

                                            if (insert && rows.Count == 0)
                                            {
                                                object value = reader.GetValue(0);

                                                if (value is long || value is int || value is short || value is byte || value is decimal)
                                                {
                                                    id = Convert.ToInt64(value);
                                                }
                                            }

                                            rows.Add(row);
                                        }

                                        while (reader.NextResult()) { }

                                        count = Math.Max(reader.RecordsAffected, 0);
                                    }

                                    if (change && count == 0)
                                    {
                                        transaction.Rollback();

                                        return new Query
                                        {
                                            execute = false,
                                            message = "Transaction canceled. Query " + (index + 1) + " did not change any rows",
                                            queryText = queryTexts[index]
                                        };
                                    }

                                    affectedRowCount += count;
                                    data.Add(rows);

                                    if (insert)
                                    {
                                        insertedId = id;
                                    }

                                    insertedIds.Add(insert ? id : null);
                                }
                            }

                            transaction.Commit();

                            return new Query
                            {
                                execute = true,
                                message = "Request completed successfully",
                                data = data,
                                affectedRowCount = affectedRowCount,
                                insertedId = insertedId,
                                insertedIds = insertedIds
                            };
                        }
                        catch
                        {
                            try { transaction.Rollback(); } catch { }

                            throw;
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                return new Query
                {
                    execute = false,
                    message = "Transaction canceled. " + e.Message,
                    stackTrace = e.StackTrace,
                    duplicate = e.Number == 2601 || e.Number == 2627,
                    exception = true,
                    exceptionType = e.GetType().Name,
                    exceptionNumber = e.Number,
                    queryText = index < queryTexts.Count ? queryTexts[index] : null
                };
            }
            catch (Exception e)
            {
                return new Query
                {
                    execute = false,
                    message = "Transaction canceled. " + e.Message,
                    stackTrace = e.StackTrace,
                    exception = true,
                    exceptionType = e.GetType().Name,
                    queryText = index < queryTexts.Count ? queryTexts[index] : null
                };
            }
        }

        /// <summary>Runs a stored procedure and returns the rows of its first result.</summary>
        /// <param name="procedureName">Procedure name.</param>
        /// <param name="procedureParameters">Parameters (name → value). A null value is sent as NULL.</param>
        /// <returns><c>data</c>: rows of the first result as "column → value"; empty if the procedure returns no rows. Later results are read to the end, so an error in them is reported.</returns>
        public static ExecuteResult execute(string procedureName, Dictionary<string, object> procedureParameters = null)
        {
            try
            {
                var rows = new List<Dictionary<string, object>>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(procedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = commandTimeout;

                        if (procedureParameters != null)
                        {
                            foreach (var parameter in procedureParameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            bool rowsRead = false;

                            do
                            {
                                if (reader.FieldCount > 0 && !rowsRead)
                                {
                                    rowsRead = true;

                                    while (reader.Read())
                                    {
                                        var row = new Dictionary<string, object>();

                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                        }

                                        rows.Add(row);
                                    }
                                }
                            }

                            while (reader.NextResult());
                        }
                    }
                }

                return new ExecuteResult { execute = true, message = "Request completed successfully", data = rows };
            }
            catch (Exception e)
            {
                return new ExecuteResult { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace, exception = true };
            }
        }
    }
}
