using StudentGroupManagement2;

class Program
{
    static void Main(string[] args)
    {
        // Set console to support UTF-8 for Ukrainian characters
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        Menu menu = new Menu();
        menu.Run();
    }
}
