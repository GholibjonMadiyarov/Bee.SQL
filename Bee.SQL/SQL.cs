using Bee.SQL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Bee.SQL
{
    /// <summary>
    /// Simple static helpers for working with a SQL Server database.
    /// </summary>
    /// <remarks>
    /// Every method opens its own connection, runs the query and closes the connection.
    /// Methods do not throw: on any error they return a result with <c>execute = false</c>
    /// and the error text in <c>message</c>.
    /// Parameter values that are <c>null</c> are sent as SQL <c>NULL</c>.
    /// </remarks>
    public class SQL
    {
        /// <summary>Connection string used by every method. Set it once at application start.</summary>
        public static string connectionString = "Server=127.0.0.1;Database=Test;User Id=TestUser;Password=TestPassword;Connection Timeout=15";

        /// <summary>Path of the log file used by <see cref="version()"/>. <c>null</c> turns logging off.</summary>
        public static string logPath = null;

        /// <summary>Writes the library version to the log file at <paramref name="path"/>. Does nothing if the path is <c>null</c>.</summary>
        /// <param name="path">Log file path.</param>
        public static void version(string path = null)
        {
            if (path != null)
            {
                Log.info(path, "Version:" + Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString());
            }
        }

        /// <summary>Writes the library version to <see cref="logPath"/>. Does nothing if <see cref="logPath"/> is <c>null</c>.</summary>
        public static void version()
        {
            if (logPath != null)
            {
                Log.info(logPath, "Version:" + Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString());
            }
        }

        /// <summary>Runs a SELECT query and returns all rows.</summary>
        /// <param name="queryText">SQL query, for example <c>SELECT id, name FROM users WHERE age &gt; @age</c>.</param>
        /// <param name="parameters">Parameters (name → value), for example <c>{ "@age", 18 }</c>. May be <c>null</c>.</param>
        /// <returns>
        /// <c>data</c>: list of rows; each row is a dictionary (column name → value). SQL <c>NULL</c> becomes <c>null</c>.
        /// The list is empty if no rows were found or the query failed (<c>execute = false</c>).
        /// </returns>
        public static Select select(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                var rows = new List<Dictionary<string, object>>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null && parameters.Count > 0)
                        {
                            foreach (KeyValuePair<string, object> parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();

                                for (int i = 0; i <= reader.FieldCount - 1; i++)
                                {
                                    // Check if the value is null or DBNull and assign null to the dictionary if it is
                                    if (reader.IsDBNull(i) || reader.GetValue(i).Equals(null) || reader.GetValue(i) == null)
                                    {
                                        row[reader.GetName(i)] = null;
                                    }
                                    else
                                    {
                                        // Assign the value to the dictionary if it is not null
                                        row[reader.GetName(i)] = reader.GetValue(i);
                                    }
                                }

                                rows.Add(row);
                            }
                        }
                    }
                }

                return new Select { execute = true, message = "Request completed successfully", data = rows };
            }
            catch(Exception e)
            {
                return new Select { execute = false, message = "Request failed. " + e.Message, exception = true, data = new List<Dictionary<string, object>>() };
            }
        }

        /// <summary>Runs a SELECT query and returns all rows with every value converted to a string.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). May be <c>null</c>.</param>
        /// <returns>
        /// <c>data</c>: list of rows; each row is a dictionary (column name → text). Numbers and dates are converted
        /// with the invariant culture (for example <c>1.5</c>, not <c>1,5</c>). SQL <c>NULL</c> becomes <c>null</c>.
        /// <c>query</c>: the query text.
        /// </returns>
        public static SelectString selectString(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null && parameters.Count > 0)
                        {
                            foreach (KeyValuePair<string, object> parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, string> row = new Dictionary<string, string>();

                                for (int i = 0; i <= reader?.FieldCount - 1; i++)
                                {
                                    if (reader.IsDBNull(i) || reader.GetValue(i).Equals(null) || reader.GetValue(i) == null)
                                    {
                                        row[reader?.GetName(i)] = null;
                                    }
                                    else
                                    {
                                        row[reader?.GetName(i)] = Convert.ToString(reader?.GetValue(i), CultureInfo.InvariantCulture);
                                    }
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
                return new SelectString { execute = false, message = "Request failed. " + e.Message, query = queryText, data = new List<Dictionary<string, string>>() };
            }
        }

        /// <summary>Runs a SELECT query and returns only the first row.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). May be <c>null</c>.</param>
        /// <returns>
        /// <c>data</c>: the first row (column name → value); empty if no row was found.
        /// <c>read</c>: <c>true</c> if a row was found.
        /// </returns>
        public static SelectRow selectRow(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                var row = new Dictionary<string, object>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null && parameters.Count > 0)
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
                                for (int i = 0; i <= reader.FieldCount - 1; i++)
                                {
                                    if (reader.IsDBNull(i) || reader.GetValue(i).Equals(null) || reader.GetValue(i) == null)
                                    {
                                        row[reader.GetName(i)] = null;
                                    }
                                    else
                                    {
                                        row[reader.GetName(i)] = reader.GetValue(i);
                                    }
                                }

                                return new SelectRow { execute = true, message = "Request completed successfully", data = row, read = true };
                            }
                        }
                    }
                }

                return new SelectRow { execute = true, message = "Request completed successfully", data = row, read = false };
            }
            catch(Exception e)
            {
                return new SelectRow { execute = false, message = "Request failed. " + e.Message , data = new Dictionary<string, object>(), read = false, exception = true };
            }
        }

        /// <summary>Runs a SELECT query and returns only the first row, with every value converted to a string.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). May be <c>null</c>.</param>
        /// <returns>
        /// <c>data</c>: the first row (column name → text, invariant culture); empty if no row was found.
        /// <c>read</c>: <c>true</c> if a row was found.
        /// </returns>
        public static SelectRowString selectRowString(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                var row = new Dictionary<string, string>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null && parameters.Count > 0)
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
                                for (int i = 0; i <= reader.FieldCount - 1; i++)
                                {
                                    if (reader.IsDBNull(i) || reader.GetValue(i).Equals(null) || reader.GetValue(i) == null)
                                    {
                                        row[reader.GetName(i)] = null;
                                    }
                                    else
                                    {
                                        row[reader.GetName(i)] = Convert.ToString(reader?.GetValue(i), CultureInfo.InvariantCulture);
                                    }
                                }

                                return new SelectRowString { execute = true, message = "Request completed successfully", data = row, read = true };
                            }
                        }
                    }
                }

                return new SelectRowString { execute = true, message = "Request completed successfully", data = row, read = false };
            }
            catch (Exception e)
            {
                return new SelectRowString { execute = false, message = "Request failed. " + e.Message, data = new Dictionary<string, string>(), read = false, exception = true };
            }
        }

        /// <summary>Runs a query and returns one value: the first column of the first row.</summary>
        /// <param name="queryText">SQL query, for example <c>SELECT name FROM users WHERE id = @id</c>.</param>
        /// <param name="parameters">Parameters (name → value). May be <c>null</c>.</param>
        /// <returns>
        /// <c>value</c>: the value, or <c>null</c> if it is SQL <c>NULL</c> or no row was found.
        /// <c>read</c>: <c>true</c> if a row was found (use it to tell "no row" from "NULL").
        /// </returns>
        public static SelectValue selectValue(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null && parameters.Count > 0)
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

                return new SelectValue { execute = true, message = "The request was successful, but no result was returned", value = null, read = false };
            }
            catch(Exception e)
            {
                return new SelectValue { execute = false, message = "Request failed. " + e.Message, value = null, read = false, exception = true};
            }
        }

        /// <summary>Runs a query and returns the first column of the first row as a string (invariant culture).</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). May be <c>null</c>.</param>
        /// <returns>
        /// <c>value</c>: the value as text, or <c>null</c> if it is SQL <c>NULL</c> or no row was found.
        /// <c>read</c>: <c>true</c> if a row was found.
        /// </returns>
        public static SelectValueString selectValueString(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null && parameters.Count > 0)
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
                                return new SelectValueString { 
                                    execute = true, 
                                    message = "Request completed successfully", 
                                    value = reader.IsDBNull(0) ? null : Convert.ToString(reader?.GetValue(0), 
                                    CultureInfo.InvariantCulture), 
                                    read = true 
                                };
                            }
                        }
                    }
                }

                return new SelectValueString { execute = true, message = "The request was successful, but no result was returned", value = null, read = false };
            }
            catch (Exception e)
            {
                return new SelectValueString { execute = false, message = "Request failed. " + e.Message, value = null, read = false, exception = true };
            }
        }

        /// <summary>Runs a query and returns the first column of the first row as an <see cref="int"/>, for example <c>SELECT COUNT(*) FROM users</c>.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). May be <c>null</c>.</param>
        /// <returns>
        /// <c>value</c>: the number, or <c>null</c> if it is SQL <c>NULL</c> or no row was found.
        /// <c>read</c>: <c>true</c> if a row was found.
        /// If the value does not fit in <see cref="int"/> (for example a large BIGINT) or is not a number,
        /// <c>execute</c> is <c>false</c>.
        /// </returns>
        public static SelectValueInteger selectValueInteger(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null && parameters.Count > 0)
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
                                if (reader.IsDBNull(0) || reader.GetValue(0).Equals(null) || reader.GetValue(0) == null)
                                {
                                    return new SelectValueInteger { execute = true, message = "Request completed successfully", value = null, read = true };
                                }
                                
                                return new SelectValueInteger { execute = true, message = "Request completed successfully", value = Convert.ToInt32(reader.GetValue(0)), read = true };
                            }
                        }
                    }
                }

                return new SelectValueInteger { execute = true, message = "The request was successful, but no result was returned", value = null, read = false };
            }
            catch (Exception e)
            {
                return new SelectValueInteger { execute = false, message = "Request failed. " + e.Message, value = null, read = false, exception = true };
            }
        }

        /// <summary>Runs a query and returns the first column of the first row as a <see cref="double"/>, for example <c>SELECT AVG(price) FROM products</c>.</summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters (name → value). May be <c>null</c>.</param>
        /// <returns>
        /// <c>value</c>: the number, or <c>null</c> if it is SQL <c>NULL</c> or no row was found.
        /// <c>read</c>: <c>true</c> if a row was found. If the value is not a number, <c>execute</c> is <c>false</c>.
        /// </returns>
        public static SelectValueDouble selectValueDouble(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null && parameters.Count > 0)
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
                                if (reader.IsDBNull(0) || reader.GetValue(0).Equals(null) || reader.GetValue(0) == null)
                                {
                                    return new SelectValueDouble { 
                                        execute = true, 
                                        message = "Request completed successfully", 
                                        value = null, 
                                        read = true 
                                    };
                                }

                                return new SelectValueDouble { 
                                    execute = true, 
                                    message = "Request completed successfully", 
                                    value = Convert.ToDouble(reader.GetValue(0), 
                                    CultureInfo.InvariantCulture), 
                                    read = true 
                                };
                            }
                        }
                    }
                }

                return new SelectValueDouble { execute = true, message = "The request was successful, but no result was returned", value = null, read = false };
            }
            catch (Exception e)
            {
                return new SelectValueDouble { execute = false, message = "Request failed. " + e.Message, value = null, read = false, exception = true };
            }
        }

        /// <summary>Runs one INSERT statement in a transaction and returns the new id, if the statement returns it.</summary>
        /// <remarks>
        /// <para>
        /// The id is read from the first column of the first returned row. Return it with
        /// <c>OUTPUT INSERTED.id</c> or <c>; SELECT SCOPE_IDENTITY()</c>. Without them the row is still inserted
        /// and <c>insertedId</c> is <c>null</c>.
        /// </para>
        /// <para>
        /// The returned id must be a number (INT, BIGINT, or the DECIMAL from <c>SCOPE_IDENTITY()</c>) and must be
        /// the first column. Otherwise (for example <c>OUTPUT INSERTED.name, INSERTED.id</c> or a UNIQUEIDENTIFIER key)
        /// the insert is rolled back and <c>execute</c> is <c>false</c>.
        /// </para>
        /// <para>
        /// If the table has enabled triggers, <c>OUTPUT</c> without <c>INTO</c> fails (error 334); use <c>SCOPE_IDENTITY()</c>.
        /// </para>
        /// </remarks>
        /// <param name="queryText">SQL INSERT statement, for example <c>INSERT INTO users(name) OUTPUT INSERTED.id VALUES (@name)</c>.</param>
        /// <param name="parameters">Parameters (name → value). May be <c>null</c>.</param>
        /// <returns>
        /// <c>execute</c>: <c>true</c> if the row was saved.
        /// <c>insertedId</c>: the new id, or <c>null</c> if the statement returned none.
        /// <c>duplicate</c>: <c>true</c> on a UNIQUE or PRIMARY KEY violation (SQL errors 2601, 2627).
        /// <c>exception</c>, <c>exceptionType</c>, <c>exceptionNumber</c>, <c>message</c>: error details.
        /// </returns>
        public static Insert insert(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                long? insertedId = null;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand command = new SqlCommand())
                            {
                                command.Connection = connection;
                                command.CommandType = CommandType.Text;
                                command.Transaction = transaction;
                                command.CommandText = queryText;

                                if (parameters != null)
                                {
                                    foreach (var parameter in parameters)
                                    {
                                        command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                                    }
                                }

                                object data = command.ExecuteScalar();

                                if (data != null && data != DBNull.Value)
                                {
                                    insertedId = Convert.ToInt64(data);
                                }

                                transaction.Commit();

                                return new Insert
                                {
                                    execute = true,
                                    message = "Request completed successfully",
                                    insertedId = insertedId
                                };
                            }
                        }
                        catch (SqlException e)
                        {
                            transaction?.Rollback();

                            return new Insert
                            {
                                execute = false,
                                message = "Transaction canceled. " + e.Message,
                                duplicate = (e.Number == 2601 || e.Number == 2627),
                                exception = true,
                                exceptionType = e?.GetType()?.Name,
                                exceptionNumber = e.Number
                            };
                        }
                        catch (Exception e)
                        {
                            transaction?.Rollback();

                            return new Insert
                            {
                                execute = false,
                                message = "Transaction canceled. " + e.Message,
                                exception = true,
                                exceptionType = e?.GetType()?.Name
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Insert { execute = false, message = "Request failed. " + e.Message, exception = true, exceptionType = e?.GetType()?.Name };
            }
        }

        /// <summary>Runs several INSERT statements in one transaction and returns the new id of each one. If one fails, nothing is saved.</summary>
        /// <remarks>
        /// Each id is read the same way as in <see cref="insert(string, Dictionary{string, object})"/>:
        /// the first column of the first returned row, which must be a number.
        /// Statements are independent: an id returned by one statement cannot be used in the next one.
        /// For dependent inserts use one SQL batch with variables.
        /// </remarks>
        /// <param name="queryTexts">SQL INSERT statements, run in order. Must not be empty.</param>
        /// <param name="parameters">
        /// Parameters for each statement, in the same order (<c>parameters[i]</c> is for <c>queryTexts[i]</c>).
        /// May be <c>null</c> if no statement has parameters. If set, its count must equal the count of
        /// <paramref name="queryTexts"/>; use <c>null</c> for a statement without parameters.
        /// </param>
        /// <returns>
        /// <c>execute</c>: <c>true</c> if all statements succeeded and were saved.
        /// <c>insertedIds[i]</c>: the id returned by statement i, or <c>null</c>.
        /// <c>queryText</c>: on failure, the statement that failed.
        /// <c>duplicate</c>, <c>exception</c>, <c>exceptionType</c>, <c>exceptionNumber</c>, <c>message</c>: error details.
        /// </returns>
        public static Insert insert(List<string> queryTexts, List<Dictionary<string, object>> parameters = null)
        {
            try
            {
                if (queryTexts == null || queryTexts.Count == 0)
                {
                    return new Insert
                    {
                        execute = false,
                        message = "No queries provided"
                    };
                }

                if (parameters != null && parameters.Count != queryTexts.Count)
                {
                    return new Insert
                    {
                        execute = false,
                        message = "The number of parameters does not match the number of queries"
                    };
                }

                List<long?> insertedIds = new List<long?>();
                int index = 0;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand command = new SqlCommand())
                            {
                                command.Connection = connection;
                                command.Transaction = transaction;

                                index = 0;

                                while (index < queryTexts.Count)
                                {
                                    command.CommandType =  CommandType.Text;
                                    command.CommandText = queryTexts[index];

                                    command.Parameters.Clear();

                                    if (parameters != null && parameters.Count > 0 && parameters[index] != null)
                                    {
                                        foreach (var parameter in parameters[index])
                                        {
                                            command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                                        }
                                    }

                                    object insertedId = command.ExecuteScalar();

                                    if (insertedId != null && insertedId != DBNull.Value)
                                    {
                                        insertedIds.Add(Convert.ToInt64(insertedId));
                                    }
                                    else
                                    {
                                        insertedIds.Add(null);
                                    }

                                    index++;
                                }

                                transaction.Commit();

                                return new Insert { execute = true, message = "Request completed successfully", insertedIds = insertedIds };
                            }
                        }
                        catch (SqlException e)
                        {
                            transaction.Rollback();

                            return new Insert { 
                                execute = false, 
                                message = "Transaction canceled. " + e.Message, 
                                duplicate = (e.Number == 2601 || e.Number == 2627), 
                                exception = true, exceptionType = e.GetType().Name, 
                                exceptionNumber = e.Number,
                                queryText = index < queryTexts.Count ? queryTexts[index] : null
                            };
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();

                            return new Insert { 
                                execute = false, 
                                message = "Transaction canceled. " + e.Message, 
                                exception = true, 
                                exceptionType = e.GetType().Name,
                                queryText = index < queryTexts.Count ? queryTexts[index] : null
                            };
                        }
                    }
                }
            }
            catch(Exception e)
            {
                return new Insert { execute = false, message = "Request failed. " + e.Message, exception = true, exceptionType = e.GetType().Name };
            }
        }

        /// <summary>Runs one UPDATE statement.</summary>
        /// <param name="queryText">SQL UPDATE statement, for example <c>UPDATE users SET age = @age WHERE id = @id</c>.</param>
        /// <param name="parameters">Parameters (name → value). May be <c>null</c>.</param>
        /// <returns>
        /// <c>affectedRowCount</c>: number of changed rows.
        /// <c>execute</c>: <c>true</c> only if at least one row was changed. If no row matched the condition,
        /// <c>execute</c> is <c>false</c> and <c>exception</c> is <c>false</c>; on an error <c>exception</c> is <c>true</c>.
        /// </returns>
        public static Update update(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null && parameters.Count > 0 )
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        int affectedRowCount = command.ExecuteNonQuery();

                        if (affectedRowCount > 0) 
                        {
                            return new Update { execute = true, message = "Request completed successfully!", affectedRowCount = affectedRowCount };
                        }

                        return new Update { execute = false, message = "Request failed!", affectedRowCount = affectedRowCount };
                    }
                }
            }
            catch (Exception e)
            {
                return new Update { execute = false, message = "Request failed. " + e.Message, exception = true};
            }
        }

        /// <summary>Runs several UPDATE statements in one transaction. If one fails, nothing is saved.</summary>
        /// <param name="queryTexts">SQL UPDATE statements, run in order.</param>
        /// <param name="parameters">
        /// Parameters for each statement, in the same order. May be <c>null</c>. If set, its count must equal
        /// the count of <paramref name="queryTexts"/>; use <c>null</c> for a statement without parameters.
        /// </param>
        /// <returns>
        /// <c>execute</c>: <c>true</c> if all statements succeeded (even if some changed no rows).
        /// <c>affectedRowCount</c>: total number of changed rows.
        /// <c>queryText</c>, <c>parameter</c>: on failure, the statement that failed and its parameters.
        /// </returns>
        public static Update update(List<string> queryTexts, List<Dictionary<string, object>> parameters = null)
        {
            try
            {
                if (parameters != null && parameters.Count != queryTexts.Count)
                {
                    return new Update { execute = false, message = "The number of parameters does not match the number of queries" };
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    int index = 0;
                    int affectedRowCount = 0;

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand command = new SqlCommand())
                            {
                                command.Connection = connection;
                                command.Transaction = transaction;
                                command.CommandType = CommandType.Text;

                                while (index <= queryTexts.Count - 1)
                                {
                                    command.CommandText = queryTexts[index];

                                    command.Parameters.Clear();

                                    if (parameters != null && parameters.Count > 0)
                                    {
                                        if (parameters[index] != null)
                                        {
                                            foreach (var parameter in parameters[index])
                                            {
                                                command.Parameters.AddWithValue(parameter.Key, parameter.Value != null ? parameter.Value : DBNull.Value);
                                            }
                                        }
                                    }

                                    int i = command.ExecuteNonQuery();

                                    index++;

                                    affectedRowCount += i;
                                }

                                transaction.Commit();

                                return new Update { execute = true, message = "Request completed successfully", affectedRowCount = affectedRowCount};
                            }
                        }
                        catch (SqlException e)
                        {
                            transaction.Rollback();

                            return new Update { execute = false, message = "SqlException. Transaction canceled. " + e.Message, exception = true, queryText = queryTexts?[index], parameter = parameters?[index] };
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();

                            return new Update { execute = false, message = "Exception. Transaction canceled. " + e.Message, exception = true, queryText = queryTexts?[index], parameter = parameters?[index] };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Update { execute = false, message = "Exception. Request failed. " + e.Message, exception = true };
            }
        }

        /// <summary>Runs one DELETE statement.</summary>
        /// <param name="queryText">SQL DELETE statement, for example <c>DELETE FROM users WHERE id = @id</c>.</param>
        /// <param name="parameters">Parameters (name → value). May be <c>null</c>.</param>
        /// <returns>
        /// <c>execute</c>: <c>true</c> if the statement ran without error (also when no row matched).
        /// <c>affectedRowCount</c>: number of deleted rows.
        /// </returns>
        public static Delete delete(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        int affectedRowCount = command.ExecuteNonQuery();

                        return new Delete { execute = true, message = "Request completed successfully!", affectedRowCount = affectedRowCount };
                    }
                }
            }
            catch (Exception e)
            {
                return new Delete { execute = false, message = "Request failed. " + e.Message, exception = true };
            }
        }

        /// <summary>Runs one SQL statement of any type: SELECT, INSERT, UPDATE, DELETE, MERGE, EXEC, CREATE, ALTER, DROP and others.</summary>
        /// <remarks>
        /// <para>
        /// Pass exactly one statement. Only the first result is read, so an error raised by a second statement
        /// in the same text is not reported. For several statements use
        /// <see cref="query(List{string}, List{Dictionary{string, object}})"/>.
        /// </para>
        /// <para>
        /// Column names in the result must be unique and not empty: give every computed column a name with
        /// <c>AS</c> (for example <c>SELECT COUNT(*) AS total</c>). Otherwise values with the same name overwrite each other.
        /// </para>
        /// <para>
        /// Runs without a transaction, so statements that SQL Server does not allow inside a transaction
        /// (CREATE DATABASE, BACKUP and others) also work here.
        /// </para>
        /// <para>
        /// <c>insertedId</c> is set only when the text starts with <c>INSERT</c> and the first column of the first
        /// returned row is a number: put the id first, for example <c>OUTPUT INSERTED.id</c>.
        /// </para>
        /// </remarks>
        /// <param name="queryText">One SQL statement.</param>
        /// <param name="parameters">Parameters (name → value). May be <c>null</c>. A <c>null</c> value is sent as SQL <c>NULL</c>.</param>
        /// <returns>
        /// <c>data[0]</c>: returned rows (empty if the statement returns none).
        /// <c>affectedRowCount</c>: number of rows changed by INSERT, UPDATE, DELETE or MERGE (0 with <c>SET NOCOUNT ON</c>).
        /// <c>insertedId</c>: the new id for INSERT (see remarks), otherwise <c>null</c>.
        /// <c>duplicate</c>: <c>true</c> on a UNIQUE or PRIMARY KEY violation (SQL errors 2601, 2627).
        /// <c>exception</c>, <c>exceptionType</c>, <c>exceptionNumber</c>, <c>message</c>, <c>stackTrace</c>: error details.
        /// </returns>
        public static Query query(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                var rows = new List<Dictionary<string, object>>();

                int affectedRowCount = 0;
                long? insertedId = null;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null && parameters.Count > 0)
                        {
                            foreach (KeyValuePair<string, object> parameter in parameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();

                                for (int i = 0; i <= reader.FieldCount - 1; i++)
                                {
                                    if (reader.IsDBNull(i))
                                    {
                                        row[reader.GetName(i)] = null;
                                    }
                                    else
                                    {
                                        row[reader.GetName(i)] = reader.GetValue(i);
                                    }
                                }

                                rows.Add(row);
                            }

                            affectedRowCount = Math.Max(reader.RecordsAffected, 0);
                        }

                        if (rows.Count > 0 && queryText.TrimStart().ToUpperInvariant().StartsWith("INSERT"))
                        {
                            object value = rows[0].Values.First();

                            if (value is int || value is long || value is decimal)
                            {
                                insertedId = Convert.ToInt64(value);
                            }
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
                return new Query
                {
                    execute = false,
                    exception = true,
                    message = "Request failed. " + e.Message,
                    exceptionType = e.GetType().Name,
                    exceptionNumber = e.Number,
                    duplicate = e.Number == 2627 || e.Number == 2601,
                    stackTrace = e.StackTrace,
                    queryText = queryText
                };
            }
            catch (Exception e)
            {
                return new Query
                {
                    execute = false,
                    message = "Request failed. " + e.Message,
                    stackTrace = e.StackTrace,
                    queryText = queryText
                };
            }
        }

        /// <summary>Runs several SQL statements of any type in one transaction. If one fails, nothing is saved.</summary>
        /// <remarks>
        /// <para>
        /// Each item must be exactly one statement, with the same rules as in
        /// <see cref="query(string, Dictionary{string, object})"/> (one statement, unique column names, id in the first column).
        /// </para>
        /// <para>
        /// Statements that SQL Server does not allow inside a transaction (CREATE/ALTER DATABASE, BACKUP, RESTORE
        /// and others) fail here (error 226); run them with <see cref="query(string, Dictionary{string, object})"/>.
        /// </para>
        /// </remarks>
        /// <param name="queryTexts">SQL statements, run in order. Must not be empty.</param>
        /// <param name="parameters">
        /// Parameters for each statement, in the same order. May be <c>null</c>. If set, its count must equal
        /// the count of <paramref name="queryTexts"/>; use <c>null</c> for a statement without parameters.
        /// </param>
        /// <returns>
        /// <c>data[i]</c>: rows returned by statement i (empty if it returned none).
        /// <c>affectedRowCount</c>: total number of changed rows.
        /// <c>insertedIds[i]</c>: the id returned by statement i if it is an INSERT, otherwise <c>null</c>.
        /// <c>insertedId</c>: the last id found.
        /// <c>duplicate</c>, <c>exception</c>, <c>exceptionType</c>, <c>exceptionNumber</c>, <c>message</c>, <c>stackTrace</c>: error details.
        /// </returns>
        public static Query query(List<string> queryTexts, List<Dictionary<string, object>> parameters = null)
        {
            try
            {
                if (queryTexts == null || queryTexts.Count == 0)
                {
                    return new Query { execute = false, message = "No queries provided" };
                }

                if (parameters != null && parameters.Count != queryTexts.Count)
                {
                    return new Query { execute = false, message = "The number of parameters does not match the number of queries" };
                }

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
                            using (SqlCommand command = new SqlCommand())
                            {
                                command.Connection = connection;
                                command.Transaction = transaction;
                                command.CommandType = CommandType.Text;

                                for (int index = 0; index <= queryTexts.Count - 1; index++)
                                {
                                    var rows = new List<Dictionary<string, object>>();
                                    long? id = null;

                                    command.CommandText = queryTexts[index];
                                    command.Parameters.Clear();

                                    if (parameters != null && parameters[index] != null)
                                    {
                                        foreach (var parameter in parameters[index])
                                        {
                                            command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                                        }
                                    }

                                    using (SqlDataReader reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            var row = new Dictionary<string, object>();

                                            for (int i = 0; i <= reader.FieldCount - 1; i++)
                                            {
                                                if (reader.IsDBNull(i))
                                                {
                                                    row[reader.GetName(i)] = null;
                                                }
                                                else
                                                {
                                                    row[reader.GetName(i)] = reader.GetValue(i);
                                                }
                                            }

                                            rows.Add(row);
                                        }

                                        affectedRowCount += Math.Max(reader.RecordsAffected, 0);
                                    }

                                    if (rows.Count > 0 && queryTexts[index].TrimStart().ToUpperInvariant().StartsWith("INSERT"))
                                    {
                                        object value = rows[0].Values.First();

                                        if (value is int || value is long || value is decimal)
                                        {
                                            id = Convert.ToInt64(value);
                                            insertedId = id;
                                        }
                                    }

                                    data.Add(rows);
                                    insertedIds.Add(id);
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
                        }
                        catch (SqlException e)
                        {
                            try 
                            { 
                                transaction.Rollback(); 
                            } 
                            catch 
                            { 

                            }

                            return new Query
                            {
                                execute = false,
                                message = "Transaction canceled. " + e.Message,
                                stackTrace = e.StackTrace,
                                duplicate = e.Number == 2627 || e.Number == 2601,
                                exception = true,
                                exceptionType = e.GetType().Name,
                                exceptionNumber = e.Number
                            };
                        }
                        catch (Exception e)
                        {
                            try 
                            { 
                                transaction.Rollback(); 
                            } 
                            catch 
                            { 

                            }

                            return new Query
                            {
                                execute = false,
                                message = "Transaction canceled. " + e.Message,
                                stackTrace = e.StackTrace,
                                exception = true,
                                exceptionType = e.GetType().Name
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Query { execute = false, message = "Request failed. " + e.Message, stackTrace = e.StackTrace };
            }
        }

        /// <summary>Runs a stored procedure and returns the rows of its first result.</summary>
        /// <param name="procedureName">Procedure name, for example <c>dbo.GetUsers</c>.</param>
        /// <param name="procedureParameters">Procedure parameters (name → value), for example <c>{ "@age", 18 }</c>. May be <c>null</c>.</param>
        /// <returns>
        /// <c>data</c>: list of rows (column name → value); empty if the procedure returns no rows.
        /// </returns>
        public static ExecuteResult execute(string procedureName, Dictionary<string, object> procedureParameters = null)
        {
            try
            {
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = procedureName;

                        if (procedureParameters != null)
                        {
                            foreach (KeyValuePair<string, object> parameter in procedureParameters)
                            {
                                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();

                                for (int i = 0; i <= reader.FieldCount - 1; i++)
                                {
                                    row[reader.GetName(i).ToString()] = reader.IsDBNull(i) ? null : reader[reader.GetValue(i).ToString()];
                                }

                                rows.Add(row);
                            }
                        }
                    }
                }

                return new ExecuteResult { execute = true, message = "Request completed successfully", data = rows };
            }
            catch (Exception e)
            {
                return new ExecuteResult { execute = false, message = "Request failed. " + e.Message, data = new List<Dictionary<string, object>>() };
            }
        }
    }
}
