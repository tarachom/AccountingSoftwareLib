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
using Gdk;

namespace InterfaceGtk4;

/// <summary>
/// РегістриВідомостейФормаЖурналМіні
/// </summary>
public abstract class RegisterAccumulationFormJournalSmall : FormJournal
{
    /// <summary>
    /// Перевизначення сховища для нового типу даних 
    /// </summary>
    public override Gio.ListStore Store { get; } = Gio.ListStore.New(RegisterAccumulationRowJournal.GetGType());

    public RegisterAccumulationFormJournalSmall(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        GridModel();

        ScrollGrid.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        ScrollGrid.SetChild(Grid);
        ScrollGrid.Vexpand = ScrollGrid.Hexpand = true;
        Append(ScrollGrid);

        ScrollPages.SetPolicy(PolicyType.Automatic, PolicyType.Never);
        ScrollPages.SetChild(HBoxPages);
        Append(ScrollPages);
    }

    protected override void GridModel()
    {
        //Модель
        MultiSelection model = MultiSelection.New(Store);

        Grid.Model = model;
    }

    public override async ValueTask SetValue()
    {
        await BeforeSetValue();
        await LoadRecords();
    }

    #region Virtual & Abstract Function

    #endregion
}
