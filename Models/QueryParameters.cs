using System.Collections.Generic;

namespace VSServices.Plugins
{
    public class QueryParameters
    {
        public string TableName { get; set; }
        public string Columns { get; set; }
        public string Token { get; set; }
        public string Filter { get; set; } = "";
        public string OrderBy { get; set; } = "";
        public int Limit { get; set; } = 100000;
        public string JoinClause { get; set; } = "";
        public string GroupBy { get; set; } = "";
        public string HavingClause { get; set; } = "";
        public Dictionary<string, object> Parameters { get; set; } = null;
    }
}
