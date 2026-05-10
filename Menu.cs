using System.Text;

namespace StudentGroupManagement2;

public class Menu
{
    private StudentGroup _group;
    private PortMatrix _matrix;
    private PortLogger _logger;

    public Menu()
    {
        _logger = new PortLogger();
        _group = new StudentGroup("КІ-22", "Комп'ютерна інженерія", 2);
        _matrix = new PortMatrix(_logger);
    }

    public void Run()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        bool running = true;
        while (running)
        {
            ShowMainMenu();
            string? choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1": AddOrRemoveStudentSubMenu(); break;
                    case "2": ShowStudentsWithGrades(); break;
                    case "3": _matrix.Initialize(); break;
                    case "4": OpenClosePort(); break;
                    case "5": WriteToPort(); break;
                    case "6": ReadFromPort(); break;
                    case "7": _matrix.PrintMatrix(); break;
                    case "8": AssignStudentToPort(); break;
                    case "9": SimulateLab(); break;
                    case "10": Console.WriteLine(_logger.GetFullLog()); break;
                    case "11": SearchMenu(); break;
                    case "12": SaveLoadMenu(); break;
                    case "13": ShowStatistics(); break;
                    case "14": Console.WriteLine(_group.GenerateMegaReport()); break;
                    case "0": running = false; break;
                    default: Console.WriteLine("  ⚠ Невірний вибір!"); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Помилка: {ex.Message}");
            }

            if (running)
            {
                Console.WriteLine("\n  Натисніть Enter щоб продовжити...");
                Console.ReadLine();
            }
        }
    }

    private void ShowMainMenu()
    {
        var sb = new StringBuilder();
        sb.Clear();
        sb.AppendLine("\n  ╔══════════════════════════════════════════════════════╗");
        sb.AppendLine($"  ║   Група: {_group.GroupName,-10} | Студентів: {_group.GroupSize,-3} | Порти: {(_matrix.IsInitialized ? "ON" : "OFF"),-3} ║");
        sb.AppendLine("  ╠══════════════════════════════════════════════════════╣");
        sb.AppendLine("  ║ 1. Додати / Видалити студента (ПР №1)              ║");
        sb.AppendLine("  ║ 2. Студенти з оцінками лаб (1D масив)              ║");
        sb.AppendLine("  ║ 3. Ініціалізувати матрицю портів 16×16             ║");
        sb.AppendLine("  ║ 4. Відкрити / Закрити порт                         ║");
        sb.AppendLine("  ║ 5. Записати дані в порт                            ║");
        sb.AppendLine("  ║ 6. Прочитати дані з порту                          ║");
        sb.AppendLine("  ║ 7. Вивести стан матриці (2D масив)                 ║");
        sb.AppendLine("  ║ 8. Прив’язати студента до порту                    ║");
        sb.AppendLine("  ║ 9. Симулювати лабораторну роботу                   ║");
        sb.AppendLine("  ║ 10. Переглянути лог портів (StringBuilder)         ║");
        sb.AppendLine("  ║ 11. Пошук (порт / студент)                         ║");
        sb.AppendLine("  ║ 12. Зберегти / Завантажити (JSON)                  ║");
        sb.AppendLine("  ║ 13. Статистика (Середній бал, активні порти)       ║");
        sb.AppendLine("  ║ 14. Генерація MEGA-звіту (100+ записів)            ║");
        sb.AppendLine("  ║ 0. Вийти                                           ║");
        sb.AppendLine("  ╚══════════════════════════════════════════════════════╝");
        sb.Append("  Ваш вибір: ");
        
        Console.Clear();
        Console.Write(sb.ToString());
    }

    private void AddOrRemoveStudentSubMenu()
    {
        Console.WriteLine("\n  1. Додати студента");
        Console.WriteLine("  2. Видалити студента");
        Console.Write("  Вибір: ");
        string? c = Console.ReadLine();
        if (c == "1") AddStudent();
        else if (c == "2") RemoveStudent();
    }

    private void AddStudent()
    {
        Console.WriteLine("\n  --- ДОДАВАННЯ СТУДЕНТА ---");
        string name = ReadString("ПІБ");
        string record = ReadString("Залікова (3 цифри)");
        DateTime dob = ReadDate("Дата народження (дд.мм.рррр)");
        string email = ReadString("Email");
        DateTime enroll = ReadDate("Дата зарахування (дд.мм.рррр)");

        var s = new Student(name, dob, record, enroll) { Group = _group.GroupName, PersonalEmail = email };
        s.UpdateAverageGrade(ReadDouble("Середній бал (0-100)", 0, 100));
        _group.AddStudent(s);
    }

    private void RemoveStudent()
    {
        string record = ReadString("Номер залікової");
        _group.RemoveStudent(record);
    }

    private void ShowStudentsWithGrades()
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n  --- ОЦІНКИ ЗА ЛАБОРАТОРНІ РОБОТИ ---");
        foreach (var s in _group.GetAllStudents())
        {
            sb.AppendLine($"  {s.FullName,-25} | Лаби: {string.Join(" ", s.LabGrades.Select(g => g.ToString("D2")))} | AVG: {s.GetAverageLabGrade()}");
        }
        Console.Write(sb);
    }

    private void OpenClosePort()
    {
        int r = (int)ReadDouble("Рядок (0-15)", 0, 15);
        int c = (int)ReadDouble("Стовпець (0-15)", 0, 15);
        var port = _matrix.GetPort(r, c);
        if (port.IsOpen) { _matrix.ClosePort(r, c); Console.WriteLine("  ✓ Порт закрито."); }
        else { _matrix.OpenPort(r, c); Console.WriteLine("  ✓ Порт відкрито."); }
    }

    private void WriteToPort()
    {
        int r = (int)ReadDouble("Рядок (0-15)", 0, 15);
        int c = (int)ReadDouble("Стовпець (0-15)", 0, 15);
        string data = ReadString("Дані для запису");
        _matrix.WriteToPort(r, c, Encoding.UTF8.GetBytes(data));
        Console.WriteLine("  ✓ Дані записано.");
    }

    private void ReadFromPort()
    {
        int r = (int)ReadDouble("Рядок (0-15)", 0, 15);
        int c = (int)ReadDouble("Стовпець (0-15)", 0, 15);
        byte[] data = _matrix.ReadFromPort(r, c);
        Console.WriteLine($"  ✓ Прочитано: {Encoding.UTF8.GetString(data)}");
    }

    private void AssignStudentToPort()
    {
        string record = ReadString("Номер залікової");
        var s = _group.FindStudent(record);
        if (s == null) { Console.WriteLine("  ⚠ Студента не знайдено!"); return; }
        
        int r = (int)ReadDouble("Рядок (0-15)", 0, 15);
        int c = (int)ReadDouble("Стовпець (0-15)", 0, 15);
        _group.AssignStudentToPort(s, r, c, _matrix);
    }

    private void SimulateLab()
    {
        string record = ReadString("Номер залікової");
        var s = _group.FindStudent(record);
        if (s == null) { Console.WriteLine("  ⚠ Студента не знайдено!"); return; }
        
        int lab = (int)ReadDouble("Номер лаби (1-10)", 1, 10);
        byte grade = (byte)ReadDouble("Оцінка (0-100)", 0, 100);
        _group.SimulateLabWork(s, lab, grade, _matrix);
    }

    private void SearchMenu()
    {
        Console.WriteLine("\n  1. Пошук студента за ПІБ");
        Console.WriteLine("  2. Пошук відкритих портів за назвою пристрою (2D матриця)");
        Console.Write("  Вибір: ");
        string? c = Console.ReadLine();
        if (c == "1")
        {
            string name = ReadString("Частина ПІБ");
            var results = _group.FindStudentByName(name);
            foreach (var r in results) Console.WriteLine($"  → {r}");
        }
        else if (c == "2")
        {
            string dev = ReadString("Частина назви пристрою");
            var ports = _matrix.FindOpenPortsByDevice(dev);
            if (ports.Count == 0) Console.WriteLine("  ⚠ Нічого не знайдено.");
            else foreach (var p in ports) Console.WriteLine($"  → {p}");
        }
    }

    private void SaveLoadMenu()
    {
        Console.WriteLine("\n  1. Зберегти");
        Console.WriteLine("  2. Завантажити");
        Console.Write("  Вибір: ");
        string? c = Console.ReadLine();
        if (c == "1") _group.SaveToFile();
        else if (c == "2")
        {
            string file = ReadString("Назва файлу");
            var loaded = StudentGroup.LoadFromFile(file);
            if (loaded != null) { _group = loaded; Console.WriteLine("  ✓ Успішно."); }
        }
    }

    private void ShowStatistics()
    {
        _group.ShowGroupStats();
        Console.WriteLine($"  Кількість активних портів: {_matrix.GetOpenPortCount()}");
    }

    // Helper methods
    private string ReadString(string prompt) { Console.Write($"  {prompt}: "); return Console.ReadLine() ?? ""; }
    private double ReadDouble(string prompt, double min, double max)
    {
        while (true)
        {
            Console.Write($"  {prompt}: ");
            if (double.TryParse(Console.ReadLine(), out double val) && val >= min && val <= max) return val;
            Console.WriteLine($"  ⚠ Введіть число від {min} до {max}!");
        }
    }
    private DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write($"  {prompt}: ");
            if (DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime d)) return d;
            Console.WriteLine("  ⚠ Формат: дд.мм.рррр");
        }
    }
}
