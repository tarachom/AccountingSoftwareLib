
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
    protected ConfiguratorItemRow? SelectionRow { get; set; } = null;

    /// <summary>
    /// Сховище
    /// </summary>
    protected Gio.ListStore Store = Gio.ListStore.New(ConfiguratorItemRow.GetGType());

    protected TreeListModel? TreeList { get; set; } = null;

    protected ColumnView Grid { get; } = ColumnView.NewWithProperties([]);

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
    public Action<ConfiguratorItemRow>? Activate { get; set; } = null;

    /// <summary>
    /// Функції для меню
    /// </summary>
    public ToolbarAction? Toolbar { get; set; } = null;

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

    public ConfiguratorTree(Action<ConfiguratorItemRow>? activate, ToolbarAction? toolbar)
    {
        Activate = activate;
        Toolbar = toolbar;

        AddToolbar();

        HBoxToolbar.MarginBottom = 5;
        VBox.Append(HBoxToolbar);

        TreeList = TreeListModel.New(Store, false, false, CreateFunc);

        MultiSelection model = MultiSelection.New(TreeList);
        model.OnSelectionChanged += (_, _) =>
        {
            Bitset selection = model.GetSelection();

            //Коли виділений один рядок
            if (selection.GetMinimum() == selection.GetMaximum())
            {
                TreeListRow? row = TreeList?.GetRow(selection.GetMaximum());
                SelectionRow = (ConfiguratorItemRow?)row?.GetItem();
            }
        };

        Grid = ColumnView.New(model);
        Grid.Reorderable = false;

        AddColumn();

        ScrolledWindow scroll = ScrolledWindow.New();
        scroll.Vexpand = scroll.Hexpand = true;
        scroll.PropagateNaturalWidth = true;
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroll.Child = Grid;

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
            button.AddCssClass("toolbar");
            button.MarginEnd = 5;
            button.TooltipText = "Додати";
            button.OnClicked += (_, _) => Toolbar.Add();
            HBoxToolbar.Append(button);
        }

        //Редагувати
        if (Toolbar.Edit != null)
        {
            Button button = Button.NewFromIconName("edit");
            button.AddCssClass("toolbar");
            button.MarginEnd = 5;
            button.TooltipText = "Редагувати";
            button.OnClicked += (_, _) =>
            {
                if (SelectionRow != null)
                    Toolbar.Edit(SelectionRow);
            };
            HBoxToolbar.Append(button);
        }

        //Оновити
        {
            Button button = Button.NewFromIconName("refresh");
            button.AddCssClass("toolbar");
            button.MarginEnd = 5;
            button.TooltipText = "Оновити";
            button.OnClicked += (_, _) => FillGrid();
            HBoxToolbar.Append(button);
        }

        //Копіювати
        if (Toolbar.Copy != null)
        {
            Button button = Button.NewFromIconName("copy");
            button.AddCssClass("toolbar");
            button.MarginEnd = 5;
            button.TooltipText = "Копіювати";
            button.OnClicked += (_, _) =>
            {
                if (SelectionRow != null)
                    Toolbar.Copy(SelectionRow);
            };
            HBoxToolbar.Append(button);
        }

        //Видалити
        if (Toolbar.Delete != null)
        {
            Button button = Button.NewFromIconName("delete");
            button.AddCssClass("toolbar");
            button.MarginEnd = 5;
            button.TooltipText = "Видалити";
            button.OnClicked += (_, _) =>
            {
                if (SelectionRow != null)
                    Toolbar.Delete(SelectionRow);
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
            button.AddCssClass("toolbar");
            button.MarginEnd = 5;
            button.TooltipText = "Відкрити окремо";
            button.OnClicked += (_, _) => Toolbar.OpenNewTab();
            HBoxToolbar.Append(button);
        }
    }

    protected void AddColumn()
    {
        //TreeExpander and Image
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += (_, args) =>
            {
                ListItem listItem = (ListItem)args.Object;
                TreeExpander expander = TreeExpander.New();
                expander.SetChild(ImageLabelTablePartCell.New());
                listItem.SetChild(expander);
            };
            factory.OnBind += (_, args) =>
            {
                ListItem listItem = (ListItem)args.Object;
                TreeExpander? expander = (TreeExpander?)listItem.GetChild();
                TreeListRow? treeRow = (TreeListRow?)listItem.GetItem();
                if (expander != null && treeRow != null)
                {
                    expander.SetListRow(treeRow);
                    ImageLabelTablePartCell? cell = (ImageLabelTablePartCell?)expander.GetChild();
                    ConfiguratorItemRow? itemRow = (ConfiguratorItemRow?)treeRow.Item;
                    if (cell != null && itemRow != null)
                        cell.SetImageAndText(itemRow.Group switch
                        {
                            "Directories" => Icon.ForConfigurator.Table,
                            "Documents" => Icon.ForConfigurator.Document,
                            "RegistersInformation" or "RegistersAccumulation" => Icon.ForConfigurator.Register,
                            "Enums" => Icon.ForConfigurator.List,
                            "Block" => Icon.ForTree.Normal,
                            "Const" => Icon.ForConfigurator.Const,
                            "Journals" => Icon.ForConfigurator.Journal,

                            "DimensionFields" => Icon.ForConfigurator.RegFields,
                            "ResourcesFields" => Icon.ForConfigurator.Calculator,
                            "PropertyFields" => Icon.ForConfigurator.Fields,

                            "FieldGroup" or "TablePartGroup" or "TabularListGroup" or "FormsGroup" => Icon.ForConfigurator.Sheets,

                            "TabularList" => Icon.ForInformation.Grid,
                            "TablePart" => Icon.ForInformation.Grid,
                            "Form" => Icon.ForConfigurator.Form,

                            "Field" or "TablePartField" or "DimensionField" or "ResourcesField" or "PropertyField" => Icon.ForConfigurator.Field,
                            _ => null
                        }, itemRow.Name);
                }
            };
            ColumnViewColumn column = ColumnViewColumn.New("Назва", factory);
            column.Resizable = true;
            Grid.AppendColumn(column);
        }

        //Назва таблиці чи поля
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += (_, args) =>
            {
                var listItem = (ListItem)args.Object;
                var cell = LabelTablePartCell.NewWithString(null);
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
                        cell.SetText(itemRow.TableOrField);
                }
            };
            var column = ColumnViewColumn.New("Таблиця / поле", factory);
            column.Resizable = true;
            Grid.AppendColumn(column);
        }

        //Тип даних
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += (_, args) =>
            {
                var listItem = (ListItem)args.Object;
                var cell = LabelTablePartCell.NewWithString(null);
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
            Grid.AppendColumn(column);
        }

        //Деталізація
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += (_, args) =>
            {
                var listItem = (ListItem)args.Object;
                var cell = LabelTablePartCell.NewWithString(null);
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
            var column = ColumnViewColumn.New("Детально", factory);
            column.Resizable = true;
            Grid.AppendColumn(column);
        }

        //Пуста колонка для заповнення вільного простору
        {
            ColumnViewColumn column = ColumnViewColumn.New(null, null);
            column.Resizable = true;
            column.Expand = true;
            Grid.AppendColumn(column);
        }

        Grid.OnActivate += (_, args) =>
        {
            TreeListRow? row = TreeList?.GetRow(args.Position);
            ConfiguratorItemRow? itemRow = (ConfiguratorItemRow?)row?.GetItem();
            if (itemRow != null)
                Activate?.Invoke(itemRow);
        };
    }

    protected static string SubstringDesc(string desc)
    {
        if (string.IsNullOrEmpty(desc))
            return string.Empty;

        return desc.Replace("\n", "").Replace("\r", "");
    }

    /// <summary>
    /// Функції для меню
    /// </summary>
    public record ToolbarAction
    {
        public Action? Add { get; set; } = null;
        public Action<ConfiguratorItemRow>? Edit { get; set; } = null;
        public Action<ConfiguratorItemRow>? Copy { get; set; } = null;
        public Action<ConfiguratorItemRow>? Delete { get; set; } = null;
        public Action? OpenNewTab { get; set; } = null;
    }
}