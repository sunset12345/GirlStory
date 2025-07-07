namespace GSDev.CSVConfig.Editor
{
    public interface IHandler
    {
        void DoHandler( string file );
    }

    public interface IFileChecker
    {
        bool OpenSheet(string file);
        bool Check();
        void GenerateReader(string outputPath);
    }

    public interface ILogger
    {
        void LogInfo(string info);
        void LogWarning(string warning);
        void LogError(string error);
    }

    public class ParseHandler :IHandler
    {
        private readonly ILogger _logger;
        public ParseHandler( ILogger logger )
        {
            _logger = logger;
        }
        public void DoHandler( string file )
        {
            var checker = new TableFileChecker(_logger);
            if (!checker.OpenSheet(file))
                return;

            if (!checker.Check())
            {
                return;
            }

            _logger.LogInfo($"{file} 检测完成".ToString());
        }
    }

    public class GenerateHandler : IHandler
    {
        private readonly ILogger _logger;
        private readonly string _outputPath;
        
        public GenerateHandler(ILogger logger, string outputPath)
        {
            _logger = logger;
            _outputPath = outputPath;
        }
        public void DoHandler( string file )
        {
            var checker = new TableFileChecker(_logger);
            if (!checker.OpenSheet(file))
                return;

            if (!checker.Check())
            {
                return;
            }

            checker.GenerateReader(_outputPath);

            _logger.LogInfo($"{file}->{_outputPath}完成");
        }
    }

    public class ConstGenerateHandler : IHandler
    {
        private readonly ILogger _logger;
        private readonly string _outputPath;
        
        public ConstGenerateHandler(ILogger logger, string outputPath)
        {
            _logger = logger;
            _outputPath = outputPath;
        }
        public void DoHandler( string file )
        {
            var checker = new ConstFileChecker(_logger);
            if (!checker.OpenSheet(file))
                return;

            if (!checker.Check())
            {
                return;
            }

            checker.GenerateReader(_outputPath);

            _logger.LogInfo($"{file}->{_outputPath}完成");
        }
    }

    public static class ScriptGenerator
    {
        public static ILogger Logger;
        public static void GenerateTable(string filePath, string outputPath)
        {
            var handler = new GenerateHandler(Logger, outputPath);
            handler.DoHandler(filePath);
        }

        public static void GenerateConst(string filePath, string outputPath)
        {
            var handler = new ConstGenerateHandler(Logger, outputPath);
            handler.DoHandler(filePath);
        }
    }
}
