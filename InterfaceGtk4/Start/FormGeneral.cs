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
using InterfaceGtkLib;

namespace InterfaceGtk4;

public abstract class FormGeneral : Window
{
    public ConfigurationParam? OpenConfigurationParam { get; set; }
    public NotebookFunction NotebookFunc { get; } = new();

    Kernel Kernel { get; set; }
    protected Statusbar StatusBar = Statusbar.New();

    public FormGeneral(Application? app, Kernel kernel) : base()
    {
        Application = app;
        Kernel = kernel;

        SetDefaultSize(1200, 900);
        SetIconName("program_logo");
        Maximized = true;

        //HeaderBar
        {
            HeaderBar headerBar = HeaderBar.New();

            //Назва
            {
                Box box = Box.New(Orientation.Vertical, 0);
                box.Valign = box.Halign = Align.Center;
                Label title = Label.New(Kernel.Conf.Name);
                title.AddCssClass("title");
                box.Append(title);

                Label subtitle = Label.New(Kernel.Conf.Subtitle);
                subtitle.AddCssClass("subtitle");
                box.Append(subtitle);

                headerBar.TitleWidget = box;
            }

            Titlebar = headerBar;

            //Блок кнопок у шапці головного вікна
            {
                //Повідомлення
                {
                    Button button = Button.New();
                    button.Child = Image.NewFromIconName("dialog-information");
                    button.TooltipText = "Повідомлення";
                    button.OnClicked += (sender, args) => ButtonMessageClicked();
                    headerBar.PackEnd(button);
                }

                //Повнотекстовий пошук
                {
                    Button button = Button.New();
                    button.Child = Image.NewFromIconName("edit-find");
                    button.TooltipText = "Пошук";
                    button.OnClicked += OnFindClicked;
                    headerBar.PackEnd(button);
                }
            }
        }

        Box vBox = Box.New(Orientation.Vertical, 0);
        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        CreateLeftMenu(hBox);

        //Створення блокноту
        hBox.Append(NotebookFunc.CreateNotebook(this, true));

        //Приєднання до подій ядра
        NotebookFunc.ConnectingToKernelEvent(kernel);

        vBox.Append(StatusBar);
        SetChild(vBox);
    }

    #region FullTextSearch

    void OnFindClicked(Button button, EventArgs args)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Bottom;
        popover.SetParent(button);

        SearchEntry entry = new() { WidthRequest = 500 };

        EventControllerKey contrKey = EventControllerKey.New();
        entry.AddController(contrKey);
        contrKey.OnKeyReleased += (sender, args) =>
        {
            if (args.Keyval == (uint)Gdk.Key.Return || args.Keyval == (uint)Gdk.Key.KP_Enter)
                ButtonFindClicked(entry.GetText());
        };

        Box hBox = Box.New(Orientation.Horizontal, 0);
        hBox.Append(entry);

        popover.SetChild(hBox);
        popover.Popup();
    }

    #endregion

    #region Virtual & Abstract Function

    protected abstract void ButtonMessageClicked();
    protected abstract void ButtonFindClicked(string text);
    protected abstract bool OpenDocumentByType(string name);
    protected abstract bool OpenDirectoryByType(string name);
    protected abstract bool OpenJournalByType(string name);
    protected abstract bool OpenRegisterInformationByType(string name);
    protected abstract bool OpenRegisterAccumulationByType(string name);
    protected abstract void Settings(LinkButton link);
    protected abstract void Service(LinkButton link);
    protected abstract void Processing(LinkButton link);

    protected virtual void MenuDocuments(Box vBox) { }
    protected virtual void MenuDirectory(Box vBox) { }
    protected virtual void MenuJournals(Box vBox) { }
    protected virtual void MenuReports(Box vBox) { }
    protected virtual void MenuRegisters(Box vBox) { }

    #endregion

    #region LeftMenu

    void CreateLeftMenu(Box hbox)
    {
        Box vBox = Box.New(Orientation.Vertical, 0);

        ScrolledWindow scroll = new();
        scroll.SetPolicy(PolicyType.Never, PolicyType.Never);
        scroll.Child = vBox;

        void Add(string name, Action<LinkButton> action, string image)
        {
            string path = Path.Combine(AppContext.BaseDirectory, $"images/{image}");

            Picture picture = Picture.NewForFilename(path);
            picture.MarginTop = 5;
            picture.SetSizeRequest(48, 48);

            Box hBoxPic = Box.New(Orientation.Horizontal, 0);
            hBoxPic.Halign = Align.Center;
            hBoxPic.Append(picture);

            Box vBoxItem = Box.New(Orientation.Vertical, 0);
            vBoxItem.Append(hBoxPic);
            vBoxItem.Append(Label.New(name));

            LinkButton link = LinkButton.New("");
            link.Child = vBoxItem;
            link.MarginStart = link.MarginEnd = link.MarginTop = link.MarginBottom = 5;
            link.AddCssClass("left-menu");
            link.TooltipText = name;
            link.OnActivateLink += (_, _) =>
            {
                action.Invoke(link);
                return true;
            };

            vBox.Append(link);
        }

        Add("Документи", Documents, "documents.png");
        Add("Журнали", Journals, "journal.png");
        Add("Звіти", Reports, "report.png");
        Add("Довідники", Directory, "directory.png");
        Add("Регістри", Registers, "register.png");
        Add("Сервіс", Service, "service.png");
        Add("Обробки", Processing, "working.png");
        Add("Налаштування", Settings, "preferences.png");

        hbox.Append(scroll);
    }

    void Documents(LinkButton linkButton)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);

        Box vBox = Box.New(Orientation.Vertical, 0);
        popover.Child = vBox;

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

            ListBox listBox = new() { SelectionMode = SelectionMode.Single };

            GestureClick gesture = GestureClick.New();
            listBox.AddController(gesture);
            gesture.OnPressed += (_, args) =>
            {
                if (args.NPress >= 2)
                {
                    ListBoxRow? selectedRow = listBox.GetSelectedRow();
                    if (selectedRow != null && OpenDocumentByType(selectedRow.GetName()))
                        popover.Hide();
                }
            };

            ScrolledWindow scroll = new() { WidthRequest = 300, HeightRequest = 300, HasFrame = true };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Child = listBox;

            vBoxList.Append(scroll);

            foreach (KeyValuePair<string, ConfigurationDocuments> documents in Kernel.Conf.Documents.OrderBy(x => x.Value.Name))
            {
                string title = string.IsNullOrEmpty(documents.Value.FullName) ? documents.Value.Name : documents.Value.FullName;

                Label label = Label.New(title);
                label.Halign = Align.Start;

                ListBoxRow row = new() { Name = documents.Key, Child = label };
                listBox.Append(row);
            }
        }

        MenuDocuments(vBox);

        popover.Show();
    }

    void Directory(LinkButton linkButton)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);

        Box vBox = Box.New(Orientation.Vertical, 0);
        popover.Child = vBox;

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

            ListBox listBox = new() { SelectionMode = SelectionMode.Single };

            GestureClick gesture = GestureClick.New();
            listBox.AddController(gesture);
            gesture.OnPressed += (_, args) =>
            {
                if (args.NPress >= 2)
                {
                    ListBoxRow? selectedRow = listBox.GetSelectedRow();
                    if (selectedRow != null && OpenDirectoryByType(selectedRow.GetName()))
                        popover.Hide();
                }
            };

            ScrolledWindow scroll = new() { WidthRequest = 300, HeightRequest = 300, HasFrame = true };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Child = listBox;

            vBoxList.Append(scroll);

            foreach (KeyValuePair<string, ConfigurationDirectories> directories in Kernel.Conf.Directories.OrderBy(x => x.Value.Name))
            {
                string title = string.IsNullOrEmpty(directories.Value.FullName) ? directories.Value.Name : directories.Value.FullName;

                Label label = Label.New(title);
                label.Halign = Align.Start;

                ListBoxRow row = new() { Name = directories.Key, Child = label };
                listBox.Append(row);
            }
        }

        MenuDirectory(vBox);

        popover.Show();
    }

    void Journals(LinkButton linkButton)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);

        Box vBox = Box.New(Orientation.Vertical, 0);
        popover.Child = vBox;

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

            ListBox listBox = new() { SelectionMode = SelectionMode.Single };

            GestureClick gesture = GestureClick.New();
            listBox.AddController(gesture);
            gesture.OnPressed += (_, args) =>
            {
                if (args.NPress >= 2)
                {
                    ListBoxRow? selectedRow = listBox.GetSelectedRow();
                    if (selectedRow != null && OpenJournalByType(selectedRow.GetName()))
                        popover.Hide();
                }
            };

            ScrolledWindow scroll = new() { WidthRequest = 300, HeightRequest = 300, HasFrame = true };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Child = listBox;

            vBoxList.Append(scroll);

            foreach (KeyValuePair<string, ConfigurationJournals> journal in Kernel.Conf.Journals.OrderBy(x => x.Value.Name))
            {
                string title = journal.Value.Name;

                Label label = Label.New(title);
                label.Halign = Align.Start;

                ListBoxRow row = new() { Name = journal.Key, Child = label };
                listBox.Append(row);
            }
        }

        MenuJournals(vBox);

        popover.Show();
    }

    void Reports(LinkButton linkButton)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);

        Box vBox = Box.New(Orientation.Vertical, 0);
        popover.Child = vBox;

        MenuReports(vBox);

        popover.Show();
    }

    void Registers(LinkButton linkButton)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);

        Box vBox = Box.New(Orientation.Vertical, 0);
        popover.Child = vBox;

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

                ListBox listBox = new() { SelectionMode = SelectionMode.Single };

                GestureClick gesture = GestureClick.New();
                listBox.AddController(gesture);
                gesture.OnPressed += (_, args) =>
                {
                    if (args.NPress >= 2)
                    {
                        ListBoxRow? selectedRow = listBox.GetSelectedRow();
                        if (selectedRow != null && OpenRegisterInformationByType(selectedRow.GetName()))
                            popover.Hide();
                    }
                };

                ScrolledWindow scroll = new() { WidthRequest = 300, HeightRequest = 300, HasFrame = true };
                scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                scroll.Child = listBox;

                vBoxBlock.Append(scroll);

                foreach (KeyValuePair<string, ConfigurationRegistersInformation> register in Kernel.Conf.RegistersInformation.OrderBy(x => x.Value.Name))
                {
                    string title = string.IsNullOrEmpty(register.Value.FullName) ? register.Value.Name : register.Value.FullName;

                    Label label = Label.New(title);
                    label.Halign = Align.Start;

                    ListBoxRow row = new() { Name = register.Key, Child = label };
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

                ListBox listBox = new() { SelectionMode = SelectionMode.Single };

                GestureClick gesture = GestureClick.New();
                listBox.AddController(gesture);
                gesture.OnPressed += (_, args) =>
                {
                    if (args.NPress >= 2)
                    {
                        ListBoxRow? selectedRow = listBox.GetSelectedRow();
                        if (selectedRow != null && OpenRegisterAccumulationByType(selectedRow.GetName()))
                            popover.Hide();
                    }
                };

                ScrolledWindow scroll = new() { WidthRequest = 300, HeightRequest = 300, HasFrame = true };
                scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                scroll.Child = listBox;

                vBoxBlock.Append(scroll);

                foreach (KeyValuePair<string, ConfigurationRegistersAccumulation> register in Kernel.Conf.RegistersAccumulation.OrderBy(x => x.Value.Name))
                {
                    string title = string.IsNullOrEmpty(register.Value.FullName) ? register.Value.Name : register.Value.FullName;

                    Label label = Label.New(title);
                    label.Halign = Align.Start;

                    ListBoxRow row = new() { Name = register.Key, Child = label };
                    listBox.Append(row);
                }
            }
        }

        MenuRegisters(vBox);

        popover.Show();
    }

    #endregion

    #region StatusBar

    public void SetStatusBar()
    {
        StatusBar.Push(1, $" {Kernel.GetCurrentTypeForm()}, сервер: {OpenConfigurationParam?.DataBaseServer}, база даних: {OpenConfigurationParam?.DataBaseBaseName}");
    }

    #endregion
}