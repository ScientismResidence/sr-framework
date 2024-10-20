using Framework;
using Framework.Logger;

namespace Infrastructure;

public class SerilogLogger : ILogger
{
    private readonly Serilog.ILogger _logger;
    
    public SerilogLogger(Serilog.ILogger logger)
    {
        this._logger = logger;
    }
    
    public void Log(string message)
    {
        this.log(message, null, LogLevel.Information);
    }

    public void Log(string message, Exception exception)
    {
        this.log(message, exception, LogLevel.Error);
    }

    public void Log(string message, LogLevel level)
    {
        this.log(message, null, level);
    }
    
    public void Log(string message, Exception exception, LogLevel level, params string[] tags)
    {
        this.log(message, exception, level, tags);
    }

    private void log(string message, Exception exception, LogLevel level, params string[] tags)
    {
        string messageTemplate = tags.Length > 0 ? "{Message} Tags:{@Tags}" : "{Message}";
        
        // By level log into the proper channel
        switch (level)
        {
            case LogLevel.Information:
                _logger.Information(exception, messageTemplate, message, tags);
                break;
            case LogLevel.Debug:
                _logger.Debug(exception, messageTemplate, message, tags);
                break;
            case LogLevel.Warning:
                _logger.Warning(exception, messageTemplate, message, tags);
                break;
            case LogLevel.Error:
                _logger.Error(exception, messageTemplate, message, tags);
                break;
            case LogLevel.Fatal:
                _logger.Fatal(exception, messageTemplate, message, tags);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }
}