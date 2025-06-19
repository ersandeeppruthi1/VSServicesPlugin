using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSServices.Plugins
{
    public interface IPlugin
    {
        bool Execute(PluginObject obj);
    }


}
