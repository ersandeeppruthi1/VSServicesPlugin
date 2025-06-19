using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSServices.Plugins
{
    public class UserPermission
    {
        public string UserId { get; set; }               // ID of the user
        public string BusinessUnitId { get; set; }        // ID of the business unit
        public int ReadPermission { get; set; }        // Read permission (0 or 1)
        public int WritePermission { get; set; }       // Write permission (0 or 1)
        public int DeletePermission { get; set; }      // Delete permission (0 or 1)
        public string EntityName { get; set; }         // Name of the entity the permission applies to
    }
}
