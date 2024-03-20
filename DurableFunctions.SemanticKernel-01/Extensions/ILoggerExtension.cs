using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;


namespace DurableFunctions.SemanticKernel.Ex01.Extensions
{
    //Provides LogInformationWithMetadata and LogErrorWithMetadata extensions
    //These extensions will add "# {ID} - {CLASS_NAME} - {METHOD_NAME} - " before the message
    //for example log.LogInformationWithMetadata("Orchestrator started") will result in
    //"# DURABLE AI - SemanticKernel - SemanticKernelOrchestrator - Orchestrator started"
    //makes it easier to search for specific logs in the log stream/appinsights
    //See Program.cs how to CoçnfigureLogger

    public static class LoggerConfiguration
    {
        private static string? _id;

        public static void ConfigureLogger(string id)
        {
            _id = id;
        }

        internal static string? GetId() => _id;
    }

    public static class LoggerExtensions
    {
        private static readonly HttpClient httpClient = new();

        public static async Task LogToExternWithMetadataAsync(this ILogger logger, string message, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            var id = LoggerConfiguration.GetId();
            var className = GetClassNameFromFilePath(filePath);

            var msg = $"# {id} - {className} - {methodName} - {message}";

            await SendLogToExternal(msg, logger);
        }

        public static async Task LogToExternAsync(this ILogger logger, string message)
        {
            await SendLogToExternal(message, logger);
        }

        private static async Task SendLogToExternal(string msg, ILogger logger)
        {
            try
            {
                var endpoint = Environment.GetEnvironmentVariable("ExternalLogEndpoint");
                if (endpoint != null)
                {
                    var content = new StringContent(msg, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(endpoint, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogErrorWithMetadata($"Failed to log to {endpoint}: {response.StatusCode}");
                    }
                }
            }
            catch
            {
                //logger.LogErrorWithMetadata($"Failed to log to external log");
            }
        }


        //LogInformation extention
        public static void LogInformationWithMetadata(this ILogger logger, string message, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            var id = LoggerConfiguration.GetId();
            var className = GetClassNameFromFilePath(filePath);

            logger.LogInformation($"# {id} - {className} - {methodName} - {message}");

            bool.TryParse(Environment.GetEnvironmentVariable("UseExternalLog"), out var isExternalLog);
            if (isExternalLog)
            {
                logger.LogInformation($"# {id} - {className} - {methodName} - {message}");
            }
        }

        //for durable function - based on isReplay flag do the logging or not
        public static void LogInformationWithMetadata(this ILogger logger, string message, bool doLog, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            if (doLog)
            {
                var id = LoggerConfiguration.GetId();
                var className = GetClassNameFromFilePath(filePath);

                logger.LogInformation($"# {id} - {className} - {methodName} - {message}");
            }
        }

        //LogError extention - with exception
        public static void LogErrorWithMetadata(this ILogger logger, Exception ex, string message, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            var id = LoggerConfiguration.GetId();
            var className = GetClassNameFromFilePath(filePath);

            logger.LogError(ex, $"# {id} - {className} - {methodName} - {message}");
        }

        //for durable function - based on isReplay flag do the logging or not
        public static void LogErrorWithMetadata(this ILogger logger, Exception ex, string message, bool doLog, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            if (doLog)
            {
                var id = LoggerConfiguration.GetId();
                var className = GetClassNameFromFilePath(filePath);
                logger.LogError(ex, $"# {id} - {className} - {methodName} - {message}");
            }
        }

        //LogError extention - without exception
        public static void LogErrorWithMetadata(this ILogger logger, string message, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            var id = LoggerConfiguration.GetId();
            var className = GetClassNameFromFilePath(filePath);

            logger.LogError($"# {id} - {className} - {methodName} - {message}");
        }

        //for durable function - based on isReplay flag do the logging or not
        public static void LogErrorWithMetadata(this ILogger logger, string message, bool doLog, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            if (doLog)
            {
                var id = LoggerConfiguration.GetId();
                var className = GetClassNameFromFilePath(filePath);
                logger.LogError($"# {id} - {className} - {methodName} - {message}");
            }
        }

        private static string GetClassNameFromFilePath(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            return fileName;
        }
    }
}

