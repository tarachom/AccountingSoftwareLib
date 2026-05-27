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

Функції для вибору файлу або файлів.
В залежності від версії GTK використовує різні функції для відкриття діалогу вибору файлу.
    
*/

using Gtk;

namespace InterfaceGtk4;

public static class FunctionForFileDialog
{
    #region Фільтри

    public static FileFilter Filter_All()
    {
        FileFilter filter = FileFilter.New();
        filter.Name = "Усі файли";
        filter.AddPattern("*");

        return filter;
    }

    public static FileFilter Filter_Excel()
    {
        FileFilter filter = FileFilter.New();
        filter.Name = "Файли Excel (*.xls, *.xlsx)";
        filter.AddPattern("*.xls");
        filter.AddPattern("*.xlsx");

        return filter;
    }

    #endregion

    /// <summary>
    /// Вибрати один файл
    /// </summary>
    /// <param name="filters">Фільтри</param>
    /// <param name="callBackSelect">Функція обробки вибору</param>
    public static async Task SelectFile(List<FileFilter> filters, Func<string?, Task> callBackSelect, Window? parent)
    {
        if (Functions.CheckVersion(4, 10, 0) != null)
        {
            //
            //Версії нижче 4.10
            //

            FileChooserNative fileChooser = FileChooserNative.New("Виберіть файл", parent, FileChooserAction.Open, "Відкрити", "Скасувати");

            foreach (var filter in filters)
                fileChooser.AddFilter(filter);

            fileChooser.OnResponse += async (_, e) =>
            {
                string? path = null;
                if (e.ResponseId == (int)ResponseType.Accept)
                {
                    Gio.File? file = fileChooser.GetFile();
                    path = file?.GetPath();
                }
                await callBackSelect.Invoke(path);

                fileChooser.Dispose();
            };

            fileChooser.Show();
        }
        else
        {
            //
            //Версії 4.10 або більше
            //

            FileDialog dialog = FileDialog.New();
            dialog.Title = "Виберіть файл";

            Gio.ListStore store = Gio.ListStore.New(FileFilter.GetGType());

            foreach (var filter in filters)
                store.Append(filter);

            dialog.Filters = store;

            try
            {
                Gio.File? file = await dialog.OpenAsync(parent);
                await callBackSelect.Invoke(file?.GetPath());
            }
            catch
            {
                await callBackSelect.Invoke(null);
            }
        }
    }

    /// <summary>
    /// Вибрати файли
    /// </summary>
    /// <param name="filters">Фільтри</param>
    /// <param name="callBackSelect">Функція обробки вибору</param>
    public static async Task SelectFiles(List<FileFilter> filters, Func<string[]?, Task> callBackSelect, Window? parent)
    {
        if (Functions.CheckVersion(4, 10, 0) != null)
        {
            //
            //Версії нижче 4.10
            //

            FileChooserNative fileChooser = FileChooserNative.New("Виберіть файли", parent, FileChooserAction.Open, "Відкрити", "Скасувати");
            fileChooser.SelectMultiple = true;

            foreach (var filter in filters)
                fileChooser.AddFilter(filter);

            fileChooser.OnResponse += async (_, e) =>
            {
                if (e.ResponseId == (int)ResponseType.Accept)
                {
                    Gio.ListModel filesList = fileChooser.GetFiles();

                    List<string> list = [];
                    for (uint i = 0; i < filesList.GetNItems(); i++)
                    {
                        Gio.File? file = (Gio.File?)filesList.GetObject(i);
                        string? path = file?.GetPath();

                        if (path != null) list.Add(path);
                    }

                    await callBackSelect.Invoke([.. list]);
                }
                else
                    await callBackSelect.Invoke(null);

                fileChooser.Dispose();
            };
            fileChooser.Show();
        }
        else
        {
            //
            //Версії 4.10 або більше
            //

            FileDialog dialog = FileDialog.New();
            dialog.Title = "Виберіть файли";

            Gio.ListStore store = Gio.ListStore.New(FileFilter.GetGType());

            foreach (var filter in filters)
                store.Append(filter);

            dialog.Filters = store;

            Gio.ListModel? files = await dialog.OpenMultipleAsync(parent);
            if (files != null)
            {
                List<string> list = [];
                for (uint i = 0; i < files.GetNItems(); i++)
                {
                    Gio.File? file = (Gio.File?)files.GetObject(i);
                    string? path = file?.GetPath();

                    if (path != null) list.Add(path);
                }

                await callBackSelect.Invoke([.. list]);
            }
            else
                await callBackSelect.Invoke(null);
        }
    }
}