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

using Gtk;

namespace InterfaceGtk4;

/// <summary>
/// Основа для класів:
///         Processing (Обробка)
/// </summary>
public abstract class FormProcessing : Form
{
    /// <summary>
    /// Верхній блок для кнопок
    /// </summary>
    protected Box HBoxTop = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Середній блок
    /// </summary>
    protected Box HBoxBody = New(Orientation.Horizontal, 0);

    //Лог
    protected virtual LogMessage Log { get; set; } = new LogMessage();

    public FormProcessing(NotebookFunction? notebook) : base(notebook)
    {
        //Кнопки
        HBoxTop.MarginBottom = 10;
        Append(HBoxTop);

        //Середній блок
        HBoxBody.MarginBottom = 2;
        Append(HBoxBody);

        //Для виводу результатів
        Log.Hexpand = Log.Vexpand = true;
        Log.MarginBottom = 5;
        Append(Log);
    }
}