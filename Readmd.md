
# VSServices.Plugins

**VSServices.Plugins** is a .NET library designed to simplify plugin-based CRUD operations on MySQL databases. It supports dynamic plugin execution, user-based read permissions, and extensible record manipulation.

## 📦 Installation

Install via NuGet:

```bash
Install-Package VSServices.Plugins
```

Or via `.csproj`:

```xml
<PackageReference Include="VSServices.Plugins" Version="1.0.0" />
```

## 🚀 Features

- ✅ Execute custom plugin logic dynamically from external DLLs
- ✅ Create, read, update records in MySQL with ease
- ✅ Enforce read permissions (`Self`, `BusinessUnit`, `Global`)
- ✅ Parameterized query execution for safety and performance
- ✅ Supports attachments and deleted records for audit trails
- ✅ Plugin-based architecture for flexible data workflows

## 🧩 Key Components

### `BasePlugin`

Provides reusable CRUD operations:

- `ExecuteNonQuery`: Run raw SQL commands
- `GetRecordById`: Fetch record with permission check
- `UpdateRecord`: Update fields of a record
- `CreateRecord`: Insert new records
- `ExecuteQueryWithPermissions`: Query with permission-based filters

### `PluginObject`

Encapsulates the context for a plugin call:

```csharp
var pluginObject = new PluginObject(connection, model, userId, businessUnitId, userName);
pluginObject.Permissions = GetUserPermissions(); // List<UserPermission>
pluginObject.Execute(plugins); // plugins is List<EntityPlugin>
```

### `RecordInsertModel`

Contains metadata for insert/update/delete operations, including:
- `TableName`, `Ids`, `ColumnValues`, `UpdatedColumnValues`
- `Attachments`
- Utility methods like `GetColumnValue<T>()`

### `IPlugin`

Interface to implement your own plugin:

```csharp
public class MyCustomPlugin : BasePlugin, IPlugin
{
    public bool Execute(PluginObject obj)
    {
        // custom logic
        return true;
    }
}
```

### `UserPermission`

Used to apply permission filters at runtime based on `ReadPermission` levels:
- `-1`: No access
- `1`: Self (CreatedById)
- `2`: BusinessUnit

## 📂 Folder Structure

```plaintext
VSServices.Plugins/
├── BasePlugin.cs
├── IPlugin.cs
├── PluginObject.cs
├── RecordInsertModel.cs
├── User.cs
├── UserPermission.cs
```

## 🔐 Permissions

Read permissions are enforced using:
- `ApplyReadPermissions` (for single record)
- `ApplyQueryPermissions` (for multiple records)

They dynamically append filters like:

```sql
WHERE CreatedById = @UserId
```

## 🧪 Example Plugin Execution

```csharp
var plugins = new List<EntityPlugin>
{
    new EntityPlugin { AssemblyName = "MyPlugins.dll", ClassName = "MyCustomPlugin" }
};

var pluginObj = new PluginObject(connection, recordId, userId)
{
    Permissions = userPermissions
};

pluginObj.Execute(plugins);
```

## 🛠️ Requirements

- .NET 6 or later
- MySqlConnector >= 2.0

## 📄 License

MIT License.
