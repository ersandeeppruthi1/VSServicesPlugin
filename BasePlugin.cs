using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSServices.Plugins
{
    public class BasePlugin
    {
        /// <summary>
        /// Executes a non-query SQL command (such as INSERT, UPDATE, DELETE) on the database.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="connection">An open MySqlConnection instance.</param>
        protected void ExecuteNonQuery(string sql, MySqlConnection connection)
        {
            using (var command = new MySqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine("SQL executed successfully.");
            }
        }

        /// <summary>
        /// Retrieves a single record from a specified entity (table) by ID with applied read permissions.
        /// </summary>
        /// <param name="pluginObject">The PluginObject containing user permissions and context.</param>
        /// <param name="tableName">The name of the database entity (table).</param>
        /// <param name="recordId">The ID of the record to retrieve.</param>
        /// <param name="connection">An open MySqlConnection instance.</param>
        /// <returns>A dictionary with column names and values, or null if no record is found.</returns>
        public Dictionary<string, object> GetRecordById(PluginObject pluginObject, string tableName, string recordId, MySqlConnection connection)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return null;

            // Construct the base SQL query
            string query = $"SELECT * FROM {tableName} WHERE Id = @Id";
            query = ApplyReadPermissions(pluginObject, query, tableName);

            // Execute the query and return the single record
            return ExecuteSingleRecordQuery(query, recordId, connection);
        }

        /// <summary>
        /// Applies user-specific read permissions to a SQL query based on the permission level.
        /// </summary>
        /// <param name="pluginObject">The PluginObject with user details and permissions.</param>
        /// <param name="query">The SQL query to which permissions will be applied.</param>
        /// <param name="tableName">The name of the entity for which permissions apply.</param>
        /// <returns>The modified SQL query with appropriate conditions.</returns>
        private string ApplyReadPermissions(PluginObject pluginObject, string query, string tableName)
        {
            var readPermissionLevel = pluginObject.Permissions.First().ReadPermission;

            // Modify the query based on the read permission level
            switch (readPermissionLevel)
            {
                case -1:
                    throw new UnauthorizedAccessException("User does not have read permissions.");

                case 1:
                    query += $" AND CreatedById = '{pluginObject.UserId}'";
                    break;

                case 2:
                    query += $" AND businessunitid = '{pluginObject.BU}'";
                    break;

                    // No additional conditions for permission level 0 or other unspecified levels
            }

            return query;
        }

        /// <summary>
        /// Executes a SQL query to retrieve a single record by ID.
        /// </summary>
        /// <param name="query">The SQL query with placeholders for parameters.</param>
        /// <param name="id">The ID of the record to retrieve.</param>
        /// <param name="connection">An open MySqlConnection instance.</param>
        /// <returns>A dictionary with the record's column names and values, or null if no record is found.</returns>
        private Dictionary<string, object> ExecuteSingleRecordQuery(string query, string id, MySqlConnection connection)
        {
            Dictionary<string, object> record = null;

            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        record = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            object value = reader.GetValue(i);

                            // Handle zero date-time
                            if (value is DateTime dt && dt == DateTime.MinValue)
                            {
                                value = null; // Replace with null or a default value
                            }

                            record[reader.GetName(i)] = value;
                        }
                    }
                }

            }

            return record;
        }

        /// <summary>
        /// Executes a SQL query based on QueryParameters, applying necessary permissions.
        /// </summary>
        /// <param name="queryParams">Query parameters including table, columns, and filters.</param>
        /// <param name="pluginObject">PluginObject containing user permissions and connection context.</param>
        /// <returns>A list of key-value pairs representing rows and columns from the result set.</returns>
        public List<List<KeyValuePair<string, object>>> ExecuteQueryWithPermissions(QueryParameters queryParams, PluginObject pluginObject)
        {
            ApplyQueryPermissions(queryParams, pluginObject.UserId, pluginObject);

            string query = BuildSelectQuery(queryParams);
            return ExecuteQueryWithParameters(query, queryParams.Parameters, pluginObject.Connection);
        }

        /// <summary>
        /// Executes a parameterized SQL query and returns the results.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to apply to the SQL command.</param>
        /// <param name="connection">An open MySqlConnection instance.</param>
        /// <returns>A list of key-value pairs representing rows and columns from the result set.</returns>
        private List<List<KeyValuePair<string, object>>> ExecuteQueryWithParameters(string query, Dictionary<string, object> parameters, MySqlConnection connection)
        {
            var results = new List<List<KeyValuePair<string, object>>>();

            using (var cmd = new MySqlCommand(query, connection))
            {
                // Add query parameters
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new List<KeyValuePair<string, object>>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(new KeyValuePair<string, object>(reader.GetName(i), reader.GetValue(i)));
                        }
                        results.Add(row);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Adds appropriate filter conditions to query parameters based on read permissions.
        /// </summary>
        /// <param name="queryParams">Query parameters to apply filters on.</param>
        /// <param name="userId">User ID for permission check.</param>
        /// <param name="pluginObject">PluginObject containing user permissions and connection context.</param>
        private void ApplyQueryPermissions(QueryParameters queryParams, string userId, PluginObject pluginObject)
        {
            if (queryParams.Parameters == null)
            {
                queryParams.Parameters = new Dictionary<string, object>();
            }

            bool hasAccess = false;
            foreach (var permission in pluginObject.Permissions)
            {
                switch (permission.ReadPermission)
                {
                    case -1:
                        AddFilterCondition(queryParams, "1=2");
                        break;

                    case 1:
                        AddFilterCondition(queryParams, $"{queryParams.TableName}.CreatedById = @UserId");
                        queryParams.Parameters["@UserId"] = userId;
                        hasAccess = true;
                        break;

                    case 2:
                        var businessUnits = pluginObject.Permissions.Where(p => p.ReadPermission == 2).Select(p => p.BusinessUnitId).ToList();
                        var buPlaceholders = string.Join(", ", businessUnits.Select((id, index) => $"@BU{index}"));
                        AddFilterCondition(queryParams, $"{queryParams.TableName}.businessunitid IN ({buPlaceholders})");

                        for (int i = 0; i < businessUnits.Count; i++)
                        {
                            queryParams.Parameters[$"@BU{i}"] = businessUnits[i];
                        }

                        hasAccess = true;
                        break;
                }
                break;
            }

            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("User does not have the required read permissions.");
            }
        }

        /// <summary>
        /// Appends a filter condition to the query parameters.
        /// </summary>
        /// <param name="queryParams">Query parameters to which the condition will be added.</param>
        /// <param name="condition">The filter condition as a SQL string.</param>
        private void AddFilterCondition(QueryParameters queryParams, string condition)
        {
            if (!string.IsNullOrWhiteSpace(queryParams.Filter))
            {
                queryParams.Filter += " AND ";
            }
            queryParams.Filter += condition;
        }

        /// <summary>
        /// Constructs a SELECT SQL query based on the provided query parameters.
        /// </summary>
        /// <param name="queryParams">Query parameters with table name, columns, filters, and additional clauses.</param>
        /// <returns>The constructed SQL SELECT query as a string.</returns>
        private string BuildSelectQuery(QueryParameters queryParams)
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.Append($"SELECT {queryParams.Columns} FROM {queryParams.TableName}");

            if (!string.IsNullOrWhiteSpace(queryParams.JoinClause))
            {
                queryBuilder.Append($" {queryParams.JoinClause}");
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Filter))
            {
                queryBuilder.Append($" WHERE {queryParams.Filter}");
            }

            if (!string.IsNullOrWhiteSpace(queryParams.GroupBy))
            {
                queryBuilder.Append($" GROUP BY {queryParams.GroupBy}");
            }

            if (!string.IsNullOrWhiteSpace(queryParams.HavingClause))
            {
                queryBuilder.Append($" HAVING {queryParams.HavingClause}");
            }

            if (!string.IsNullOrWhiteSpace(queryParams.OrderBy))
            {
                queryBuilder.Append($" ORDER BY {queryParams.OrderBy}");
            }

            if (queryParams.Limit > 0)
            {
                queryBuilder.Append($" LIMIT {queryParams.Limit}");
            }

            return queryBuilder.ToString();
        }


        /// <summary>
        /// Updates a record in the specified table with the provided values.
        /// </summary>
        /// <param name="tableName">The name of the table to update.</param>
        /// <param name="recordId">The ID of the record to update.</param>
        /// <param name="columnValues">A dictionary of column names and values to update.</param>
        /// <param name="connection">An open MySqlConnection instance.</param>
        public void UpdateRecord(string tableName, string recordId, Dictionary<string, object> columnValues, MySqlConnection connection)
        {
            if (columnValues == null || columnValues.Count == 0)
            {
                throw new ArgumentException("No column values provided for the update.");
            }

            // Build the SET clause with placeholders for each column
            var setClause = string.Join(", ", columnValues.Keys.Select(column => $"{column} = @{column}"));
            var query = $"UPDATE {tableName} SET {setClause} WHERE id = @Id";

            using (var command = new MySqlCommand(query, connection))
            {
                // Add the record ID parameter
                command.Parameters.AddWithValue("@Id", recordId);

                // Add parameters for each column to update
                foreach (var column in columnValues)
                {
                    command.Parameters.AddWithValue($"@{column.Key}", column.Value);
                }

                // Execute the update command
                var rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} rows updated in {tableName} table.");
            }
        }

        public string CreateRecord(PluginObject pluginObject, Dictionary<string, object> columnValues, MySqlConnection connection)
        {
            var query = BuildInsertQuery(pluginObject.Model.TableName, columnValues, pluginObject.UserId, pluginObject.BU);

            var d = ExecuteInsertAndReturnId(query, columnValues, pluginObject.UserId, pluginObject.BU, connection);
            return columnValues["id"].ToString();
        }

        private int ExecuteInsertAndReturnId(string query, Dictionary<string, object> parameters, string id, string bu, MySqlConnection conn)
        {
            query = query + "; SELECT LAST_INSERT_ID();";

            {

                // Create a command using the provided query and connection
                using (var cmd = new MySqlCommand(query, conn))
                {
                    // Add parameters to the command if provided
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            // Use AddWithValue to safely add each parameter to the command
                            cmd.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value ?? DBNull.Value);
                        }
                    }

                    // Add parameters for default columns
                    cmd.Parameters.AddWithValue("@createdbyid", id);
                    if (!parameters.ContainsKey("businessunitid"))
                        cmd.Parameters.AddWithValue("@businessunitid", bu);

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result); // Convert the result to int (record ID)
                }
            }
        }



        private string BuildInsertQuery(string tableName, Dictionary<string, object> columnValues, string userId, string bu)
        {
            // Check if columnValues is null or empty
            if (columnValues == null || columnValues.Count == 0)
            {
                throw new ArgumentException("Column values cannot be null or empty", nameof(columnValues));
            }

            if (!columnValues.ContainsKey("id"))
                columnValues.Add("id", Guid.NewGuid());

            // Filter out any invalid columns (keys that are null or whitespace)
            var validColumns = columnValues
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            // Add the default columns for CreatedById, CreatedByName, and OwningBusinessUnitId
            validColumns.Add("createdbyid", userId);
            if (!validColumns.ContainsKey("businessunitid"))
                validColumns.Add("businessunitid", bu);

            // Build the list of column names
            string columns = string.Join(", ", validColumns.Keys);

            // Build the list of parameter placeholders for the values
            string parameterNames = string.Join(", ", validColumns.Keys.Select(k => $"@{k}"));

            // Construct the INSERT SQL statement
            string query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameterNames})";

            return query;
        }
    }
}
