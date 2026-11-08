
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

using GObject;

namespace InterfaceGtk4;

/// <summary>
/// Клас для даних моделі яка викоритовується для виведення дерева конфігурації
/// </summary>
[Subclass<GObject.Object>]
public partial class ConfiguratorItemRow
{
    public static ConfiguratorItemRow New() => NewWithProperties([]);

    /// <summary>
    /// Група
    /// </summary>
    public string Group { get; set; } = "";

    /// <summary>
    /// Назва
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Латиницею
    /// </summary>
    public string LatinName { get; set; } = "";

    /// <summary>
    /// Об'єкт
    /// </summary>
    public object? Obj { get; set; } = null;

    /// <summary>
    /// Власник об'єкту
    /// </summary>
    public object? ParentObj { get; set; } = null;

    /// <summary>
    /// Назва таблиці чи поля
    /// </summary>
    public string TableOrField { get; set; } = "";

    /// <summary>
    /// Тип даних
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// Пояснення
    /// </summary>
    public string Desc { get; set; } = "";
}