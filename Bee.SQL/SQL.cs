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
        /// <returns>The response is returned as a list of dictionary type.</returns>
        public static List<Dictionary<string, string>> select(string connectionString, string queryText, Dictionary<string, object> parameters = null)
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

                return rows;
            }
            catch
            {
                return new List<Dictionary<string, string>>();
            }
        }

        /// <summary>
        /// Used to retrieve data from a database.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns>The first line is returned as a dictionary.</returns>
        public static Dictionary<string, string> selectItem(string connectionString, string queryText, Dictionary<string, object> parameters = null)
        {
            try
            {
                Dictionary<string, string> row = new Dictionary<string, string>();

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

                return row;
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Used to retrieve data from a database.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="queryText">SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns>The first column of the first row is returned.</returns>
        public static string selectOne(string connectionString, string queryText, Dictionary<string, object> parameters = null)
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
                                return reader.IsDBNull(0) ? null : reader[0].ToString();
                            }
                        }
                    }
                }
                return "";
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Requests (update, insert, delete) are executed via transactions.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="queryTexts">The SQL querys is represented as a list.</param>
        /// <param name="parameters">Parameters are given accordingly for each request.</param>
        /// <returns>If the request is successful, 1 is returned, otherwise 0 is returned.</returns>
        public static int query(string connectionString, List<string> queryTexts, List<Dictionary<string, object>> parameters = null)
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
                                        foreach (var parameter in parameters[index])
                                        {
                                            if (parameters[index] == null)
                                                command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                                            else
                                                command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                                        }
                                    }

                                    command.Transaction = transaction;
                                    command.ExecuteNonQuery();
                                    index++;
                                }

                                transaction.Commit();

                                return 1;
                            }
                        }
                        catch
                        {
                            transaction.Rollback();

                            return 0;
                        }
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Executes update, insert, delete requests.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="queryTexts">The SQL query.</param>
        /// <param name="parameters">Parameters.</param>
        /// <returns>If the request is successful, 1 is returned, otherwise 0 is returned.</returns>
        public static int query(string connectionString, string queryText, Dictionary<string, object> parameters)
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

                        if (result > 0)
                            return 1;

                        return 0;
                    }
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
