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
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Основа для класів:
///     ДовідникЖурнал, 
///     ДовідникШвидкийВибір, 
///     ДокументЖурнал, 
///     Журнал, 
///     РегістриВідомостейЖурнал, 
///     РегістриНакопиченняЖурнал,
///     РегістриНакопиченняЖурнал_СпрощенийРежим
/// </summary>
public abstract class FormJournal : Form
{
    /// <summary>
    /// Вспливаюче вікно власник даної форми
    /// </summary>
    public Popover? PopoverParent { get; set; }

    /// <summary>
    /// Елемент на який треба спозиціонувати список при обновленні
    /// </summary>
    public UnigueID? SelectPointerItem { get; set; }

    /// <summary>
    /// Дерево
    /// </summary>
    protected TreeView TreeViewGrid = new TreeView(); //??

    /// <summary>
    /// Прокрутка дерева
    /// </summary>
    protected ScrolledWindow ScrollTree = ScrolledWindow.New();

    /// <summary>
    /// Прокрутка для сторінок
    /// </summary>
    protected ScrolledWindow ScrollPages = ScrolledWindow.New();

    /// <summary>
    /// Бокс для сторінок
    /// </summary>
    protected Box HBoxPages = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Режим який вказує що форма використовується як елемент в іншій формі 
    /// (наприклад дерево використовується в ішому журналі)
    /// </summary>
    public bool CompositeMode { get; set; } = false;

    public FormJournal()
    {
        
    }

    #region Virtual & Abstract Function

    /// <summary>
    /// Присвоєння значень
    /// </summary>
    public abstract ValueTask SetValue();

    /// <summary>
    /// Додаткова функція яка викликається із SetValue
    /// </summary>
    protected virtual async ValueTask BeforeSetValue() { await ValueTask.FromResult(true); }

    /// <summary>
    /// Фокус за стандартом
    /// </summary>
    public virtual void DefaultGrabFocus() { TreeViewGrid.GrabFocus(); }

    /// <summary>
    /// Завантаження списку
    /// </summary>
    public abstract ValueTask LoadRecords();

    /// <summary>
    /// Завантаження списку при пошуку
    /// </summary>
    public virtual async ValueTask LoadRecords_OnSearch(string searchText) { await ValueTask.FromResult(true); }

    /// <summary>
    /// Фільтер
    /// </summary>
    public virtual async ValueTask LoadRecords_OnFilter() { await ValueTask.FromResult(true); }

    /// <summary>
    /// Для дерева
    /// </summary>
    /// <returns></returns>
    public virtual async ValueTask LoadRecords_OnTree() { await ValueTask.FromResult(true); }

    #endregion
}