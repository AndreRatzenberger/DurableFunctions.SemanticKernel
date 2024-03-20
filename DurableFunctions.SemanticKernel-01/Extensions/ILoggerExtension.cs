using System.Runtime.CompilerServices;
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
        //LogInformation extention
        public static void LogInformationWithMetadata(this ILogger logger, string message, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            var id = LoggerConfiguration.GetId();
            var className = GetClassNameFromFilePath(filePath);

            logger.LogInformation($"# {id} - {className} - {methodName} - {message}");
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

