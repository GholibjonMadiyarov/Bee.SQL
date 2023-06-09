﻿using Bee.SQL.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Bee.SQL
{
    public class SQL
    {
        /// <summary>
        /// Used to retrieve data from a database.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns> Select model. The response is returned as a list of dictionary type.</returns>
        public static Select select(string connectionString, string queryText, Dictionary<string, object> parameters = null)
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

                return new Select { execute = true, message = "Request completed successfully", result = rows };
            }
            catch(Exception e)
            {
                return new Select { execute = false, message = "Request failed. " + e.Message, result = new List<Dictionary<string, string>>() };
            }
        }

        /// <summary>
        /// Used to retrieve data from a database.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns> SelectItem model. The first line is returned as a dictionary.</returns>
        public static SelectItem selectItem(string connectionString, string queryText, Dictionary<string, object> parameters = null)
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
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader[reader.GetName(i)].ToString();
                                }
                            }
                        }
                    }
                }

                return new SelectItem { execute = true, message = "Request completed successfully", result = row };
            }
            catch(Exception e)
            {
                return new SelectItem { execute = false, message = "Request failed. " + e.Message , result = new Dictionary<string, string>() };
            }
        }

        /// <summary>
        /// Used to retrieve data from a database.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns> SelectOne model. The first column of the first row is returned.</returns>
        public static SelectOne selectOne(string connectionString, string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
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
                                return new SelectOne { execute = true, message = "Request completed successfully", result = reader.IsDBNull(0) ? null : reader[0].ToString() };
                            }
                        }
                    }
                }
                return new SelectOne { execute = false, message = "The request was successful, but no result was returned", result = null };
            }
            catch(Exception e)
            {
                return new SelectOne { execute = false, message = "Request failed. " + e.Message, result = null };
            }
        }

        /// <summary>
        /// Requests (update, insert, delete) are executed via transactions.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="queryTexts">The SQL querys is represented as a list.</param>
        /// <param name="parameters">Parameters are given accordingly for each request.</param>
        /// <returns>Query model</returns>
        public static Query query(string connectionString, List<string> queryTexts, List<Dictionary<string, object>> parameters = null)
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

                                return new Query { execute = true, message = "Request completed successfully"};
                            }
                        }
                        catch(Exception e)
                        {
                            transaction.Rollback();
                            return new Query { execute = false, message = "Transaction canceled. " + e.Message };
                        }
                    }
                }
            }
            catch(Exception e)
            {
                return new Query { execute = false, message = "Request failed. " + e.Message };
            }
        }


        /// <summary>
        /// Executes update, insert, delete requests.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="queryTexts">The SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns>Query model</returns>
        public static Query query(string connectionString, string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
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

                        int result = command.ExecuteNonQuery();

                        if(result > 0)
                            return new Query { execute = true, message = "Request completed successfully! Number of changes:" + result };
                        else
                            return new Query { execute = false, message = "The request was completed successfully, but the database did not change" };
                    }
                }
            }
            catch(Exception e)
            {
                return new Query { execute = true, message = "Request failed. " + e.Message };
            }
        }
    }
}
