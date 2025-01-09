namespace Framework.Logger;

public interface ILogger
{
    void Log(string message);

    void Log(string message, Exception exception);
    
    void Log(string message, LogLevel level);
    
    void Log(string message, Exception exception, LogLevel level, params string[] tags);
}

public interface IRootLogger : ILogger;