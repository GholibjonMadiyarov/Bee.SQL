﻿using Bee.SQL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Bee.SQL
{
    public class SQL
    {
        public static string connectionString = "Server=127.0.0.1;Database=Test;User Id=TestUser;Password=TestPassword;Connection Timeout=15";

        /// <summary>
        /// Used to retrieve all rows from a table.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="limit">Limit.</param>
        /// <returns> Select model </returns>
        //public static Select select(string tableName, int? limit = null)
        //{
        //    try
        //    {
        //        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();

        //            using (SqlCommand command = new SqlCommand())
        //            {
        //                command.Connection = connection;
        //                command.CommandType = CommandType.Text;
        //                command.CommandText = "select " + limit == null ? "" : " top @limit " + " * from @table";

        //                command.Parameters.AddWithValue("@table", tableName);
        //                command.Parameters.AddWithValue("@limit", limit);

        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        Dictionary<string, object> row = new Dictionary<string, object>();

        //                        for (int i = 0; i <= reader.FieldCount - 1; i++)
        //                        {
        //                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader[reader.GetName(i)];
        //                        }

        //                        rows.Add(row);
        //                    }
        //                }
        //            }
        //        }

        //        return new Select { execute = true, message = "Request completed successfully", data = rows };
        //    }
        //    catch (Exception e)
        //    {
        //        return new Select { execute = false, message = "Request failed. " + e.Message, data = new List<Dictionary<string, object>>() };
        //    }
        //}

        ///// <summary>
        ///// Used to retrieve all rows from a table.
        ///// </summary>
        ///// <param name="tableName">Table name.</param>
        ///// <returns> Select model </returns>
        //public static Select select(string tableName)
        //{
        //    try
        //    {
        //        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();

        //            using (SqlCommand command = new SqlCommand())
        //            {
        //                command.Connection = connection;
        //                command.CommandType = CommandType.Text;
        //                command.CommandText = "select * from @table" ;

        //                command.Parameters.AddWithValue("@table", tableName);

        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        Dictionary<string, object> row = new Dictionary<string, object>();

        //                        for (int i = 0; i <= reader.FieldCount - 1; i++)
        //                        {
        //                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader[reader.GetName(i)];
        //                        }

        //                        rows.Add(row);
        //                    }
        //                }
        //            }
        //        }

        //        return new Select { execute = true, message = "Request completed successfully", data = rows };
        //    }
        //    catch (Exception e)
        //    {
        //        return new Select { execute = false, message = "Request failed. " + e.Message, data = new List<Dictionary<string, object>>() };
        //    }
        //}

        /// <summary>
        /// Used to retrieve data from a database.
        /// </summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns> Select model</returns>
        public static Select select(string queryText, Dictionary<string, object> parameters = null)
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
                        command.CommandType = CommandType.Text;
                        command.CommandText = queryText;

                        if (parameters != null)
                        {
                            foreach (KeyValuePair<string, object> parameter in parameters)
                            {
                                if (parameter.Value == null)
                                    command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                else
                                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();

                                for (int i = 0; i <= reader.FieldCount - 1; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader[reader.GetName(i)];
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
                return new Select { execute = false, message = "Request failed. " + e.Message, data = new List<Dictionary<string, object>>() };
            }
        }

        /// <summary>
        /// Used to retrieve data from a database.
        /// </summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns> Select model </returns>
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

                        if (parameters != null)
                        {
                            foreach (KeyValuePair<string, object> parameter in parameters)
                            {
                                if (parameter.Value == null)
                                    command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                else
                                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, string> row = new Dictionary<string, string>();

                                for (int i = 0; i <= reader.FieldCount - 1; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader[reader.GetName(i)].ToString();
                                }

                                rows.Add(row);
                            }
                        }
                    }
                }

                return new SelectString { execute = true, message = "Request completed successfully", data = rows };
            }
            catch (Exception e)
            {
                return new SelectString { execute = false, message = "Request failed. " + e.Message, data = new List<Dictionary<string, string>>() };
            }
        }

        /// <summary>
        /// Used to retrieve data from a database.
        /// </summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns> SelectRow model</returns>
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

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                if (parameter.Value == null)
                                    command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                else
                                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                for (int i = 0; i <= reader.FieldCount - 1; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader[reader.GetName(i)];
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

        /// <summary>
        /// Used to retrieve data from a database.
        /// </summary>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns> SelectValue model. The first column of the first row is returned.</returns>
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

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                if (parameter.Value == null)
                                    command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                else
                                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new SelectValue { execute = true, message = "Request completed successfully", value = reader.IsDBNull(0) ? null : reader[0], read = true };
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

        /// <summary>
        /// Requests insert for multiple queres.
        /// </summary>
        /// <param name="queryTexts">The SQL querys is represented as a list.</param>
        /// <param name="parameters">Parameters are given accordingly for each request.</param>
        /// <returns>Insert model</returns>
        public static Insert insert(List<string> queryTexts, List<Dictionary<string, object>> parameters = null)
        {
            try
            {
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

                                int index = 0;

                                while (index <= queryTexts.Count - 1)
                                {
                                    command.CommandType =  CommandType.Text;
                                    command.CommandText = queryTexts[index];

                                    command.Parameters.Clear();

                                    if (parameters != null)
                                    {
                                        if (parameters[index] != null)
                                        {
                                            foreach (var parameter in parameters[index])
                                            {
                                                command.Parameters.AddWithValue(parameter.Key, parameter.Value != null ? parameter.Value : DBNull.Value);
                                            }
                                        }
                                    }

                                    command.Transaction = transaction;

                                    command.ExecuteNonQuery();
                                    index++;
                                }

                                transaction.Commit();

                                return new Insert { execute = true, message = "Request completed successfully"};
                            }
                        }
                        catch (SqlException e)
                        {
                            transaction.Rollback();
                            return new Insert { execute = false, message = "SqlException. Transaction canceled. " + e.Message, duplicate = (e.Number == 2601 || e.Number == 2627) ? true : false, exception = true };
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            return new Insert { execute = false, message = "Exception. Transaction canceled. " + e.Message, exception = true };
                        }
                    }
                }
            }
            catch(Exception e)
            {
                return new Insert { execute = false, message = "Request failed. " + e.Message };
            }
        }

        /// <summary>
        /// Requests insert
        /// </summary>
        /// <param name="queryText">The SQL query is represented as a text.</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Insert model</returns>
        public static Insert insert(string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
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
                                command.CommandText = queryText + "; SELECT SCOPE_IDENTITY()";

                                if (parameters != null)
                                {
                                    foreach (var parameter in parameters)
                                    {
                                        if (parameter.Value == null)
                                            command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                        else
                                            command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                                    }
                                }

                                command.Transaction = transaction;

                                var r = command.ExecuteScalar();

                                if (r == null)
                                {
                                    transaction.Commit();

                                    return new Insert { execute = true, message = "Request completed successfully", lastInsertedId = null };
                                }

                                transaction.Commit();
                                return new Insert { execute = true, message = "Request completed successfully", lastInsertedId = r };
                            }
                        }
                        catch (SqlException e)
                        {
                            transaction.Rollback();
                            return new Insert { execute = false, message = "SqlException. Transaction canceled. " + e.Message, duplicate = (e.Number == 2601 || e.Number == 2627) ? true : false, exception = true };
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            return new Insert { execute = false, message = "Exception. Transaction canceled. " + e.Message, exception = true };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Insert { execute = false, message = "Request failed. " + e.Message };
            }
        }

        /// <summary>
        /// Executes update requests.
        /// </summary>
        /// <param name="queryText">The SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns>Update model</returns>
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

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                if (parameter.Value == null)
                                    command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                else
                                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
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
                return new Update { execute = false, message = "Request failed. " + e.Message };
            }
        }

        /// <summary>
        /// Executes delete requests.
        /// </summary>
        /// <param name="queryText">The SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns>Delete model</returns>
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
                                if (parameter.Value == null)
                                    command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                else
                                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                            }
                        }

                        int affectedRowCount = command.ExecuteNonQuery();
                        return new Delete { execute = true, message = "Request completed successfully!", affectedRowCount = affectedRowCount };
                    }
                }
            }
            catch (Exception e)
            {
                return new Delete { execute = false, message = "Request failed. " + e.Message };
            }
        }

        /// <summary>
        /// Executes any query with out select requests.
        /// </summary>
        /// <param name="queryText">The SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns>Query model</returns>
        public static Query query(string queryText, Dictionary<string, object> parameters = null)
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
                                if (parameter.Value == null)
                                    command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                else
                                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                            }
                        }

                        int r = command.ExecuteNonQuery();

                        return new Query { execute = true, message = "Request completed successfully!", data = r };
                    }
                }
            }
            catch (SqlException e)
            {
                return new Query { execute = false, message = "Request failed. " + e.Message, duplicate = (e.Number == 2601 || e.Number == 2627) ? true : false };
            }
            catch (Exception e)
            {
                return new Query { execute = false, message = "Request failed. " + e.Message };
            }
        }

        /// <summary>
        /// Executes any query with out select requests.
        /// </summary>
        /// <param name="queryText">The SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns>Query model</returns>
        public static Query query(List<string> queryTexts, List<Dictionary<string, object>> parameters = null)
        {
            try
            {
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

                                int index = 0;
                                while (index <= queryTexts.Count - 1)
                                {
                                    command.CommandText = queryTexts[index];

                                    command.Parameters.Clear();

                                    if (parameters != null)
                                    {
                                        if (parameters[index] != null)
                                        {
                                            foreach (KeyValuePair<string, object> parameter in parameters[index])
                                            {
                                                if (parameter.Value == null)
                                                    command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                                else
                                                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                                            }
                                        }
                                    }

                                    command.Transaction = transaction;
                                    command.ExecuteNonQuery();
                                    index++;
                                }

                                transaction.Commit();
                                return new Query { execute = true, message = "Request completed successfully!" };
                            }
                        }
                        catch (SqlException e)
                        {
                            transaction.Rollback();
                            return new Query { execute = false, message = "Request failed. " + e.Message, duplicate = (e.Number == 2601 || e.Number == 2627) ? true : false };
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            return new Query { execute = false, message = "Request failed. " + e.Message };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Query { execute = false, message = "Request failed. " + e.Message };
            }
        }

        /// <summary>
        /// Executes stored procedures, for sellect.
        /// </summary>
        /// <param name="procedureName">Procedure name.</param>
        /// <param name="procedureParameters">Parameters.</param>
        /// <returns>ExecuteSelect model</returns>
        public static ExecuteResult executeSelect(string procedureName, Dictionary<string, object> procedureParameters = null)
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
                                if (parameter.Value == null)
                                    command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                else
                                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();

                                for (int i = 0; i <= reader.FieldCount - 1; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader[reader.GetName(i)];
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

        /// <summary>
        /// Executes stored procedures, with out select requests.
        /// </summary>
        /// <param name="procedureName">Procedure name.</param>
        /// <param name="procedureParameters">Procedure parameters.</param>
        /// <returns>ExecuteQuery model</returns>
        public static ExecuteQuery executeQuery(string procedureName, Dictionary<string, object> procedureParameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = procedureName;

                        if (procedureName != null)
                        {
                            foreach (var parameter in procedureParameters)
                            {
                                if (parameter.Value == null)
                                    command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                else
                                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                            }
                        }

                        int r = command.ExecuteNonQuery();
                        return new ExecuteQuery { execute = true, message = "Request completed successfully!", affectedRowCount = r };
                    }
                }
            }
            catch (SqlException e)
            {
                return new ExecuteQuery { execute = false, message = "SqlException. Request failed. " + e.Message, };
            }
            catch (Exception e)
            {
                return new ExecuteQuery { execute = false, message = "Exception. Request failed. " + e.Message };
            }
        }
    }
}
