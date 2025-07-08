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
using InterfaceGtkLib;

namespace InterfaceGtk4;

public abstract class FormGeneral : Window
{
    public ConfigurationParam? OpenConfigurationParam { get; set; }
    //private Kernel Kernel { get; set; }

    public Notebook Notebook = NotebookFunction.CreateNotebook(true, true);
    protected Statusbar StatusBar = new Statusbar();

    public Button ButtonMessage;

    public FormGeneral(Application app/*, Kernel kernel*/) : base()
    {
        //Kernel = kernel;

        Application = app;
        //Title = "EditableLabel";

        SetDefaultSize(1200, 900);

        //if (File.Exists(Іконки.ДляФорми.General)) SetDefaultIconFromFile(Іконки.ДляФорми.General);

        //HeaderBar
        {
            HeaderBar headerBar = HeaderBar.New();

            headerBar.TitleWidget = Label.New("Title"); //Kernel.Conf.Name, Kernel.Conf.Subtitle,
                                                        // headerBar.ShowTitleButtons = true;

            //Блок кнопок у шапці головного вікна
            {
                //Повідомлення
                ButtonMessage = Button.New();
                ButtonMessage.Child = Image.NewFromIconName("application-exit");
                ButtonMessage.TooltipText = "Повідомлення";
                ButtonMessage.OnClicked += (sender, args) => ButtonMessageClicked();
                headerBar.PackEnd(ButtonMessage);

                //Повнотекстовий пошук
                Button buttonFind = Button.New();
                buttonFind.Child = Image.NewFromIconName("application-exit");
                buttonFind.TooltipText = "Пошук";
                buttonFind.OnClicked += OnButtonFindClicked;
                headerBar.PackEnd(buttonFind);
            }

            Titlebar = headerBar;
        }

        Box vBox = Box.New(Orientation.Vertical, 5);
        vBox.MarginTop = vBox.MarginEnd = 5; //vBox.MarginBottom = vBox.MarginStart = vBox.MarginEnd = 5;

        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        CreateLeftMenu(hBox);

        hBox.Append(Notebook);

        //NotebookFunction.ConnectingToKernelObjectChangeEvents(Notebook, kernel);

        vBox.Append(StatusBar);
        Child = vBox;
    }

    public void SetStatusBar()
    {
        StatusBar.Push(1, $" Сервер: {OpenConfigurationParam?.DataBaseServer}, база даних: {OpenConfigurationParam?.DataBaseBaseName}");
    }

    #region FullTextSearch

    void OnButtonFindClicked(Button buttonFind, EventArgs args)
    {
        Popover popoverFind = Popover.New();
        popoverFind.Position = PositionType.Bottom;
        popoverFind.SetParent(buttonFind);

        SearchEntry entry = new SearchEntry() { WidthRequest = 500 };
        // entryFullTextSearch.KeyReleaseEvent += (sender, args) =>
        // {
        //     if (args.Event.Key == Gdk.Key.Return || args.Event.Key == Gdk.Key.KP_Enter)
        //         ButtonFindClicked(((SearchEntry)sender!).Text);
        // };

        Box hBox = Box.New(Orientation.Horizontal, 10);
        hBox.Append(entry);

        popoverFind.SetChild(hBox);
        popoverFind.Popup();
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
        Box vbox = Box.New(Orientation.Vertical, 0);

        ScrolledWindow scroll = new ScrolledWindow();
        scroll.SetPolicy(PolicyType.Never, PolicyType.Never);
        scroll.Child = vbox;

        CreateItemLeftMenu(vbox, "Документи", Документи, "images/documents.png");
        CreateItemLeftMenu(vbox, "Журнали", Журнали, "images/journal.png");
        CreateItemLeftMenu(vbox, "Звіти", Звіти, "images/report.png");
        CreateItemLeftMenu(vbox, "Довідники", Довідники, "images/directory.png");
        CreateItemLeftMenu(vbox, "Регістри", Регістри, "images/register.png");
        CreateItemLeftMenu(vbox, "Сервіс", Сервіс, "images/service.png");
        CreateItemLeftMenu(vbox, "Обробки", Обробки, "images/working.png");
        CreateItemLeftMenu(vbox, "Налаштування", Налаштування, "images/preferences.png");

        hbox.Append(scroll);
    }

    void CreateItemLeftMenu(Box vBox, string name, Action<Button> ClikAction, string image)
    {
        Image img = Image.NewFromFile($"{AppContext.BaseDirectory}{image}");
        img.SetSizeRequest(48, 48);

        Box hBox = Box.New(Orientation.Horizontal, 5);
        hBox.Append(img);
        hBox.Append(Label.New(name));

        Button button = Button.New();
        button.Cursor = Gdk.Cursor.NewFromName("hand", null);
        button.MarginStart = button.MarginEnd = button.MarginTop = button.MarginBottom = 10;
        button.AddCssClass("left-menu");
        button.Child = hBox;
        button.TooltipText = name;
        button.OnClicked += (sender, args) => ClikAction.Invoke(button);

        vBox.Append(button);
    }

    void Документи(Button button)
    {
        /*
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
            listBox.ButtonPressEvent += (sender, args) =>
            {
                if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                    ВідкритиДокументВідповідноДоВиду(listBox.SelectedRows[0].Name);
            };

            ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, ShadowType = ShadowType.In };
            scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollList.Add(listBox);

            vBoxList.PackStart(scrollList, false, false, 2);

            foreach (KeyValuePair<string, ConfigurationDocuments> documents in Kernel.Conf.Documents.OrderBy(x => x.Value.Name))
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
        */
    }

    void Довідники(Button button)
    {
        /*
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
            listBox.ButtonPressEvent += (sender, args) =>
            {
                if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                    ВідкритиДовідникВідповідноДоВиду(listBox.SelectedRows[0].Name);
            };

            ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, ShadowType = ShadowType.In };
            scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollList.Add(listBox);

            vBoxList.PackStart(scrollList, false, false, 2);

            foreach (KeyValuePair<string, ConfigurationDirectories> directories in Kernel.Conf.Directories.OrderBy(x => x.Value.Name))
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
        */
    }

    void Журнали(Button button)
    {
        /*
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
            listBox.ButtonPressEvent += (sender, args) =>
            {
                if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                    ВідкритиЖурналВідповідноДоВиду(listBox.SelectedRows[0].Name);
            };

            ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, ShadowType = ShadowType.In };
            scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollList.Add(listBox);

            vBoxList.PackStart(scrollList, false, false, 2);

            foreach (KeyValuePair<string, ConfigurationJournals> journal in Kernel.Conf.Journals.OrderBy(x => x.Value.Name))
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
        */
    }

    void Звіти(Button button)
    {
        /*
        Box vBox = new Box(Orientation.Vertical, 0);

        МенюЗвіти(vBox);

        Popover popover = new Popover(lb) { Position = PositionType.Right };
        popover.Add(vBox);
        popover.ShowAll();
        */
    }

    void Регістри(Button button)
    {
        /*
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
                listBox.ButtonPressEvent += (sender, args) =>
                {
                    if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                        ВідкритиРегістрВідомостейВідповідноДоВиду(listBox.SelectedRows[0].Name);
                };

                ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, ShadowType = ShadowType.In };
                scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                scrollList.Add(listBox);

                vBoxBlock.PackStart(scrollList, false, false, 2);

                foreach (KeyValuePair<string, ConfigurationRegistersInformation> register in Kernel.Conf.RegistersInformation.OrderBy(x => x.Value.Name))
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
                listBox.ButtonPressEvent += (sender, args) =>
                {
                    if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                        ВідкритиРегістрНакопиченняВідповідноДоВиду(listBox.SelectedRows[0].Name);
                };

                ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, ShadowType = ShadowType.In };
                scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                scrollList.Add(listBox);

                vBoxBlock.PackStart(scrollList, false, false, 2);

                foreach (KeyValuePair<string, ConfigurationRegistersAccumulation> register in Kernel.Conf.RegistersAccumulation.OrderBy(x => x.Value.Name))
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
        */
    }

    protected abstract void Налаштування(Button button);
    protected abstract void Сервіс(Button button);
    protected abstract void Обробки(Button button);

    #endregion
}