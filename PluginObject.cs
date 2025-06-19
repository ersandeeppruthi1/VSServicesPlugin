using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VSServices.Plugins
{
    public class PluginObject
    {
        // Default constructor
        public PluginObject() { }

        // Constructor with Connection, RecordId, and UserId parameters
        public PluginObject(MySqlConnection connection, string recordId, string userId)
        {
            Connection = connection;
            RecordId = recordId;
            UserId = userId;
        }

        // New constructor with full parameter list for initializing all properties
        public PluginObject(MySqlConnection connection, RecordInsertModel model, string userId, string bu, string userName)
        {
            Connection = connection;
            Model = model;
            UserId = userId;
            BU = bu;
            UserName = userName;
        }

        // Properties
        public MySqlConnection Connection { get; set; }
        public RecordInsertModel Model { get; set; }
        public string RecordId { get; set; }
        public string UserId { get; set; }
        public string BU { get; set; }
        public string UserName { get; set; }

        public List<UserPermission> Permissions { get; set; }

        public void Execute(List<EntityPlugin> plugins)
        {
            foreach (var plugin in plugins)
            {
                // Dynamically load and execute the plugin
                string pluginAssemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", plugin.AssemblyName);

                // Load the assembly
                var assembly = Assembly.LoadFrom(pluginAssemblyPath);

                // Find the VastuItemPlugin type
                var pluginType = assembly.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && t.Name == plugin.ClassName);

                // Create an instance of the plugin and execute it
                var pluginInstance = (IPlugin)Activator.CreateInstance(pluginType);
                pluginInstance.Execute(this);
            }
        }
    }
}

