namespace Infrastructure;

public class SerilogLogger(Serilog.ILogger logger) : Framework.Logger.ILogger
{
    public void Log(string message)
    {
        log(message, null, Framework.Logger.LogLevel.Information);
    }

    public void Log(string message, Exception exception)
    {
        log(message, exception, Framework.Logger.LogLevel.Error);
    }

    public void Log(string message, Framework.Logger.LogLevel level)
    {
        log(message, null, level);
    }
    
    public void Log(string message, Exception exception, Framework.Logger.LogLevel level, params string[] tags)
    {
        log(message, exception, level, tags);
    }

    private void log(string message, Exception exception, Framework.Logger.LogLevel level, params string[] tags)
    {
        string messageTemplate = tags.Length > 0 ? "{Message} Tags:{@Tags}" : "{Message}";
        
        // By level log into the proper channel
        switch (level)
        {
            case Framework.Logger.LogLevel.Information:
                logger.Information(exception, messageTemplate, message, tags);
                break;
            case Framework.Logger.LogLevel.Debug:
                logger.Debug(exception, messageTemplate, message, tags);
                break;
            case Framework.Logger.LogLevel.Warning:
                logger.Warning(exception, messageTemplate, message, tags);
                break;
            case Framework.Logger.LogLevel.Error:
                logger.Error(exception, messageTemplate, message, tags);
                break;
            case Framework.Logger.LogLevel.Fatal:
                logger.Fatal(exception, messageTemplate, message, tags);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }
}