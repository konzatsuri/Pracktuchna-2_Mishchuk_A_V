namespace StudentGroupManagement2;

// Custom exception for port operations
public class PortException : Exception
{
    public int PortRow { get; }
    public int PortCol { get; }

    public PortException(string message, int row, int col) : base(message)
    {
        PortRow = row;
        PortCol = col;
    }
}

public class Port : ICloneable
{
    private static int _nextId = 1;

    public int PortNumber { get; }
    public int Row { get; }
    public int Col { get; }
    public byte[] DataBuffer { get; private set; } = new byte[64];
    public bool IsOpen { get; private set; }
    public string DeviceName { get; set; }
    public int BytesWritten { get; private set; }
    public DateTime? LastActivity { get; private set; }
    public string? AssignedStudentId { get; set; }  // RecordBookNumber

    public Port(int row, int col, string deviceName = "")
    {
        PortNumber = _nextId++;
        Row = row;
        Col = col;
        DeviceName = string.IsNullOrWhiteSpace(deviceName)
            ? $"Device_{row:D2}_{col:D2}"
            : deviceName;
        DataBuffer = new byte[64];
        IsOpen = false;
        BytesWritten = 0;
    }

    // Reset static ID counter (for reinit)
    public static void ResetIdCounter() => _nextId = 1;

    public void Open()
    {
        if (IsOpen)
            throw new PortException($"Порт [{Row},{Col}] вже відкритий!", Row, Col);
        IsOpen = true;
        LastActivity = DateTime.Now;
    }

    public void Close()
    {
        if (!IsOpen)
            throw new PortException($"Порт [{Row},{Col}] вже закритий!", Row, Col);
        IsOpen = false;
        LastActivity = DateTime.Now;
    }

    public void Write(byte[] data)
    {
        if (!IsOpen)
            throw new PortException($"Порт [{Row},{Col}] закритий! Відкрийте порт перед записом.", Row, Col);
        if (data == null || data.Length == 0)
            throw new ArgumentException("Дані для запису не можуть бути порожніми!");
        if (data.Length > 64)
            throw new PortException($"Дані ({data.Length} байт) перевищують розмір буфера (64 байти)!", Row, Col);

        Array.Clear(DataBuffer, 0, DataBuffer.Length);
        Array.Copy(data, DataBuffer, data.Length);
        BytesWritten = data.Length;
        LastActivity = DateTime.Now;
    }

    public byte[] Read()
    {
        if (!IsOpen)
            throw new PortException($"Порт [{Row},{Col}] закритий! Відкрийте порт перед читанням.", Row, Col);

        byte[] result = new byte[BytesWritten];
        Array.Copy(DataBuffer, result, BytesWritten);
        LastActivity = DateTime.Now;
        return result;
    }

    public override string ToString()
    {
        string status = IsOpen ? "ВІДКР" : "ЗАКР ";
        string student = AssignedStudentId != null ? $"[{AssignedStudentId}]" : "     ";
        return $"[{Row,2},{Col,2}] {status} {DeviceName,-18} {BytesWritten,3}B {student}";
    }

    public object Clone()
    {
        Port clone = new Port(Row, Col, DeviceName)
        {
            AssignedStudentId = this.AssignedStudentId,
            BytesWritten = this.BytesWritten,
            LastActivity = this.LastActivity
        };
        if (IsOpen) clone.Open();
        Array.Copy(this.DataBuffer, clone.DataBuffer, 64);
        return clone;
    }
}
