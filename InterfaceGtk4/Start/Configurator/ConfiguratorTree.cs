
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

namespace InterfaceGtk4;

/// <summary>
/// Базовий клас для виводу певного участку дерева конфігурації
/// 
/// Основа для класів:
///     ConfiguratorConstantsTree
///     ConfiguratorDirectoriesTree
///     ConfiguratorDocumentsTree
///     ConfiguratorEnumsTree
///     ConfiguratorJournalsTree
///     ConfiguratorRegistersAccumulationTree
///     ConfiguratorRegistersInformationTree
/// </summary>
public abstract class ConfiguratorTree
{
    /// <summary>
    /// Вибраний елемент дерева
    /// </summary>
    ConfiguratorItemRow? SelectionRow { get; set; }

    /// <summary>
    /// Сховище
    /// </summary>
    protected Gio.ListStore Store = Gio.ListStore.New(ConfiguratorItemRow.GetGType());

    /// <summary>
    /// Функція побудови дерева для TreeListModel
    /// </summary>
    protected abstract Gio.ListModel? CreateFunc(GObject.Object item);

    /// <summary>
    /// Заповнення дерева
    /// </summary>
    protected abstract void FillGrid();

    /// <summary>
    /// Функція активації елементу дерева
    /// </summary>
    public Action<string, string>? Activate { get; set; }

    /// <summary>
    /// Функції для меню
    /// </summary>
    public ToolbarAction? Toolbar { get; set; }

    /// <summary>
    /// Основний бокс
    /// </summary>
    protected Box VBox { get; set; } = Box.New(Orientation.Vertical, 0);

    /// <summary>
    /// Верхній набір меню
    /// </summary>
    Box HBoxToolbar { get; } = Box.New(Orientation.Horizontal, 0);

    /// <summary>
    /// Бокс для таблиці
    /// </summary>
    Box HBoxGrid { get; set; } = Box.New(Orientation.Horizontal, 0);

    public ConfiguratorTree(Action<string, string>? activate, ToolbarAction? toolbar)
    {
        Activate = activate;
        Toolbar = toolbar;

        AddToolbar();

        HBoxToolbar.MarginBottom = 5;
        VBox.Append(HBoxToolbar);

        TreeListModel list = TreeListModel.New(Store, false, false, CreateFunc);
        SingleSelection model = SingleSelection.New(list);
        ColumnView columnView = ColumnView.New(model);
        columnView.Reorderable = false;

        model.OnSelectionChanged += (_, _) =>
        {
            TreeListRow? row = (TreeListRow?)model.GetSelectedItem();
            SelectionRow = (ConfiguratorItemRow?)row?.Item;
        };

        AddColumn(columnView);

        ScrolledWindow scroll = ScrolledWindow.New();
        scroll.Vexpand = scroll.Hexpand = true;
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroll.Child = columnView;

        HBoxGrid.Append(scroll);
        VBox.Append(HBoxGrid);
    }

    protected void AddToolbar()
    {
        if (Toolbar == null) return;

        //Новий
        if (Toolbar.Add != null)
        {
            Button button = Button.NewFromIconName("new");
            button.MarginEnd = 5;
            button.TooltipText = "Додати";
            button.OnClicked += (_, _) => Toolbar.Add();
            HBoxToolbar.Append(button);
        }

        //Редагувати
        if (Toolbar.Edit != null)
        {
            Button button = Button.NewFromIconName("edit");
            button.MarginEnd = 5;
            button.TooltipText = "Редагувати";
            button.OnClicked += (_, _) =>
            {
                if (SelectionRow != null)
                    Toolbar.Edit(SelectionRow.Group, SelectionRow.Name);
            };
            HBoxToolbar.Append(button);
        }

        //Оновити
        {
            Button button = Button.NewFromIconName("refresh");
            button.MarginEnd = 5;
            button.TooltipText = "Оновити";
            button.OnClicked += (_, _) => FillGrid();
            HBoxToolbar.Append(button);
        }

        //Копіювати
        if (Toolbar.Copy != null)
        {
            Button button = Button.NewFromIconName("copy");
            button.MarginEnd = 5;
            button.TooltipText = "Копіювати";
            button.OnClicked += (_, _) =>
            {
                if (SelectionRow != null)
                    Toolbar.Copy(SelectionRow.Group, SelectionRow.Name);
            };
            HBoxToolbar.Append(button);
        }

        //Видалити
        if (Toolbar.Delete != null)
        {
            Button button = Button.NewFromIconName("delete");
            button.MarginEnd = 5;
            button.TooltipText = "Видалити";
            button.OnClicked += (_, _) =>
            {
                if (SelectionRow != null)
                    Toolbar.Delete(SelectionRow.Group, SelectionRow.Name);
            };
            HBoxToolbar.Append(button);
        }

        //Відкрити окремо
        if (Toolbar.OpenNewTab != null)
        {
            Separator separator = Separator.New(Orientation.Vertical);
            separator.MarginStart = 5;
            separator.MarginEnd = 10;
            HBoxToolbar.Append(separator);

            Button button = Button.NewFromIconName("go-up");
            button.MarginEnd = 5;
            button.TooltipText = "Відкрити окремо";
            button.OnClicked += (_, _) => Toolbar.OpenNewTab();
            HBoxToolbar.Append(button);
        }
    }

    protected void AddColumn(ColumnView columnView)
    {
        //Дерево
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += (_, args) =>
            {
                ListItem listItem = (ListItem)args.Object;
                var cell = LabelTablePartCell.New(null);

                TreeExpander expander = TreeExpander.New();
                expander.SetChild(cell);

                listItem.SetChild(expander);
            };

            factory.OnBind += (_, args) =>
            {
                ListItem listItem = (ListItem)args.Object;
                TreeListRow? row = (TreeListRow?)listItem.GetItem();
                if (row != null)
                {
                    TreeExpander? expander = (TreeExpander?)listItem.GetChild();
                    var cell = (LabelTablePartCell?)expander?.GetChild();
                    ConfiguratorItemRow? itemRow = (ConfiguratorItemRow?)row.GetItem();
                    if (expander != null && cell != null && itemRow != null)
                    {
                        expander.SetListRow(row);
                        cell.SetText(itemRow.Name);
                    }
                }
            };
            var column = ColumnViewColumn.New("Константи", factory);
            column.Resizable = true;
            columnView.AppendColumn(column);
        }

        //Тип даних
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += (_, args) =>
            {
                var listItem = (ListItem)args.Object;
                var cell = LabelTablePartCell.New(null);
                listItem.SetChild(cell);

            };
            factory.OnBind += (_, args) =>
            {
                ListItem listItem = (ListItem)args.Object;
                TreeListRow? row = (TreeListRow?)listItem.GetItem();
                if (row != null)
                {
                    var cell = (LabelTablePartCell?)listItem.Child;
                    ConfiguratorItemRow? itemRow = (ConfiguratorItemRow?)row.GetItem();
                    if (cell != null && itemRow != null)
                        cell.SetText(itemRow.Type);
                }
            };
            var column = ColumnViewColumn.New("Тип даних", factory);
            column.Resizable = true;
            columnView.AppendColumn(column);
        }

        //Деталізація
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += (_, args) =>
            {
                var listItem = (ListItem)args.Object;
                var cell = LabelTablePartCell.New(null);
                listItem.SetChild(cell);

            };
            factory.OnBind += (_, args) =>
            {
                ListItem listItem = (ListItem)args.Object;
                TreeListRow? row = (TreeListRow?)listItem.GetItem();
                if (row != null)
                {
                    var cell = (LabelTablePartCell?)listItem.Child;
                    ConfiguratorItemRow? itemRow = (ConfiguratorItemRow?)row.GetItem();
                    if (cell != null && itemRow != null)
                        cell.SetText(itemRow.Desc);
                }
            };
            var column = ColumnViewColumn.New("Деталізація", factory);
            column.Resizable = true;
            columnView.AppendColumn(column);
        }

        //Пуста колонка для заповнення вільного простору
        {
            ColumnViewColumn column = ColumnViewColumn.New(null, null);
            column.Resizable = true;
            column.Expand = true;
            columnView.AppendColumn(column);
        }

        columnView.OnActivate += (_, _) =>
        {
            SingleSelection model = (SingleSelection)columnView.Model;
            TreeListRow? row = (TreeListRow?)model.GetSelectedItem();
            if (row != null)
            {
                ConfiguratorItemRow? itemRow = (ConfiguratorItemRow?)row.Item;
                if (itemRow != null)
                    Activate?.Invoke(itemRow.Group, itemRow.Name);
            }
        };
    }

    /// <summary>
    /// Функції для меню
    /// </summary>
    public record ToolbarAction
    {
        public Action? Add { get; set; }
        public Action<string, string>? Edit { get; set; }
        public Action<string, string>? Copy { get; set; }
        public Action<string, string>? Delete { get; set; }
        public Action? OpenNewTab { get; set; }
    }
}