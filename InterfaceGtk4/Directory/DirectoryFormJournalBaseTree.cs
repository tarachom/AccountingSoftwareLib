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
using AccountingSoftware;

namespace InterfaceGtk4;

public abstract class DirectoryFormJournalBaseTree : DirectoryFormJournalBase
{
    /// <summary>
    /// Перевизначення сховища для нового типу даних
    /// </summary>
    public override Gio.ListStore Store { get; } = Gio.ListStore.New(DirectoryHierarchicalRow.GetGType());

    public DirectoryFormJournalBaseTree(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        GridModel();

        Grid.OnActivate += async (_, args) => await GridOnActivate(args.Position);
    }

    protected override void GridModel()
    {
        TreeListModel list = TreeListModel.New(Store, false, false, CreateFunc);

        //Модель
        MultiSelection model = MultiSelection.New(list);
        model.OnSelectionChanged += GridOnSelectionChanged;

        Grid.Model = model;
    }

    public override async ValueTask SetValue()
    {
        DefaultGrabFocus();
        await BeforeSetValue();

        await LoadRecords();
        //RunUpdateRecords();
    }

    private static Gio.ListModel? CreateFunc(GObject.Object item)
    {
        var itemRow = (DirectoryHierarchicalRow)item;
        Gio.ListStore? store = null;

        if (itemRow.Sub.Count > 0)
        {
            store = Gio.ListStore.New(DirectoryHierarchicalRow.GetGType());
            foreach (DirectoryHierarchicalRow subRow in itemRow.Sub)
                store.Append(subRow);
        }

        return store;
    }
}
