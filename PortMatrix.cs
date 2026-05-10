using System.Text;

namespace StudentGroupManagement2;

public class PortMatrix
{
    private const int SIZE = 16;
    private Port[,] _matrix;
    private bool _initialized = false;
    private PortLogger _logger;

    public bool IsInitialized => _initialized;
    public int Size => SIZE;

    public PortMatrix(PortLogger logger)
    {
        _matrix = new Port[SIZE, SIZE];
        _logger = logger;
    }

    public void Initialize()
    {
        Port.ResetIdCounter();
        string[] deviceTypes = { "Keyboard", "Mouse", "Monitor", "HDD", "USB-Hub", "Webcam",
                                  "Printer", "Scanner", "Speaker", "Sensor", "NetCard", "GPU",
                                  "RAM-Ctrl", "LED-Panel", "Servo", "ADC" };

        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                string device = deviceTypes[(r + c) % deviceTypes.Length] + $"_{r:D2}{c:D2}";
                _matrix[r, c] = new Port(r, c, device);
            }
        }

        _initialized = true;
        _logger.LogOperation("INIT", -1, $"Матрицю {SIZE}x{SIZE} ініціалізовано — {SIZE * SIZE} портів.");
        Console.WriteLine($"  ✓ Матрицю {SIZE}×{SIZE} ініціалізовано ({SIZE * SIZE} портів).");
    }

    private void CheckInit()
    {
        if (!_initialized)
            throw new InvalidOperationException("Матриця не ініціалізована! Спочатку виконайте ініціалізацію (пункт 3).");
    }

    private void CheckBounds(int row, int col)
    {
        if (row < 0 || row >= SIZE || col < 0 || col >= SIZE)
            throw new IndexOutOfRangeException($"Координати [{row},{col}] виходять за межі матриці (0–{SIZE - 1}).");
    }

    public Port GetPort(int row, int col)
    {
        CheckInit();
        CheckBounds(row, col);
        return _matrix[row, col];
    }

    public void OpenPort(int row, int col)
    {
        CheckInit();
        CheckBounds(row, col);
        _matrix[row, col].Open();
        _logger.LogOperation("OPEN", _matrix[row, col].PortNumber, $"Порт [{row},{col}] ({_matrix[row, col].DeviceName}) відкрито.");
    }

    public void ClosePort(int row, int col)
    {
        CheckInit();
        CheckBounds(row, col);
        _matrix[row, col].Close();
        _logger.LogOperation("CLOSE", _matrix[row, col].PortNumber, $"Порт [{row},{col}] ({_matrix[row, col].DeviceName}) закрито.");
    }

    public void WriteToPort(int row, int col, byte[] data)
    {
        CheckInit();
        CheckBounds(row, col);
        _matrix[row, col].Write(data);
        _logger.LogOperation("WRITE", _matrix[row, col].PortNumber,
            $"Порт [{row},{col}]: записано {data.Length} байт.");
    }

    public byte[] ReadFromPort(int row, int col)
    {
        CheckInit();
        CheckBounds(row, col);
        byte[] data = _matrix[row, col].Read();
        _logger.LogOperation("READ", _matrix[row, col].PortNumber,
            $"Порт [{row},{col}]: прочитано {data.Length} байт.");
        return data;
    }

    public List<Port> ScanMatrix()
    {
        CheckInit();
        var openPorts = new List<Port>();
        for (int r = 0; r < SIZE; r++)
            for (int c = 0; c < SIZE; c++)
                if (_matrix[r, c].IsOpen)
                    openPorts.Add(_matrix[r, c]);

        _logger.LogOperation("SCAN", -1, $"Сканування: знайдено {openPorts.Count} відкритих портів.");
        return openPorts;
    }

    // Find all open ports of a specific device name (search in 2D matrix)
    public List<Port> FindOpenPortsByDevice(string deviceNamePart)
    {
        CheckInit();
        var found = new List<Port>();
        for (int r = 0; r < SIZE; r++)
            for (int c = 0; c < SIZE; c++)
                if (_matrix[r, c].IsOpen &&
                    _matrix[r, c].DeviceName.Contains(deviceNamePart, StringComparison.OrdinalIgnoreCase))
                    found.Add(_matrix[r, c]);
        return found;
    }

    public int GetOpenPortCount()
    {
        if (!_initialized) return 0;
        int count = 0;
        for (int r = 0; r < SIZE; r++)
            for (int c = 0; c < SIZE; c++)
                if (_matrix[r, c].IsOpen) count++;
        return count;
    }

    public void PrintMatrix()
    {
        CheckInit();
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("  МАТРИЦЯ ПОРТІВ 16×16");
        sb.AppendLine("  " + new string('═', 68));

        // Header row (cols 0-15)
        sb.Append("       ");
        for (int c = 0; c < SIZE; c++)
            sb.Append($" {c,2}");
        sb.AppendLine();
        sb.AppendLine("  " + new string('─', 68));

        for (int r = 0; r < SIZE; r++)
        {
            sb.Append($"  [{r,2}]  ");
            for (int c = 0; c < SIZE; c++)
            {
                bool open = _matrix[r, c].IsOpen;
                bool hasStudent = _matrix[r, c].AssignedStudentId != null;
                char symbol = open ? (hasStudent ? 'S' : '▪') : '·';
                sb.Append($"  {symbol}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("  " + new string('─', 68));
        sb.AppendLine("  Легенда: · = закрито   ▪ = відкрито   S = студент");
        sb.AppendLine($"  Відкритих портів: {GetOpenPortCount()} / {SIZE * SIZE}");
        sb.AppendLine("  " + new string('═', 68));

        Console.Write(sb);
    }

    public void AssignStudent(int row, int col, string recordBookNumber)
    {
        CheckInit();
        CheckBounds(row, col);
        _matrix[row, col].AssignedStudentId = recordBookNumber;
        _logger.LogOperation("ASSIGN", _matrix[row, col].PortNumber,
            $"Студент {recordBookNumber} прив'язаний до порту [{row},{col}].");
    }
}
