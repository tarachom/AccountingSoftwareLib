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

using Gtk;

namespace InterfaceGtk4;

/// <summary>
/// Функції для списку
/// </summary>
public static class FunctionForListBox
{
    /// <summary>
    /// Повна очистка списку
    /// </summary>
    /// <param name="listBox"></param>
    public static void RemoveAll(ListBox listBox)
    {
        if (Functions.CheckVersion(4, 12, 0) != null)
        {
            //
            //Версія нижче 4.12
            //

            Widget? child = listBox.GetFirstChild();
            while (child != null)
            {
                Widget? next = child.GetNextSibling();
                listBox.Remove(child);
                child = next;
            }
        }
        else
        {
            //Метод для 4.12+
            listBox.RemoveAll();
        }
    }
}