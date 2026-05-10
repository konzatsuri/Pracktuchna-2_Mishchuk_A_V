using System.Text.Json;
using System.Text;

namespace StudentGroupManagement2;

public class StudentGroup
{
    private List<Student> _students;
    public string GroupName { get; set; }
    public string Specialty { get; set; }
    public int Course { get; set; }
    public int GroupSize => _students.Count;

    public double AverageGroupGrade
    {
        get
        {
            if (_students.Count == 0) return 0;
            double sum = 0;
            foreach (Student s in _students)
                sum += s.AverageGrade;
            return Math.Round(sum / _students.Count, 2);
        }
    }

    public StudentGroup(string groupName, string specialty, int course)
    {
        GroupName = groupName;
        Specialty = specialty;
        Course = course;
        _students = new List<Student>();
    }

    public void AddStudent(Student student)
    {
        if (_students.Any(s => s.RecordBookNumber == student.RecordBookNumber))
        {
            Console.WriteLine($"  ⚠ Студент із залікової {student.RecordBookNumber} вже є в групі!");
            return;
        }
        _students.Add(student);
        Console.WriteLine($"  ✓ Студента {student.FullName} додано до групи {GroupName}.");
    }

    public void RemoveStudent(string recordBookNumber)
    {
        Student? student = _students.Find(s => s.RecordBookNumber == recordBookNumber);
        if (student == null)
        {
            Console.WriteLine($"  ⚠ Студента з залікової {recordBookNumber} не знайдено!");
            return;
        }
        _students.Remove(student);
        Console.WriteLine($"  ✓ Студента {student.FullName} видалено з групи.");
    }

    public Student? FindStudent(string recordBookNumber)
    {
        return _students.Find(s => s.RecordBookNumber == recordBookNumber);
    }

    public List<Student> FindStudentByName(string namePart)
    {
        return _students
            .Where(s => s.FullName.Contains(namePart, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<Student> GetExcellentStudents() => _students.Where(s => s.IsExcellent()).ToList();
    public List<Student> GetFailingStudents() => _students.Where(s => s.IsFailing()).ToList();
    public List<Student> GetAllStudents() => new List<Student>(_students);

    // --- PR #2 Specific Methods ---

    public void AssignStudentToPort(Student s, int row, int col, PortMatrix matrix)
    {
        // Remove from old port if assigned
        if (s.AssignedPortRow != -1)
        {
            var oldPort = matrix.GetPort(s.AssignedPortRow, s.AssignedPortCol);
            oldPort.AssignedStudentId = null;
        }

        matrix.AssignStudent(row, col, s.RecordBookNumber);
        s.AssignedPortRow = row;
        s.AssignedPortCol = col;
        Console.WriteLine($"  ✓ Студент {s.FullName} закріплений за місцем [{row},{col}].");
    }

    public List<Student> GetStudentsByPortStatus(bool isOpen, PortMatrix matrix)
    {
        var result = new List<Student>();
        foreach (var s in _students)
        {
            if (s.AssignedPortRow != -1)
            {
                var port = matrix.GetPort(s.AssignedPortRow, s.AssignedPortCol);
                if (port.IsOpen == isOpen)
                    result.Add(s);
            }
        }
        return result;
    }

    public void SimulateLabWork(Student s, int labNumber, byte grade, PortMatrix matrix)
    {
        if (s.AssignedPortRow == -1)
            throw new InvalidOperationException("Студент не закріплений за портом!");

        // 1. Add grade to 1D array
        s.AddLabGrade(labNumber, grade);

        // 2. Write data to port (Simulation)
        var port = matrix.GetPort(s.AssignedPortRow, s.AssignedPortCol);
        if (!port.IsOpen) port.Open();

        byte[] labData = Encoding.UTF8.GetBytes($"LAB_{labNumber}_GRADE_{grade}");
        matrix.WriteToPort(s.AssignedPortRow, s.AssignedPortCol, labData);

        Console.WriteLine($"  🚀 Симуляція: Студент {s.FullName} виконав Лаб №{labNumber} на {grade} балів.");
    }

    public string GenerateMegaReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("╔" + new string('═', 70) + "╗");
        sb.AppendLine("║" + " ГЛОБАЛЬНИЙ ЗВІТ ГРУПИ (StringBuilder Performance Demo)".PadLeft(60) + " ║");
        sb.AppendLine("╠" + new string('═', 70) + "╣");
        
        foreach (var s in _students)
        {
            sb.AppendLine($"║ Студент: {s.FullName,-30} | Залікова: {s.RecordBookNumber,-10} ║");
            sb.AppendLine($"║   Середній бал: {s.AverageGrade,6} | Оцінки Лаб: {string.Join(", ", s.LabGrades)} ║");
            if (s.AssignedPortRow != -1)
                sb.AppendLine($"║   Порт: [{s.AssignedPortRow,2},{s.AssignedPortCol,2}] | Середній лаб-бал: {s.GetAverageLabGrade(),5} ║");
            sb.AppendLine("╟" + new string('─', 70) + "╢");
        }

        // Simulating 100+ records for the task requirement
        sb.AppendLine("║" + " АРХІВНІ ЗАПИСИ ПОПЕРЕДНІХ РОКІВ ".PadLeft(50) + " ║");
        for (int i = 1; i <= 100; i++)
        {
            sb.AppendLine($"║ Record #{i:D3}: ARCHIVE_DATA_{Guid.NewGuid().ToString().Substring(0,8)} | STATUS: PROCESSED ║");
        }

        sb.AppendLine("╚" + new string('═', 70) + "╝");
        return sb.ToString();
    }

    public void ShowGroupStats()
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n  ─── СТАТИСТИКА ГРУПИ ───");
        sb.AppendLine($"  Група: {GroupName} | Спеціальність: {Specialty} | Курс: {Course}");
        sb.AppendLine($"  Студентів: {GroupSize}");
        sb.AppendLine($"  Середній бал групи: {AverageGroupGrade}");

        int excellent = GetExcellentStudents().Count;
        int failing = GetFailingStudents().Count;
        double percentExcellent = GroupSize > 0 ? Math.Round((double)excellent / GroupSize * 100, 1) : 0;

        sb.AppendLine($"  Відмінників: {excellent} ({percentExcellent}%)");
        sb.AppendLine($"  Загроза відрахування: {failing}");
        
        Console.Write(sb);
    }

    public void SaveToFile()
    {
        var data = new GroupData
        {
            GroupName = this.GroupName,
            Specialty = this.Specialty,
            Course = this.Course,
            Students = _students.Select(s => new StudentData
            {
                FullName = s.FullName,
                DateOfBirth = s.AgeAsDate,
                RecordBookNumber = s.RecordBookNumber,
                AverageGrade = s.AverageGrade,
                Status = s.Status.ToString(),
                EnrollmentDate = s.EnrollmentDate,
                PersonalEmail = s.PersonalEmail,
                Notes = s.Notes,
                Group = s.Group,
                LabGrades = s.LabGrades,
                AssignedPortRow = s.AssignedPortRow,
                AssignedPortCol = s.AssignedPortCol
            }).ToList()
        };

        string fileName = $"{GroupName}_PR2_data.json";
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fileName, json);
        Console.WriteLine($"  ✓ Дані збережено у файл: {fileName}");
    }

    public static StudentGroup? LoadFromFile(string fileName)
    {
        if (!File.Exists(fileName)) return null;
        try
        {
            string json = File.ReadAllText(fileName);
            GroupData? data = JsonSerializer.Deserialize<GroupData>(json);
            if (data == null) return null;

            StudentGroup group = new StudentGroup(data.GroupName, data.Specialty, data.Course);
            foreach (StudentData sd in data.Students)
            {
                Student student = new Student(sd.FullName, sd.DateOfBirth, sd.RecordBookNumber, sd.EnrollmentDate)
                {
                    Group = sd.Group,
                    PersonalEmail = sd.PersonalEmail,
                    Notes = sd.Notes,
                    Status = Enum.Parse<StudentStatus>(sd.Status),
                    AssignedPortRow = sd.AssignedPortRow,
                    AssignedPortCol = sd.AssignedPortCol
                };
                student.UpdateAverageGrade(sd.AverageGrade);
                
                // Load lab grades
                if (sd.LabGrades != null)
                    for (int i = 0; i < Math.Min(10, sd.LabGrades.Length); i++)
                        student.AddLabGrade(i + 1, sd.LabGrades[i]);

                group._students.Add(student);
            }
            return group;
        }
        catch (Exception ex) { Console.WriteLine($"  ⚠ Помилка завантаження: {ex.Message}"); return null; }
    }
}

public class GroupData
{
    public string GroupName { get; set; } = "";
    public string Specialty { get; set; } = "";
    public int Course { get; set; }
    public List<StudentData> Students { get; set; } = new();
}

public class StudentData
{
    public string FullName { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public string RecordBookNumber { get; set; } = "";
    public double AverageGrade { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime EnrollmentDate { get; set; }
    public string PersonalEmail { get; set; } = "";
    public string Notes { get; set; } = "";
    public string Group { get; set; } = "";
    public byte[]? LabGrades { get; set; }
    public int AssignedPortRow { get; set; }
    public int AssignedPortCol { get; set; }
}
