/*
Copyright (C) 2019-2025 TARAKHOMYN YURIY IVANOVYCH
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

Перевірка правильності числових значень

*/

namespace InterfaceGtk3;

public class Validate
{
    /// <summary>
    /// Чи це ціле число
    /// </summary>
    /// <param name="text">Значення</param>
    /// <returns></returns>
    public static (bool, int) IsInt(string text)
    {
        return (int.TryParse(text, out int value), value);
    }

    /// <summary>
    /// Чи це число з комою
    /// </summary>
    /// <param name="text">Значення</param>
    /// <returns></returns>
    public static (bool, decimal) IsDecimal(string text)
    {
        // Треба протестувати чи варто заміняти крапку на кому на інших мовах операційної системи
        if (text.Contains('.', StringComparison.CurrentCulture))
            text = text.Replace(".", ",");

        return (decimal.TryParse(text, out decimal value), value);
    }

    /// <summary>
    /// Чи це дата
    /// </summary>
    /// <param name="text">Значення</param>
    /// <returns></returns>
    public static (bool, DateTime) IsDateTime(string text)
    {
        return (DateTime.TryParse(text, out DateTime value), value);
    }

    /// <summary>
    /// Чи це час
    /// </summary>
    /// <param name="text">Значення</param>
    /// <returns></returns>
    public static (bool, TimeSpan) IsTime(string text)
    {
        return (TimeSpan.TryParse(text, out TimeSpan value), value);
    }
}