using System.Diagnostics;

namespace YourJournal.Services;

/// <summary>
/// Centralized logging service for application-wide error tracking and debugging.
/// Implements structured logging with different severity levels.
/// </summary>
public class LoggingService
{
    #region Constants
    
    private const string LogFileName = "yourjournal.log";
    private readonly string _logFilePath;
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// Initializes logging service with file path in app data directory.
    /// </summary>
    public LoggingService()
    {
        _logFilePath = Path.Combine(FileSystem.AppDataDirectory, LogFileName);
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="context">Optional context information</param>
    public void LogInfo(string message, string? context = null)
    {
        WriteLog("INFO", message, context);
    }
    
    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The warning message</param>
    /// <param name="context">Optional context information</param>
    public void LogWarning(string message, string? context = null)
    {
        WriteLog("WARN", message, context);
    }
    
    /// <summary>
    /// Logs an error with exception details.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="context">Optional context information</param>
    public void LogError(string message, Exception? exception = null, string? context = null)
    {
        var fullMessage = exception != null 
            ? $"{message} | Exception: {exception.Message} | StackTrace: {exception.StackTrace}"
            : message;
        WriteLog("ERROR", fullMessage, context);
    }
    
    /// <summary>
    /// Logs a debug message (only in debug builds).
    /// </summary>
    /// <param name="message">The debug message</param>
    /// <param name="context">Optional context information</param>
    [Conditional("DEBUG")]
    public void LogDebug(string message, string? context = null)
    {
        WriteLog("DEBUG", message, context);
    }
    
    /// <summary>
    /// Retrieves recent log entries from the log file.
    /// </summary>
    /// <param name="lineCount">Number of recent lines to retrieve</param>
    /// <returns>Array of recent log lines</returns>
    public async Task<string[]> GetRecentLogsAsync(int lineCount = 50)
    {
        try
        {
            if (!File.Exists(_logFilePath))
                return Array.Empty<string>();
            
            var lines = await File.ReadAllLinesAsync(_logFilePath);
            return lines.Skip(Math.Max(0, lines.Length - lineCount)).ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
    
    /// <summary>
    /// Clears the log file.
    /// </summary>
    public void ClearLogs()
    {
        try
        {
            if (File.Exists(_logFilePath))
                File.Delete(_logFilePath);
        }
        catch { }
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Writes a formatted log entry to the log file.
    /// </summary>
    /// <param name="level">Log level (INFO, WARN, ERROR, DEBUG)</param>
    /// <param name="message">The message to log</param>
    /// <param name="context">Optional context information</param>
    private void WriteLog(string level, string message, string? context)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var contextInfo = !string.IsNullOrWhiteSpace(context) ? $" [{context}]" : "";
            var logEntry = $"{timestamp} | {level,-5}{contextInfo} | {message}{Environment.NewLine}";
            
            // Write to debug console
            Debug.WriteLine(logEntry.TrimEnd());
            
            // Append to log file
            File.AppendAllText(_logFilePath, logEntry);
            
            // Rotate log file if it gets too large (>5MB)
            var fileInfo = new FileInfo(_logFilePath);
            if (fileInfo.Exists && fileInfo.Length > 5 * 1024 * 1024)
            {
                RotateLogFile();
            }
        }
        catch
        {
            // Fail silently to avoid recursive logging errors
        }
    }
    
    /// <summary>
    /// Rotates log file by renaming current file and starting a new one.
    /// </summary>
    private void RotateLogFile()
    {
        try
        {
            var backupPath = $"{_logFilePath}.{DateTime.Now:yyyyMMdd-HHmmss}.bak";
            File.Move(_logFilePath, backupPath);
        }
        catch { }
    }
    
    #endregion
}
