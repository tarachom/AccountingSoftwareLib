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

namespace InterfaceGtk3;

public abstract class РегістриНакопиченняЖурнал : ФормаЖурнал
{
    /// <summary>
    /// Період
    /// </summary>
    protected PeriodControl Період = new PeriodControl();

    /// <summary>
    /// Верхній набір меню
    /// </summary>
    protected Toolbar ToolbarTop = new Toolbar();

    /// <summary>
    /// Верхній бок для додаткових кнопок
    /// </summary>
    protected Box HBoxTop = new Box(Orientation.Horizontal, 0);

    /// <summary>
    /// Пошук
    /// </summary>
    SearchControl Пошук = new SearchControl();

    public РегістриНакопиченняЖурнал()
    {
        //Кнопки
        PackStart(HBoxTop, false, false, 10);

        //Період
        Період.Changed = PeriodChanged;
        HBoxTop.PackStart(Період, false, false, 2);

        //Пошук
        Пошук.Select = async x => await BeforeLoadRecords_OnSearch(x);
        Пошук.Clear = async () => await BeforeLoadRecords();
        HBoxTop.PackStart(Пошук, false, false, 2);

        CreateToolbar();

        TreeViewGrid.RowActivated += OnRowActivated;
        TreeViewGrid.KeyReleaseEvent += OnKeyReleaseEvent;

        //Сторінки
        AddPages(new Сторінки.Налаштування() { Тип = Сторінки.ТипЖурналу.РегістриНакопичення });

        ScrollTree.Add(TreeViewGrid);

        PackStart(ScrollTree, true, true, 0);
        PackStart(ScrollPages, false, true, 0);

        ShowAll();
    }

    public override async ValueTask SetValue()
    {
        DefaultGrabFocus();

        await BeforeSetValue();
    }

    #region Toolbar & Menu

    void CreateToolbar()
    {
        PackStart(ToolbarTop, false, false, 0);

        ToolButton refreshButton = new ToolButton(new Image(Stock.Refresh, IconSize.Menu), "Обновити") { TooltipText = "Обновити" };
        refreshButton.Clicked += OnRefreshClick;
        ToolbarTop.Add(refreshButton);
    }

    #endregion

    #region Virtual Function

    protected abstract void PeriodChanged();

    #endregion

    #region  TreeView

    void OnRowActivated(object sender, RowActivatedArgs args)
    {
        if (TreeViewGrid.Selection.CountSelectedRows() != 0)
        {
            TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]);
            SelectPointerItem = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
        }
    }

    void OnKeyReleaseEvent(object? sender, KeyReleaseEventArgs args)
    {
        switch (args.Event.Key)
        {
            case Gdk.Key.F5:
                {
                    OnRefreshClick(null, new EventArgs());
                    break;
                }
            case Gdk.Key.End:
            case Gdk.Key.Home:
            case Gdk.Key.Up:
            case Gdk.Key.Down:
            case Gdk.Key.Prior:
            case Gdk.Key.Next:
                {
                    OnRowActivated(TreeViewGrid, new RowActivatedArgs());
                    break;
                }
        }
    }

    #endregion

    #region ToolBar

    async void OnRefreshClick(object? sender, EventArgs args)
    {
        ToolButtonSensitive(sender, false);

        await BeforeLoadRecords();

        ToolButtonSensitive(sender, true);
    }

    #endregion
}