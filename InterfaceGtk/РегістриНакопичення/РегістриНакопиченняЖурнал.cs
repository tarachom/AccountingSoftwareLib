/*
Copyright (C) 2019-2024 TARAKHOMYN YURIY IVANOVYCH
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

namespace InterfaceGtk
{
    public abstract class РегістриНакопиченняЖурнал : ФормаЖурнал
    {
        /// <summary>
        /// Для позиціювання в списку
        /// </summary>
        public UnigueID? SelectPointerItem { get; set; }

        /// <summary>
        /// Період
        /// </summary>
        protected PeriodControl Період = new PeriodControl();

        /// <summary>
        /// Верхній набір меню
        /// </summary>
        protected Toolbar ToolbarTop = new Toolbar();

        /// <summary>
        /// Верхній бок для періоду
        /// </summary>
        protected Box HBoxPeriod = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Верхній бок для додаткових кнопок
        /// </summary>
        protected Box HBoxTop = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Дерево
        /// </summary>
        protected TreeView TreeViewGrid = new TreeView();

        /// <summary>
        /// Пошук
        /// </summary>
        SearchControl ПошукПовнотекстовий = new SearchControl();

        public РегістриНакопиченняЖурнал() : base()
        {
            //Період
            PackStart(HBoxPeriod, false, false, 10);
            Період.Changed = PeriodChanged;
            HBoxPeriod.PackStart(Період, false, false, 2);

            //Пошук
            ПошукПовнотекстовий.Select = LoadRecords_OnSearch;
            ПошукПовнотекстовий.Clear = LoadRecords;
            HBoxPeriod.PackStart(ПошукПовнотекстовий, false, false, 2);

            //Кнопки
            PackStart(HBoxTop, false, false, 0);

            CreateToolbar();

            ScrolledWindow scrollTree = new ScrolledWindow() { ShadowType = ShadowType.In };
            scrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

            TreeViewGrid.Selection.Mode = SelectionMode.Multiple;
            TreeViewGrid.ActivateOnSingleClick = true;
            TreeViewGrid.RowActivated += OnRowActivated;
            TreeViewGrid.KeyReleaseEvent += OnKeyReleaseEvent;
            scrollTree.Add(TreeViewGrid);

            PackStart(scrollTree, true, true, 0);

            ShowAll();
        }

        public async ValueTask SetValue()
        {
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

        protected virtual async ValueTask BeforeSetValue() { await ValueTask.FromResult(true); }

        protected virtual void LoadRecords() { }

        protected virtual void LoadRecords_OnSearch(string searchText) { }

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
                        LoadRecords();
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

        void OnRefreshClick(object? sender, EventArgs args)
        {
            LoadRecords();
        }

        #endregion
    }
}