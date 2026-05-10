using System.Text;

namespace StudentGroupManagement2;

public enum StudentStatus
{
    Active,
    AcademicLeave,
    Expelled,
    Graduated
}

public class Student : ICloneable
{
    private string _fullName = "";
    private DateTime _dateOfBirth;
    private string _recordBookNumber = "";
    private double _averageGrade;
    private string _personalEmail = "";

    // --- Lab Grades (1D Array) ---
    public byte[] LabGrades { get; private set; } = new byte[10];
    public int AssignedPortRow { get; set; } = -1;
    public int AssignedPortCol { get; set; } = -1;

    public Student(string fullName, DateTime dateOfBirth, string recordBookNumber, DateTime enrollmentDate)
    {
        FullName = fullName;
        _dateOfBirth = dateOfBirth;
        RecordBookNumber = recordBookNumber;
        EnrollmentDate = enrollmentDate;

        Status = StudentStatus.Active;
        _averageGrade = 0;
        Notes = "";
        LabGrades = new byte[10];
    }

    public string FullName
    {
        get => _fullName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ПІБ не може бути порожнім!");
            if (value.Trim().Length < 3)
                throw new ArgumentException("ПІБ занадто короткий (мінімум 3 символів)!");
            _fullName = value.Trim();
        }
    }

    public string RecordBookNumber
    {
        get => _recordBookNumber;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Номер залікової не може бути порожнім!");
            if (value.Length != 3 || !value.All(char.IsDigit))
                throw new ArgumentException("Номер залікової має містити рівно 3 цифр! Наприклад: 001");
            _recordBookNumber = value;
        }
    }

    public double AverageGrade
    {
        get => _averageGrade;
        set
        {
            if (value < 0 || value > 100)
                throw new ArgumentException("Середній бал має бути від 0 до 100!");
            _averageGrade = Math.Round(value, 1);
        }
    }

    public string PersonalEmail
    {
        get => _personalEmail;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email не може бути порожнім!");
            if (!value.Contains('@') || !value.Contains('.'))
                throw new ArgumentException("Невірний формат email! Приклад: student@gmail.com");
            _personalEmail = value.ToLower();
        }
    }

    public StudentStatus Status { get; set; }
    public string Notes { get; set; } = "";
    public DateTime EnrollmentDate { get; init; }
    public int GradeChangeCount { get; private set; }
    public required string Group { get; set; }

    public int Age
    {
        get
        {
            int age = DateTime.Today.Year - _dateOfBirth.Year;
            if (_dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
            return age;
        }
    }

    public DateTime AgeAsDate => _dateOfBirth;

    public int YearOfStudy
    {
        get
        {
            int years = DateTime.Today.Year - EnrollmentDate.Year;
            if (EnrollmentDate.Date > DateTime.Today.AddYears(-years)) years--;
            return years + 1;
        }
    }

    // --- Lab Grade Methods ---
    public void AddLabGrade(int labNumber, byte grade)
    {
        if (labNumber < 1 || labNumber > 10)
            throw new IndexOutOfRangeException($"Номер лабораторної має бути від 1 до 10, отримано: {labNumber}");
        if (grade > 100)
            throw new ArgumentException("Оцінка за лабораторну має бути від 0 до 100!");

        LabGrades[labNumber - 1] = grade;
    }

    public double GetAverageLabGrade()
    {
        int count = 0;
        double sum = 0;
        foreach (byte g in LabGrades)
        {
            if (g > 0) { sum += g; count++; }
        }
        return count == 0 ? 0 : Math.Round(sum / count, 2);
    }

    public byte[] GetSortedLabGrades()
    {
        byte[] copy = (byte[])LabGrades.Clone();
        Array.Sort(copy);
        return copy;
    }

    public void UpdateAverageGrade(double newGrade)
    {
        if (newGrade < 0 || newGrade > 100)
        {
            Console.WriteLine("Помилка: бал має бути від 0 до 100!");
            return;
        }
        double oldGrade = _averageGrade;
        AverageGrade = newGrade;
        GradeChangeCount++;
        Console.WriteLine($"Бал оновлено: {oldGrade} → {AverageGrade}");
    }

    public bool IsExcellent() => AverageGrade >= 90;
    public bool IsFailing() => AverageGrade < 60 && AverageGrade > 0;

    public int CalculateAge() => Age;
    public int GetYearsToGraduation() => Math.Max(0, 4 - YearOfStudy);

    private string GetStatusText() => Status switch
    {
        StudentStatus.Active => "Активний",
        StudentStatus.AcademicLeave => "Академічна відпустка",
        StudentStatus.Expelled => "Відрахований",
        StudentStatus.Graduated => "Випускник",
        _ => "Невідомо"
    };

    public void ShowDetailedInfo()
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n" + new string('=', 55));
        sb.AppendLine("  ІНФОРМАЦІЯ ПРО СТУДЕНТА");
        sb.AppendLine(new string('=', 55));
        sb.AppendLine($"  ПІБ:               {FullName}");
        sb.AppendLine($"  Група:             {Group}");
        sb.AppendLine($"  Дата народження:   {_dateOfBirth:dd.MM.yyyy}");
        sb.AppendLine($"  Вік:               {Age} років");
        sb.AppendLine($"  Залікова книжка:   {RecordBookNumber}");
        sb.AppendLine($"  Email:             {PersonalEmail}");
        sb.AppendLine($"  Дата зарахування:  {EnrollmentDate:dd.MM.yyyy}");
        sb.AppendLine($"  Курс:              {YearOfStudy}");
        sb.AppendLine($"  Статус:            {GetStatusText()}");
        sb.AppendLine($"  Середній бал:      {AverageGrade}");
        sb.AppendLine($"  Оцінка змінена:    {GradeChangeCount} разів");

        // Lab grades
        sb.AppendLine($"  --- Оцінки за лабораторні ---");
        for (int i = 0; i < LabGrades.Length; i++)
        {
            string gradeStr = LabGrades[i] == 0 ? "не здано" : LabGrades[i].ToString();
            sb.AppendLine($"  Лаб.{i + 1,2}: {gradeStr}");
        }
        sb.AppendLine($"  Середній бал (лаби): {GetAverageLabGrade()}");

        if (AssignedPortRow >= 0)
            sb.AppendLine($"  Лабораторне місце: порт [{AssignedPortRow},{AssignedPortCol}]");

        if (!string.IsNullOrWhiteSpace(Notes))
            sb.AppendLine($"  Нотатки: {Notes}");

        sb.AppendLine(new string('=', 55));

        if (IsExcellent()) sb.AppendLine("  ★ ВІДМІННИК!");
        else if (IsFailing()) sb.AppendLine("  ⚠ ЗАГРОЗА ВІДРАХУВАННЯ (бал < 60)");

        Console.Write(sb);
    }

    public override string ToString()
    {
        return $"{FullName} | Залікова: {RecordBookNumber} | Бал: {AverageGrade} | {GetStatusText()}";
    }

    // ICloneable
    public object Clone()
    {
        Student clone = new Student(FullName, _dateOfBirth, RecordBookNumber, EnrollmentDate)
        {
            Group = this.Group,
            Status = this.Status,
            Notes = this.Notes,
            AssignedPortRow = this.AssignedPortRow,
            AssignedPortCol = this.AssignedPortCol
        };
        clone._personalEmail = this._personalEmail;
        clone._averageGrade = this._averageGrade;
        clone.LabGrades = (byte[])this.LabGrades.Clone();
        return clone;
    }
}
