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
    Kernel Kernel { get; set; }

    protected Notebook Notebook;
    protected Statusbar StatusBar;

    public FormGeneral(Application? app, Kernel kernel) : base()
    {
        Application = app;
        Kernel = kernel;

        SetDefaultSize(1200, 900);
        SetIconName("gtk");
        Maximized = true;

        //HeaderBar
        {
            HeaderBar headerBar = HeaderBar.New();
            headerBar.TitleWidget = Label.New($"{Kernel.Conf.Name} - {Kernel.Conf.Subtitle}");
            Titlebar = headerBar;

            //Блок кнопок у шапці головного вікна
            {
                //Повідомлення
                {
                    Button button = Button.New();
                    button.Child = Image.NewFromIconName("messaging-app");
                    button.TooltipText = "Повідомлення";
                    button.OnClicked += (sender, args) => ButtonMessageClicked();
                    headerBar.PackEnd(button);
                }

                //Повнотекстовий пошук
                {
                    Button button = Button.New();
                    button.Child = Image.NewFromIconName("find");
                    button.TooltipText = "Пошук";
                    button.OnClicked += OnButtonFindClicked;
                    headerBar.PackEnd(button);
                }
            }
        }

        Box vBox = Box.New(Orientation.Vertical, 0);
        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        CreateLeftMenu(hBox);

        hBox.Append(Notebook = NotebookFunction.CreateNotebook(true, true));
        vBox.Append(StatusBar = Statusbar.New());

        Child = vBox;

        //NotebookFunction.ConnectingToKernelObjectChangeEvents(Notebook, kernel);
    }

    #region FullTextSearch

    void OnButtonFindClicked(Button buttonFind, EventArgs args)
    {
        Popover popoverFind = Popover.New();
        popoverFind.Position = PositionType.Bottom;
        popoverFind.SetParent(buttonFind);

        SearchEntry entry = new SearchEntry() { WidthRequest = 500 };

        EventControllerKey eventControllerKey = EventControllerKey.New();
        entry.AddController(eventControllerKey);
        eventControllerKey.OnKeyReleased += (sender, args) =>
        {
            /*
            if (args.Event.Key == Gdk.Key.Return || args.Event.Key == Gdk.Key.KP_Enter)
                ButtonFindClicked(((SearchEntry)sender!).Text);
            */
        };

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
        Box vBox = Box.New(Orientation.Vertical, 0);

        ScrolledWindow scroll = new ScrolledWindow();
        scroll.SetPolicy(PolicyType.Never, PolicyType.Never);
        scroll.Child = vBox;

        void Add(string name, Action<Button> action, string image)
        {
            string path = Path.Combine(AppContext.BaseDirectory, $"images/{image}");

            Image img = Image.NewFromFile(path);
            img.SetSizeRequest(48, 48);

            Box hBox = Box.New(Orientation.Horizontal, 5);
            hBox.Append(img);
            hBox.Append(Label.New(name));

            Button button = Button.New();
            button.Cursor = Gdk.Cursor.NewFromName("hand", null);
            button.MarginStart = button.MarginEnd = button.MarginTop = button.MarginBottom = 5;
            button.AddCssClass("left-menu");
            button.Child = hBox;
            button.TooltipText = name;
            button.OnClicked += (sender, args) => action.Invoke(button);

            vBox.Append(button);
        }

        Add("Документи", Документи, "documents.png");
        Add("Журнали", Журнали, "journal.png");
        Add("Звіти", Звіти, "report.png");
        Add("Довідники", Довідники, "directory.png");
        Add("Регістри", Регістри, "register.png");
        Add("Сервіс", Сервіс, "service.png");
        Add("Обробки", Обробки, "working.png");
        Add("Налаштування", Налаштування, "preferences.png");

        hbox.Append(scroll);
    }

    void Документи(Button button)
    {
        Box vBox = Box.New(Orientation.Vertical, 0);

        //Всі Документи
        {
            Box hBox = Box.New(Orientation.Horizontal, 0);
            vBox.Append(hBox);

            Expander expander = Expander.New("Всі документи");
            hBox.Append(expander);

            Box vBoxList = Box.New(Orientation.Vertical, 0);
            expander.Child = vBoxList;

            Label labelCaption = Label.New("Документи");
            labelCaption.MarginTop = labelCaption.MarginBottom = 5;
            vBoxList.Append(labelCaption);

            ListBox listBox = new ListBox() { SelectionMode = SelectionMode.Single };

            GestureClick gesture = GestureClick.New();
            listBox.AddController(gesture);
            gesture.OnPressed += (_, args) =>
            {
                if (args.NPress >= 2)
                {
                    ListBoxRow? selectedRow = listBox.GetSelectedRow();
                    if (selectedRow != null && selectedRow.Name != null)
                        ВідкритиДокументВідповідноДоВиду(selectedRow.Name);
                }
            };

            ScrolledWindow scroll = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, HasFrame = true };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Child = listBox;

            vBoxList.Append(scroll);

            foreach (KeyValuePair<string, ConfigurationDocuments> documents in Kernel.Conf.Documents.OrderBy(x => x.Value.Name))
            {
                string title = string.IsNullOrEmpty(documents.Value.FullName) ? documents.Value.Name : documents.Value.FullName;

                Label label = Label.New(title);
                label.Halign = Align.Start;

                ListBoxRow row = new ListBoxRow() { Name = documents.Key, Child = label };
                listBox.Append(row);
            }
        }

        МенюДокументи(vBox);

        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(button);
        popover.Child = vBox;
        popover.Show();
    }

    void Довідники(Button button)
    {
        Box vBox = Box.New(Orientation.Vertical, 0);

        //Всі Довідники
        {
            Box hBox = Box.New(Orientation.Horizontal, 0);
            vBox.Append(hBox);

            Expander expander = Expander.New("Всі довідники");
            hBox.Append(expander);

            Box vBoxList = Box.New(Orientation.Vertical, 0);
            expander.Child = vBoxList;

            Label labelCaption = Label.New("Довідники");
            labelCaption.MarginTop = labelCaption.MarginBottom = 5;
            vBoxList.Append(labelCaption);

            ListBox listBox = new ListBox() { SelectionMode = SelectionMode.Single };

            GestureClick gesture = GestureClick.New();
            listBox.AddController(gesture);
            gesture.OnPressed += (_, args) =>
            {
                if (args.NPress >= 2)
                {
                    ListBoxRow? selectedRow = listBox.GetSelectedRow();
                    if (selectedRow != null && selectedRow.Name != null)
                        ВідкритиДовідникВідповідноДоВиду(selectedRow.Name);
                }
            };

            ScrolledWindow scroll = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, HasFrame = true };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Child = listBox;

            vBoxList.Append(scroll);

            foreach (KeyValuePair<string, ConfigurationDirectories> directories in Kernel.Conf.Directories.OrderBy(x => x.Value.Name))
            {
                string title = string.IsNullOrEmpty(directories.Value.FullName) ? directories.Value.Name : directories.Value.FullName;

                Label label = Label.New(title);
                label.Halign = Align.Start;

                ListBoxRow row = new ListBoxRow() { Name = directories.Key, Child = label };
                listBox.Append(row);
            }
        }

        МенюДовідники(vBox);

        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(button);
        popover.Child = vBox;
        popover.Show();
    }

    void Журнали(Button button)
    {
        Box vBox = Box.New(Orientation.Vertical, 0);

        //Всі Журнали
        {
            Box hBox = Box.New(Orientation.Horizontal, 0);
            vBox.Append(hBox);

            Expander expander = Expander.New("Всі журнали");
            hBox.Append(expander);

            Box vBoxList = Box.New(Orientation.Vertical, 0);
            expander.Child = vBoxList;

            Label labelCaption = Label.New("Журнали");
            labelCaption.MarginTop = labelCaption.MarginBottom = 5;
            vBoxList.Append(labelCaption);

            ListBox listBox = new ListBox() { SelectionMode = SelectionMode.Single };

            GestureClick gesture = GestureClick.New();
            listBox.AddController(gesture);
            gesture.OnPressed += (_, args) =>
            {
                if (args.NPress >= 2)
                {
                    ListBoxRow? selectedRow = listBox.GetSelectedRow();
                    if (selectedRow != null && selectedRow.Name != null)
                        ВідкритиЖурналВідповідноДоВиду(selectedRow.Name);
                }
            };

            ScrolledWindow scroll = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, HasFrame = true };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Child = listBox;

            vBoxList.Append(scroll);

            foreach (KeyValuePair<string, ConfigurationJournals> journal in Kernel.Conf.Journals.OrderBy(x => x.Value.Name))
            {
                string title = journal.Value.Name;

                Label label = Label.New(title);
                label.Halign = Align.Start;

                ListBoxRow row = new ListBoxRow() { Name = journal.Key, Child = label };
                listBox.Append(row);
            }
        }

        МенюЖурнали(vBox);

        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(button);
        popover.Child = vBox;
        popover.Show();
    }

    void Звіти(Button button)
    {
        Box vBox = Box.New(Orientation.Vertical, 0);

        МенюЗвіти(vBox);

        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(button);
        popover.Child = vBox;
        popover.Show();
    }

    void Регістри(Button button)
    {
        Box vBox = Box.New(Orientation.Vertical, 0);

        //Всі Регістри
        {
            Box hBox = Box.New(Orientation.Horizontal, 0);
            vBox.Append(hBox);

            Expander expanderAll = Expander.New("Всі регістри");
            hBox.Append(expanderAll);

            Box hBoxList = Box.New(Orientation.Horizontal, 0);
            expanderAll.Child = hBoxList;

            //Регістри відомостей
            {
                Box vBoxBlock = Box.New(Orientation.Vertical, 0);
                vBoxBlock.MarginEnd = 5;
                hBoxList.Append(vBoxBlock);

                Label labelCaption = Label.New("Регістри відомостей");
                labelCaption.MarginTop = labelCaption.MarginBottom = 5;
                vBoxBlock.Append(labelCaption);

                ListBox listBox = new ListBox() { SelectionMode = SelectionMode.Single };

                GestureClick gesture = GestureClick.New();
                listBox.AddController(gesture);
                gesture.OnPressed += (_, args) =>
                {
                    if (args.NPress >= 2)
                    {
                        ListBoxRow? selectedRow = listBox.GetSelectedRow();
                        if (selectedRow != null && selectedRow.Name != null)
                            ВідкритиРегістрВідомостейВідповідноДоВиду(selectedRow.Name);
                    }
                };

                ScrolledWindow scroll = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, HasFrame = true };
                scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                scroll.Child = listBox;

                vBoxBlock.Append(scroll);

                foreach (KeyValuePair<string, ConfigurationRegistersInformation> register in Kernel.Conf.RegistersInformation.OrderBy(x => x.Value.Name))
                {
                    string title = string.IsNullOrEmpty(register.Value.FullName) ? register.Value.Name : register.Value.FullName;

                    Label label = Label.New(title);
                    label.Halign = Align.Start;

                    ListBoxRow row = new ListBoxRow() { Name = register.Key, Child = label };
                    listBox.Append(row);
                }
            }

            //Регістри накопичення
            {
                Box vBoxBlock = Box.New(Orientation.Vertical, 0);
                hBoxList.Append(vBoxBlock);

                Label labelCaption = Label.New("Регістри накопичення");
                labelCaption.MarginTop = labelCaption.MarginBottom = 5;
                vBoxBlock.Append(labelCaption);

                ListBox listBox = new ListBox() { SelectionMode = SelectionMode.Single };

                GestureClick gesture = GestureClick.New();
                listBox.AddController(gesture);
                gesture.OnPressed += (_, args) =>
                {
                    if (args.NPress >= 2)
                    {
                        ListBoxRow? selectedRow = listBox.GetSelectedRow();
                        if (selectedRow != null && selectedRow.Name != null)
                            ВідкритиРегістрНакопиченняВідповідноДоВиду(selectedRow.Name);
                    }
                };

                ScrolledWindow scroll = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 300, HasFrame = true };
                scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                scroll.Child = listBox;

                vBoxBlock.Append(scroll);

                foreach (KeyValuePair<string, ConfigurationRegistersAccumulation> register in Kernel.Conf.RegistersAccumulation.OrderBy(x => x.Value.Name))
                {
                    string title = string.IsNullOrEmpty(register.Value.FullName) ? register.Value.Name : register.Value.FullName;

                    Label label = Label.New(title);
                    label.Halign = Align.Start;

                    ListBoxRow row = new ListBoxRow() { Name = register.Key, Child = label };
                    listBox.Append(row);
                }
            }
        }

        МенюРегістри(vBox);

        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(button);
        popover.Child = vBox;
        popover.Show();
    }

    protected abstract void Налаштування(Button button);
    protected abstract void Сервіс(Button button);
    protected abstract void Обробки(Button button);

    #endregion

    #region StatusBar

    public void SetStatusBar()
    {
        StatusBar.Push(1, $" Сервер: {OpenConfigurationParam?.DataBaseServer}, база даних: {OpenConfigurationParam?.DataBaseBaseName}");
    }

    #endregion
}