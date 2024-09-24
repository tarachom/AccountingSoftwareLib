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
    public abstract class FormGeneral : Window
    {
        public ConfigurationParam? OpenConfigurationParam { get; set; }
        private Kernel Kernel { get; set; }

        public Notebook Notebook = NotebookFunction.CreateNotebook();
        protected Statusbar StatusBar = new Statusbar();

        public Button ButtonMessage;


        public FormGeneral(Kernel kernel) : base("")
        {
            Kernel = kernel;

            SetDefaultSize(1200, 900);
            SetPosition(WindowPosition.Center);
            Maximize();

            DeleteEvent += delegate { Application.Quit(); };

            if (File.Exists(Іконки.ДляФорми.General))
                SetDefaultIconFromFile(Іконки.ДляФорми.General);

            //HeaderBar
            {
                HeaderBar headerBar = new HeaderBar()
                {
                    Title = Kernel.Conf.Name,
                    Subtitle = Kernel.Conf.Subtitle,
                    ShowCloseButton = true
                };

                //Блок кнопок у шапці головного вікна
                {
                    //Повідомлення
                    ButtonMessage = new Button() { Image = new Image(Stock.Index, IconSize.Button), TooltipText = "Повідомлення" };
                    ButtonMessage.Clicked += (object? sender, EventArgs args) => ButtonMessageClicked();
                    headerBar.PackEnd(ButtonMessage);

                    //Повнотекстовий пошук
                    Button buttonFind = new Button() { Image = new Image(Stock.Find, IconSize.Button), TooltipText = "Пошук" };
                    buttonFind.Clicked += OnButtonFindClicked;
                    headerBar.PackEnd(buttonFind);
                }

                Titlebar = headerBar;
            }

            Box vBox = new Box(Orientation.Vertical, 0);
            Add(vBox);

            Box hBox = new Box(Orientation.Horizontal, 0);
            vBox.PackStart(hBox, true, true, 0);

            CreateLeftMenu(hBox);

            hBox.PackStart(Notebook, true, true, 0);

            vBox.PackStart(StatusBar, false, false, 0);
            ShowAll();
        }

        public void SetStatusBar()
        {
            StatusBar.Halign = Align.Start;
            StatusBar.Add(new Label($" Сервер: {OpenConfigurationParam?.DataBaseServer} ") { UseUnderline = false });
            StatusBar.Add(new Separator(Orientation.Vertical));
            StatusBar.Add(new Label($" База даних: {OpenConfigurationParam?.DataBaseBaseName} ") { UseUnderline = false });

            StatusBar.ShowAll();
        }

        #region FullTextSearch

        void OnButtonFindClicked(object? sender, EventArgs args)
        {
            Popover PopoverFind = new Popover((Button)sender!) { Position = PositionType.Bottom, BorderWidth = 5 };

            SearchEntry entryFullTextSearch = new SearchEntry() { WidthRequest = 500 };
            entryFullTextSearch.KeyReleaseEvent += (object? sender, KeyReleaseEventArgs args) =>
            {
                if (args.Event.Key == Gdk.Key.Return || args.Event.Key == Gdk.Key.KP_Enter)
                    ButtonFindClicked(((SearchEntry)sender!).Text);
            };

            PopoverFind.Add(entryFullTextSearch);
            PopoverFind.ShowAll();
        }

        #endregion

        #region Virtual & Abstract Function

        protected abstract void ButtonMessageClicked();
        protected abstract void ButtonFindClicked(string text);
        protected abstract void ВідкритиДокументВідповідноДоВиду(string name);
        protected abstract void ВідкритиДовідникВідповідноДоВиду(string name);
        protected abstract void ВідкритиЖурналВідповідноДоВиду(string name);
        protected abstract void ВідкритиРегістрВідомостейВідповідноДоВиду(string name);
        protected abstract void ВідкритиРегістрНакопиченняВідповідноДоВиду(string name);

        protected virtual void МенюДокументи(Box vBox) { }
        protected virtual void МенюДовідники(Box vBox) { }
        protected virtual void МенюЖурнали(Box vBox) { }
        protected virtual void МенюЗвіти(Box vBox) { }
        protected virtual void МенюРегістри(Box vBox) { }

        #endregion

        #region LeftMenu

        void CreateLeftMenu(Box hbox)
        {
            Box vbox = new Box(Orientation.Vertical, 0) { BorderWidth = 0 };

            ScrolledWindow scrolLeftMenu = new ScrolledWindow();
            scrolLeftMenu.SetPolicy(PolicyType.Never, PolicyType.Never);
            scrolLeftMenu.Add(vbox);

            CreateItemLeftMenu(vbox, "Документи", Документи, "images/documents.png");
            CreateItemLeftMenu(vbox, "Журнали", Журнали, "images/journal.png");
            CreateItemLeftMenu(vbox, "Звіти", Звіти, "images/report.png");
            CreateItemLeftMenu(vbox, "Довідники", Довідники, "images/directory.png");
            CreateItemLeftMenu(vbox, "Регістри", Регістри, "images/register.png");
            CreateItemLeftMenu(vbox, "Сервіс", Сервіс, "images/service.png");
            CreateItemLeftMenu(vbox, "Налаштування", Налаштування, "images/preferences.png");

            hbox.PackStart(scrolLeftMenu, false, false, 0);
        }

        void CreateItemLeftMenu(Box vBox, string name, System.Action<LinkButton> ClikAction, string image)
        {
            LinkButton lb = new LinkButton(name, name)
            {
                Halign = Align.Start,
                Image = new Image($"{AppContext.BaseDirectory}{image}"),
                AlwaysShowImage = true
            };

            lb.Image.Valign = Align.End;
            lb.Clicked += (object? sender, EventArgs args) => { ClikAction.Invoke(lb); };

            vBox.PackStart(lb, false, false, 10);
        }

        void Документи(LinkButton lb)
        {
            Box vBox = new Box(Orientation.Vertical, 0);

            //Всі Документи
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                vBox.PackStart(hBox, false, false, 10);

                Expander expander = new Expander("Всі документи");
                hBox.PackStart(expander, false, false, 5);

                Box vBoxList = new Box(Orientation.Vertical, 0);
                expander.Add(vBoxList);

                vBoxList.PackStart(new Label("Документи"), false, false, 2);

                ListBox listBox = new ListBox() { SelectionMode = SelectionMode.Single };
                listBox.ButtonPressEvent += (object? sender, ButtonPressEventArgs args) =>
                {
                    if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                        ВідкритиДокументВідповідноДоВиду(listBox.SelectedRows[0].Name);
                };

                ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, ShadowType = ShadowType.In };
                scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                scrollList.Add(listBox);

                vBoxList.PackStart(scrollList, false, false, 2);

                foreach (KeyValuePair<string, ConfigurationDocuments> documents in Kernel.Conf.Documents)
                {
                    string title = string.IsNullOrEmpty(documents.Value.FullName) ? documents.Value.Name : documents.Value.FullName;

                    ListBoxRow row = new ListBoxRow() { Name = documents.Key };
                    row.Add(new Label(title) { Halign = Align.Start });

                    listBox.Add(row);
                }
            }

            МенюДокументи(vBox);

            Popover popover = new Popover(lb) { Position = PositionType.Right };
            popover.Add(vBox);
            popover.ShowAll();
        }

        void Довідники(LinkButton lb)
        {
            Box vBox = new Box(Orientation.Vertical, 0);

            //Всі Довідники
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                vBox.PackStart(hBox, false, false, 10);

                Expander expander = new Expander("Всі довідники");
                hBox.PackStart(expander, false, false, 5);

                Box vBoxList = new Box(Orientation.Vertical, 0);
                expander.Add(vBoxList);

                vBoxList.PackStart(new Label("Довідники"), false, false, 2);

                ListBox listBox = new ListBox();
                listBox.ButtonPressEvent += (object? sender, ButtonPressEventArgs args) =>
                {
                    if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                        ВідкритиДовідникВідповідноДоВиду(listBox.SelectedRows[0].Name);
                };

                ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, ShadowType = ShadowType.In };
                scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                scrollList.Add(listBox);

                vBoxList.PackStart(scrollList, false, false, 2);

                foreach (KeyValuePair<string, ConfigurationDirectories> directories in Kernel.Conf.Directories)
                {
                    string title = string.IsNullOrEmpty(directories.Value.FullName) ? directories.Value.Name : directories.Value.FullName;

                    ListBoxRow row = new ListBoxRow() { Name = directories.Key };
                    row.Add(new Label(title) { Halign = Align.Start });

                    listBox.Add(row);
                }
            }

            МенюДовідники(vBox);

            Popover popover = new Popover(lb) { Position = PositionType.Right };
            popover.Add(vBox);
            popover.ShowAll();
        }

        void Журнали(LinkButton lb)
        {
            Box vBox = new Box(Orientation.Vertical, 0);

            //Всі Журнали
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                vBox.PackStart(hBox, false, false, 10);

                Expander expanderAll = new Expander("Всі журнали");
                hBox.PackStart(expanderAll, false, false, 5);

                Box vBoxList = new Box(Orientation.Vertical, 0);
                expanderAll.Add(vBoxList);

                vBoxList.PackStart(new Label("Журнали"), false, false, 2);

                ListBox listBox = new ListBox();
                listBox.ButtonPressEvent += (object? sender, ButtonPressEventArgs args) =>
                {
                    if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                        ВідкритиЖурналВідповідноДоВиду(listBox.SelectedRows[0].Name);
                };

                ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, ShadowType = ShadowType.In };
                scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                scrollList.Add(listBox);

                vBoxList.PackStart(scrollList, false, false, 2);

                foreach (KeyValuePair<string, ConfigurationJournals> journal in Kernel.Conf.Journals)
                {
                    string title = journal.Value.Name;

                    ListBoxRow row = new ListBoxRow() { Name = journal.Key };
                    row.Add(new Label(title) { Halign = Align.Start });

                    listBox.Add(row);
                }
            }

            МенюЖурнали(vBox);

            Popover popover = new Popover(lb) { Position = PositionType.Right };
            popover.Add(vBox);
            popover.ShowAll();
        }

        void Звіти(LinkButton lb)
        {
            Box vBox = new Box(Orientation.Vertical, 0);

            МенюЗвіти(vBox);

            Popover popover = new Popover(lb) { Position = PositionType.Right };
            popover.Add(vBox);
            popover.ShowAll();
        }

        void Регістри(LinkButton lb)
        {
            Box vBox = new Box(Orientation.Vertical, 0);

            //Всі Регістри
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                vBox.PackStart(hBox, false, false, 10);

                Expander expanderAll = new Expander("Всі регістри");
                hBox.PackStart(expanderAll, false, false, 5);

                Box hBoxList = new Box(Orientation.Horizontal, 0);
                expanderAll.Add(hBoxList);

                //Регістри відомостей
                {
                    Box vBoxBlock = new Box(Orientation.Vertical, 0);
                    hBoxList.PackStart(vBoxBlock, false, false, 2);

                    vBoxBlock.PackStart(new Label("Регістри відомостей"), false, false, 2);

                    ListBox listBox = new ListBox();
                    listBox.ButtonPressEvent += (object? sender, ButtonPressEventArgs args) =>
                    {
                        if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                            ВідкритиРегістрВідомостейВідповідноДоВиду(listBox.SelectedRows[0].Name);
                    };

                    ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, ShadowType = ShadowType.In };
                    scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                    scrollList.Add(listBox);

                    vBoxBlock.PackStart(scrollList, false, false, 2);

                    foreach (KeyValuePair<string, ConfigurationRegistersInformation> register in Kernel.Conf.RegistersInformation)
                    {
                        string title = string.IsNullOrEmpty(register.Value.FullName) ? register.Value.Name : register.Value.FullName;

                        ListBoxRow row = new ListBoxRow() { Name = register.Key };
                        row.Add(new Label(title) { Halign = Align.Start });

                        listBox.Add(row);
                    }
                }

                //Регістри накопичення
                {
                    Box vBoxBlock = new Box(Orientation.Vertical, 0);
                    hBoxList.PackStart(vBoxBlock, false, false, 2);

                    vBoxBlock.PackStart(new Label("Регістри накопичення"), false, false, 2);

                    ListBox listBox = new ListBox();
                    listBox.ButtonPressEvent += (object? sender, ButtonPressEventArgs args) =>
                    {
                        if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                            ВідкритиРегістрНакопиченняВідповідноДоВиду(listBox.SelectedRows[0].Name);
                    };

                    ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, ShadowType = ShadowType.In };
                    scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                    scrollList.Add(listBox);

                    vBoxBlock.PackStart(scrollList, false, false, 2);

                    foreach (KeyValuePair<string, ConfigurationRegistersAccumulation> register in Kernel.Conf.RegistersAccumulation)
                    {
                        string title = string.IsNullOrEmpty(register.Value.FullName) ? register.Value.Name : register.Value.FullName;

                        ListBoxRow row = new ListBoxRow() { Name = register.Key };
                        row.Add(new Label(title) { Halign = Align.Start });

                        listBox.Add(row);
                    }
                }
            }

            МенюРегістри(vBox);

            Popover popover = new Popover(lb) { Position = PositionType.Right };
            popover.Add(vBox);
            popover.ShowAll();
        }

        protected abstract void Налаштування(LinkButton lb);
        protected abstract void Сервіс(LinkButton lb);

        #endregion
    }
}