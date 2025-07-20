using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSServices.Plugins
{
    public interface ICustomPlugin
    {
        PluginResult Execute(PluginObject obj);
    }
}
