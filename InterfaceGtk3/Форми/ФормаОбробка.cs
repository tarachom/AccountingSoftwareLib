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

using Gtk;

namespace InterfaceGtk3;

/// <summary>
/// Основа для класів:
///         Обробка
/// </summary>
public abstract class ФормаОбробка : Форма
{
    /// <summary>
    /// Верхній блок для кнопок
    /// </summary>
    protected Box HBoxTop = new Box(Orientation.Horizontal, 0);

    /// <summary>
    /// Середній блок
    /// </summary>
    protected Box HBoxBody = new Box(Orientation.Horizontal, 0);

    //Лог
    protected virtual LogMessage Лог { get; set; } = new LogMessage();

    public ФормаОбробка()
    {
        //Кнопки
        PackStart(HBoxTop, false, false, 10);

        //Середній блок
        PackStart(HBoxBody, false, false, 0);

        //Для виводу результатів
        PackStart(Лог, true, true, 5);

        ShowAll();
    }
}