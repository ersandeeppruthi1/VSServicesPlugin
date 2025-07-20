namespace VSServices.Plugins
{
    /// <summary>
    /// Represents the result of executing a plugin, including success state, message, data, and cache duration.
    /// </summary>
    public class PluginResult
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PluginResult"/> with default success = true.
        /// </summary>
        public PluginResult()
        {
            Success = true;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PluginResult"/> with a message.
        /// </summary>
        public PluginResult(string message)
        {
            Success = true;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PluginResult"/> with data.
        /// </summary>
        public PluginResult(string data, bool isData)
        {
            Success = true;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PluginResult"/> with message and data.
        /// </summary>
        public PluginResult(string message, string data)
        {
            Success = true;
            Message = message;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PluginResult"/> with message, data, and success state.
        /// </summary>
        public PluginResult(string message, string data, bool success)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PluginResult"/> with message, data, success state, and cache time.
        /// </summary>
        public PluginResult(string message, string data, bool success, int cacheTime)
        {
            Success = success;
            Message = message;
            Data = data;
            CacheTime = cacheTime;
        }

        /// <summary>
        /// Gets or sets whether the plugin execution was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message returned by the plugin.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets additional data returned by the plugin.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the cache time in minutes for the plugin result.
        /// </summary>
        public int CacheTime { get; set; }
    }
}
