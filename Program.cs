class Program
{
    static int totalWords = 0;
    static int totalFiles = 0;
    static object locker = new object();
    static List<string> fileNames = new List<string>();

    static void Main()
    {
        string directoryPath = @"C:\Users\legan\OneDrive\Рабочий стол\HW_30.05.2025\Test_HW";

        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine("Директория не найдена.");
            return;
        }

        string[] files = Directory.GetFiles(directoryPath, "*.txt").Where(filePath => !filePath.EndsWith("_result.txt")).ToArray();

        List<Task> tasks = new List<Task>();
        List<Thread> threads = new List<Thread>();

        foreach (var filePath in files)
        {
            Task tk = Task.Run(() =>
            {
                Thread td = ProcessFile(filePath);
                lock (locker)
                {
                    threads.Add(td);
                }
            });

            tasks.Add(tk);
        }


        Task.WaitAll(tasks.ToArray());

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine("\n=== Обработанные файлы ===");
        lock (locker)
        {
            foreach (var name in fileNames)
            {
                Console.WriteLine(name);
            }

            Console.WriteLine("\n=== Общая статистика ===");
            Console.WriteLine($"Обработано файлов: {totalFiles}");
            Console.WriteLine($"Суммарное количество слов: {totalWords}");
        }
    }

    static Thread ProcessFile(string filePath)
    {
        string info;
        int wordCount;

        try
        {
            info = File.ReadAllText(filePath);
            wordCount = info.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении файла {filePath}: {ex.Message}");
            return null;
        }

        string resultFile = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_result.txt");

        Thread thread = new Thread(() =>
        {
            try
            {
                File.WriteAllText(resultFile, $"Файл: {Path.GetFileName(filePath)} содержит {wordCount} слов");

                lock (locker)
                {
                    totalWords += wordCount;
                    totalFiles++;
                    fileNames.Add(Path.GetFileName(filePath));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи файла {resultFile}: {ex.Message}");
            }
        });

        thread.Start();
        return thread;
    }
}