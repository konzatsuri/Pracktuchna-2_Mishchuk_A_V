using System.Text;

namespace StudentGroupManagement2;

public class PortLogger
{
    private readonly StringBuilder _logBuilder;
    private readonly string _logFilePath;

    public PortLogger()
    {
        _logBuilder = new StringBuilder();
        _logFilePath = "port_operations.log";
        _logBuilder.AppendLine($"--- Log started at {DateTime.Now:dd.MM.yyyy HH:mm:ss} ---");
    }

    public void LogOperation(string operation, int portNumber, string details)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string portStr = portNumber >= 0 ? $"Port #{portNumber:D3}" : "SYSTEM   ";
        
        // Using StringBuilder for efficient string construction as required
        string entry = $"[{timestamp}] | {operation,-8} | {portStr} | {details}";
        
        _logBuilder.AppendLine(entry);
        
        // Also print to console for visibility
        Console.WriteLine($"  [LOG] {entry}");
    }

    public string GetFullLog()
    {
        return _logBuilder.ToString();
    }

    public void SaveLogToFile()
    {
        try
        {
            File.AppendAllText(_logFilePath, _logBuilder.ToString());
            _logBuilder.Clear();
            _logBuilder.AppendLine($"--- Log rotated at {DateTime.Now:dd.MM.yyyy HH:mm:ss} ---");
            Console.WriteLine($"  ✓ Лог успішно збережено у файл: {_logFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠ Помилка збереження логу: {ex.Message}");
        }
    }

    public void ClearLog()
    {
        _logBuilder.Clear();
        _logBuilder.AppendLine($"--- Log cleared at {DateTime.Now:dd.MM.yyyy HH:mm:ss} ---");
    }
}
