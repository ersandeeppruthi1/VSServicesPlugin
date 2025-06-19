using System;
using System.Collections.Generic;

namespace VSServices.Plugins
{
    public class RecordInsertModel
    {
        public RecordInsertModel() {
            this.DeletedColumnValues = new Dictionary<string, object>();
        }
        public string Token { get; set; }
        public string TableName { get; set; }
        public string Id { get; set; }
        public List<string> Ids { get; set; }

        public Dictionary<string, object> ColumnValues { get; set; }
        public Dictionary<string, object> UpdatedColumnValues { get; set; }
        public Dictionary<string, object> DeletedColumnValues { get; set; }
        public Dictionary<string, List<string>> Attachments { get; set; } // Added attachments

        /// <summary>
        /// Fetches the value of the specified column from the appropriate dictionary.
        /// </summary>
        /// <param name="columnName">The name of the column whose value is to be fetched.</param>
        /// <returns>The value of the column as a string or null if not found.</returns>
        public string GetColumnValue(string columnName)
        {
            // Determine which dictionary to use
            var columnData = DeletedColumnValues?.Count > 0 ? DeletedColumnValues : ColumnValues;

            if (columnData == null || !columnData.ContainsKey(columnName))
            {
                return null; // Return null if the column does not exist
            }

            return columnData[columnName]?.ToString();
        }

        public T GetColumnValue<T>(string columnName)
        {
            var columnData = DeletedColumnValues?.Count > 0 ? DeletedColumnValues : ColumnValues;

            if (columnData == null || !columnData.ContainsKey(columnName))
            {
                return default; // Return default value for the type
            }

            try
            {
                return (T)Convert.ChangeType(columnData[columnName], typeof(T));
            }
            catch
            {
                return default; // Return default value if conversion fails
            }
        }

    }
}
