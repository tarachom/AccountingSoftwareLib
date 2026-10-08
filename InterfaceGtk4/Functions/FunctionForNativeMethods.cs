/*
Copyright (C) 2019-2026 TARAKHOMYN YURIY IVANOVYCH
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

/*
Автор:    Тарахомин Юрій Іванович
Адреса:   Україна, м. Львів
Сайт:     accounting.org.ua
*/

/*

*/

using System.Runtime.InteropServices;
using System.Text.Json;

namespace InterfaceGtk4;

/// <summary>
/// Cистемний виклик Windows, який додає вказану папку до списку пошуку DLL
/// </summary>
internal static partial class NativeMethods
{
    [LibraryImport("kernel32.dll", EntryPoint = "SetDllDirectoryW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetDllDirectory(string lpPathName);
}

public static class FunctionForNativeMethods
{
    /// <summary>
    /// Задає шлях до бібліотек GTK
    /// </summary>
    /// <param name="path">Шлях</param>
    public static void SetMsysDirectory(string? path = null)
    {
        //Для Windows реєструється шлях до бібліотек Gtk
        if (OperatingSystem.IsWindows())
        {
            string msysPath;
            if (string.IsNullOrEmpty(path))
            {
                // Стандартний шлях до папки MSYS
                const string defMsysPath = "C:\\msys64\\ucrt64\\bin";

                // Шлях до папки MSYS з конфігураційного файлу
                msysPath = GetMsysDirectory() ?? defMsysPath;
            }
            else
                msysPath = path;

            if (Directory.Exists(msysPath))
            {
                if (!NativeMethods.SetDllDirectory(msysPath))
                    Console.WriteLine("Warning: Failed to set DLL directory.");
            }
            else
                Console.WriteLine($"Warning: MSYS2 path not found at {msysPath}");
        }
    }

    /// <summary>
    /// Функція читає конфігураційний файл
    /// </summary>
    /// <returns>Повертає шлях із конфігураційного файлу або null</returns>
    static string? GetMsysDirectory()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "MsysSettings.json");
        if (File.Exists(path))
        {
            string content = File.ReadAllText(path);
            JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
            MsysConfig? config = JsonSerializer.Deserialize<MsysConfig>(content, options);
            return (config != null && !string.IsNullOrEmpty(config.MsysDirectory)) ? config.MsysDirectory : null;
        }

        return null;
    }

    record MsysConfig
    {
        public string? MsysDirectory { get; set; } = null;
    }
}