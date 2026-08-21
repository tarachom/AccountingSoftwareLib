using System.Diagnostics;

namespace InterfaceGtk3;

public static class ФункціїДляФайлів
{
    public static void ВідкритиФайл(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Помилка: Файл '{filePath}' не знайдено.");
            return;
        }

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };

            Process.Start(processInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не вдалося відкрити файл: {ex.Message}");
        }
    }
}