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

/*

Повнотекстовий пошук

*/

using Gtk;
using System.Reflection;
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class PageFullTextSearch : Форма
    {
        private Kernel Kernel { get; set; }
        private string NameSpageProgram { get; set; }
        private string NameSpageCodeGeneration { get; set; }
        private Assembly ExecutingAssembly { get; } = Assembly.GetCallingAssembly();

        Box vBoxMessage = new Box(Orientation.Vertical, 0);
        SearchEntry entryTextSearch = new SearchEntry() { WidthRequest = 500 };
        const int maxRowsToPage = 10;
        uint offset = 0;
        int count = 0;

        public PageFullTextSearch(Kernel kernel, string nameSpageProgram, string nameSpageCodeGeneration) : base()
        {
            Kernel = kernel;
            NameSpageProgram = nameSpageProgram;
            NameSpageCodeGeneration = nameSpageCodeGeneration;

            Box hBoxTop = new Box(Orientation.Horizontal, 0) { Halign = Align.Center };
            PackStart(hBoxTop, false, false, 10);

            hBoxTop.PackStart(entryTextSearch, false, false, 10);
            entryTextSearch.KeyReleaseEvent += (object? sender, KeyReleaseEventArgs args) =>
            {
                if (args.Event.Key == Gdk.Key.Return || args.Event.Key == Gdk.Key.KP_Enter)
                    Find(entryTextSearch.Text, offset = 0);
            };

            ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Add(vBoxMessage);

            PackStart(scroll, true, true, 0);

            ShowAll();
        }

        public async void Find(string findtext, uint offset = 0)
        {
            entryTextSearch.Text = findtext;

            var recordResult = await Kernel.DataBase.SpetialTableFullTextSearchSelect(findtext, offset, Kernel.Conf.DictTSearch);

            foreach (Widget Child in vBoxMessage.Children)
                vBoxMessage.Remove(Child);

            if (recordResult != null)
            {
                count = recordResult.ListRow.Count;

                CreatePagination();

                foreach (Dictionary<string, object> row in recordResult.ListRow)
                    CreateMessage(row);

                CreatePagination();
            }

            vBoxMessage.ShowAll();
        }

        #region Функції

        Widget? CreateCompositControl(string caption, UuidAndText uuidAndText)
        {
            object? compositControlInstance = ExecutingAssembly.CreateInstance($"{NameSpageProgram}.CompositePointerControl");
            if (compositControlInstance != null)
            {
                dynamic compositControl = compositControlInstance;

                compositControl.Caption = caption;
                compositControl.ClearSensetive = false;
                compositControl.TypeSelectSensetive = false;
                compositControl.Pointer = uuidAndText;

                return compositControl;
            }
            else
                return null;
        }

        #endregion

        void CreateMessage(Dictionary<string, object> row)
        {
            Widget? widgetObj = CreateCompositControl("", (UuidAndText)row["obj"]);
            if (widgetObj != null)
            {
                CompositePointerControl Обєкт = (CompositePointerControl)widgetObj;

                Box hBoxRowInfo = new Box(Orientation.Horizontal, 0);
                vBoxMessage.PackStart(hBoxRowInfo, false, false, 3);
                hBoxRowInfo.PackStart(new Label(row["value"].ToString()) { UseMarkup = true, Wrap = true, Selectable = true }, false, false, 12);

                Box hBoxRowType = new Box(Orientation.Horizontal, 0);
                vBoxMessage.PackStart(hBoxRowType, false, false, 3);
                hBoxRowType.PackStart(new Label("<small>" + Обєкт.PointerName + ": " + Обєкт.TypeCaption + "</small>") { UseMarkup = true, Selectable = true, UseUnderline = false }, false, false, 12);
                hBoxRowType.PackStart(new Label("<small>Додано: " + row["dateadd"].ToString() + "</small>") { UseMarkup = true, Selectable = true, UseUnderline = false }, false, false, 12);

                Box hBoxRowControl = new Box(Orientation.Horizontal, 0);
                vBoxMessage.PackStart(hBoxRowControl, false, false, 3);
                hBoxRowControl.PackStart(Обєкт, false, false, 0);

                vBoxMessage.PackStart(new Separator(Orientation.Horizontal), false, false, 0);
            }
        }

        void CreatePagination()
        {
            Box hBoxPagination = new Box(Orientation.Horizontal, 0) { Halign = Align.Center };

            if (offset >= maxRowsToPage)
            {
                LinkButton linkButtonLast = new LinkButton("", " Попередня сторінка") { Halign = Align.Start, Image = new Image(Stock.GoBack, IconSize.Button), AlwaysShowImage = true };
                linkButtonLast.Clicked += (object? sender, EventArgs args) =>
                {
                    if (offset >= maxRowsToPage) offset -= maxRowsToPage;
                    Find(entryTextSearch.Text, offset);
                };

                hBoxPagination.PackStart(linkButtonLast, false, false, 0);
            }

            if (count == maxRowsToPage)
            {
                LinkButton linkButtonNext = new LinkButton("", " Наступна сторінка") { Halign = Align.Start, Image = new Image(Stock.GoForward, IconSize.Button), AlwaysShowImage = true, ImagePosition = PositionType.Right };
                linkButtonNext.Clicked += (object? sender, EventArgs args) =>
                {
                    Find(entryTextSearch.Text, offset += maxRowsToPage);
                };

                hBoxPagination.PackStart(linkButtonNext, false, false, 0);
            }

            vBoxMessage.PackStart(hBoxPagination, false, false, 10);
        }
    }
}