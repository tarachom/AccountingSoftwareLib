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

public abstract class FormConfigurator : Window
{
    public ConfigurationParam? OpenConfigurationParam { get; set; }
    public NotebookFunction NotebookFunc { get; } = new();

    Kernel Kernel { get; set; }
    protected Statusbar StatusBar = Statusbar.New();

    public FormConfigurator(Application? app, Kernel kernel) : base()
    {
        Application = app;
        Kernel = kernel;

        SetDefaultSize(1200, 900);
        SetIconName("gtk");
        Maximized = true;

        //HeaderBar
        {
            HeaderBar headerBar = HeaderBar.New();

            //Назва
            {
                Box box = Box.New(Orientation.Vertical, 0);
                box.Valign = box.Halign = Align.Center;
                Label title = Label.New("КОНФІГУРАТОР");
                title.AddCssClass("title");
                box.Append(title);

                Label subtitle = Label.New(Kernel.Conf.Name + " / " + Kernel.Conf.Subtitle);
                subtitle.AddCssClass("subtitle");
                box.Append(subtitle);

                headerBar.TitleWidget = box;
            }

            Titlebar = headerBar;
        }

        Box vBox = Box.New(Orientation.Vertical, 0);
        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        CreateLeftMenu(hBox);

        //Створення блокноту
        hBox.Append(NotebookFunc.CreateNotebook(this, true));

        vBox.Append(StatusBar);
        SetChild(vBox);
    }

    #region Virtual & Abstract Function

    protected abstract void Service(LinkButton link);
    protected abstract void Settings(LinkButton link);

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

            Label label = Label.New(name);
            label.Wrap = true;
            label.WrapMode = Pango.WrapMode.Word;
            vBoxItem.Append(label);

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

        Add("Константи", Constants, "directory.png");
        Add("Довідники", Directory, "directory.png");
        Add("Документи", Documents, "documents.png");
        Add("Перелічення", Enums, "directory.png");
        Add("Журнали", Journals, "journal.png");
        Add("Регістри інформації", RegistersInformation, "register.png");
        Add("Регістри накопичення", RegistersAccumulation, "register.png");
        Add("Сервіс", Service, "service.png");
        Add("Налаштування", Settings, "preferences.png");

        hbox.Append(scroll);
    }

    const int WidthList = 800, HeightList = 600;

    void Constants(LinkButton linkButton)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);
        popover.WidthRequest = WidthList;
        popover.HeightRequest = HeightList;

        Box vBox = Box.New(Orientation.Vertical, 0);
        popover.Child = vBox;

        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        static void Activate(string group, string name)
        {

        }

        Box GetBox() => new ConfiguratorConstantsTree(Kernel.Conf, Activate).Fill();

        {
            LinkButton link = LinkButton.New("");
            link.Halign = Align.Start;
            link.Label = "Відкрити в окремому вікні";
            link.OnActivateLink += (_, _) =>
            {
                NotebookFunc.CreatePage("Константи", GetBox());
                popover.Hide();

                return true;
            };

            hBox.Append(link);
        }

        vBox.Append(GetBox());
        popover.Show();
    }

    void Directory(LinkButton linkButton)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);
        popover.WidthRequest = WidthList;
        popover.HeightRequest = HeightList;

        Box vBox = Box.New(Orientation.Vertical, 0);
        popover.Child = vBox;

        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        static void Activate(string group, string name)
        {

        }

        Box GetBox() => new ConfiguratorDirectoriesTree(Kernel.Conf, Activate).Fill();

        {
            LinkButton link = LinkButton.New("");
            link.Halign = Align.Start;
            link.Label = "Відкрити в окремому вікні";
            link.OnActivateLink += (_, _) =>
            {
                NotebookFunc.CreatePage("Довідники", GetBox());
                popover.Hide();

                return true;
            };

            hBox.Append(link);
        }

        vBox.Append(GetBox());
        popover.Show();
    }

    void Documents(LinkButton linkButton)
    {
        /*static void Activate(string group, string name)
        {

        }

        Box GetBox() => new ConfiguratorDocumentsTree(Kernel.Conf, Activate).Fill();

        NotebookFunc.CreatePage("Документи", GetBox());*/

        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);
        popover.WidthRequest = WidthList;
        popover.HeightRequest = HeightList;

        Box vBox = Box.New(Orientation.Vertical, 0);
        popover.Child = vBox;

        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        static void Activate(string group, string name)
        {

        }

        Box GetBox() => new ConfiguratorDocumentsTree(Kernel.Conf, Activate).Fill();

        {
            LinkButton link = LinkButton.New("");
            link.Halign = Align.Start;
            link.Label = "Відкрити в окремому вікні";
            link.OnActivateLink += (_, _) =>
            {
                NotebookFunc.CreatePage("Документи", GetBox());
                popover.Hide();

                return true;
            };

            hBox.Append(link);
        }

        vBox.Append(GetBox());
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
            expander.Expanded = true;
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
                    if (selectedRow != null)
                        popover.Hide();
                }
            };

            ScrolledWindow scroll = new() { WidthRequest = WidthList, HeightRequest = HeightList, HasFrame = true };
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

        popover.Show();
    }

    void Enums(LinkButton linkButton)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);
        popover.WidthRequest = WidthList;
        popover.HeightRequest = HeightList;

        Box vBox = Box.New(Orientation.Vertical, 0);
        popover.Child = vBox;

        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        static void Activate(string group, string name)
        {

        }

        Box GetBox() => new ConfiguratorEnumsTree(Kernel.Conf, Activate).Fill();

        {
            LinkButton link = LinkButton.New("");
            link.Halign = Align.Start;
            link.Label = "Відкрити в окремому вікні";
            link.OnActivateLink += (_, _) =>
            {
                NotebookFunc.CreatePage("Перелічення", GetBox());
                popover.Hide();

                return true;
            };

            hBox.Append(link);
        }

        vBox.Append(GetBox());
        popover.Show();
    }

    void RegistersInformation(LinkButton linkButton)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);
        popover.WidthRequest = WidthList;
        popover.HeightRequest = HeightList;

        Box vBox = Box.New(Orientation.Vertical, 0);
        popover.Child = vBox;

        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        static void Activate(string group, string name)
        {

        }

        Box GetBox() => new ConfiguratorRegistersInformationTree(Kernel.Conf, Activate).Fill();

        {
            LinkButton link = LinkButton.New("");
            link.Halign = Align.Start;
            link.Label = "Відкрити в окремому вікні";
            link.OnActivateLink += (_, _) =>
            {
                NotebookFunc.CreatePage("Регістри інформації", GetBox());
                popover.Hide();

                return true;
            };

            hBox.Append(link);
        }

        vBox.Append(GetBox());
        popover.Show();
    }

    void RegistersAccumulation(LinkButton linkButton)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);
        popover.WidthRequest = WidthList;
        popover.HeightRequest = HeightList;

        Box vBox = Box.New(Orientation.Vertical, 0);
        popover.Child = vBox;

        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        static void Activate(string group, string name)
        {

        }

        Box GetBox() => new ConfiguratorRegistersAccumulationTree(Kernel.Conf, Activate).Fill();

        {
            LinkButton link = LinkButton.New("");
            link.Halign = Align.Start;
            link.Label = "Відкрити в окремому вікні";
            link.OnActivateLink += (_, _) =>
            {
                NotebookFunc.CreatePage("Регістри накопичення", GetBox());
                popover.Hide();

                return true;
            };

            hBox.Append(link);
        }

        vBox.Append(GetBox());
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