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
/// Функції для випадаючого списку
/// </summary>
public static class FunctionForDropDown
{
    public static bool SelectByValue(DropDown dropDown, string value)
    {
        if (string.IsNullOrEmpty(value) || value.Trim() == "0") return true;

        if (dropDown.Model is Gio.ListStore model)
            for (uint i = 0; i < model.GetNItems(); i++)
                if (model.GetObject(i) is DropDownItemRow itemRow && itemRow.Name == value)
                {
                    dropDown.Selected = i;
                    return true;
                }

        dropDown.Selected = 0;
        return false;
    }

    /// <summary>
    /// Шукає в списку елемент із значенням text і якщо знаходить то виділяє його
    /// </summary>
    /// <param name="dropDown">Список</param>
    /// <param name="text">Значення</param>
    public static void SelectByText(DropDown dropDown, string text)
    {
        if (dropDown.Model is StringList model)
            for (uint i = 0; i < model.GetNItems(); i++)
            {
                if (model.GetString(i) == text)
                {
                    dropDown.SetSelected(i);
                    break;
                }
            }
    }


}