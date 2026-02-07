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

using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Основа для класів:
///      ДовідникФормаЕлемент, 
///      ДокументФормаЕлемент, 
///      РегістриВідомостейФормаЕлемент
/// </summary>
public abstract class FormElement : Form
{
    /// <summary>
    /// Об'єкт який привязується до форми
    /// </summary>
    protected AccountingSoftware.Object? Element { get; init; }

    /// <summary>
    /// Чи це новий елемент
    /// </summary>
    public bool IsNew { get => Element?.IsNew ?? throw new NullReferenceException("Element is null"); }

    /// <summary>
    /// ІД елементу
    /// </summary>
    public UnigueID? UnigueID { get => Element?.UnigueID; }

    /// <summary>
    /// Назва         
    /// </summary>
    public string Caption { get => Element?.Caption ?? ""; }

    /// <summary>
    /// Функція зворотнього виклику для перевантаження списку
    /// </summary>
    public Action<UnigueID?>? CallBack_LoadRecords { get; set; }

    /// <summary>
    /// Інформація про блокування
    /// </summary>
    protected LockControl LockInfo = new();

    public FormElement(NotebookFunction? notebookFunc) : base(notebookFunc) { }

    #region Abstract Function

    /// <summary>
    /// Присвоєння значень
    /// </summary>
    public abstract ValueTask SetValue();

    /// <summary>
    /// Додаткова функція яка викликається із SetValue
    /// </summary>
    public virtual async ValueTask AssignValue() { await ValueTask.FromResult(true); }

    /// <summary>
    /// Фокус за стандартом
    /// </summary>
    public virtual void DefaultGrabFocus() { }

    /// <summary>
    /// Зчитування значень
    /// </summary>
    protected abstract void GetValue();

    /// <summary>
    /// Збереження
    /// </summary>
    protected abstract ValueTask<bool> Save();

    #endregion
}