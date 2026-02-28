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
using System.Text.RegularExpressions;

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
        SetIconName("program_logo");
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

    protected virtual async ValueTask PageConstantBlock(string name, bool isNew = false) { }
    protected virtual async ValueTask PageConstant(string name, bool isNew = false) { }
    protected virtual async ValueTask PageDirectory(string name, bool isNew = false) { }
    protected virtual async ValueTask PageDocument(string name, bool isNew = false) { }
    protected virtual async ValueTask PageJournal(string name, bool isNew = false) { }
    protected virtual async ValueTask PageEnum(string name, bool isNew = false) { }
    protected virtual async ValueTask PageRegisterInformation(string name, bool isNew = false) { }
    protected virtual async ValueTask PageRegisterAccumulation(string name, bool isNew = false) { }

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

        hbox.Append(scroll);
    }

    Popover CreatePopover(LinkButton linkButton)
    {
        Popover popover = Popover.New();
        popover.Position = PositionType.Right;
        popover.SetParent(linkButton);
        popover.WidthRequest = 800;
        popover.HeightRequest = 600;
        return popover;
    }

    void Constants(LinkButton linkButton)
    {
        Popover popover = CreatePopover(linkButton);
        popover.Child = getbox();
        popover.Show();

        async void Activate(string group, string name)
        {
            switch (group)
            {
                case "Block":
                    {
                        await PageConstantBlock(name);
                        return;
                    }
                case "Const":
                    {
                        await PageConstant(name);
                        return;
                    }
                case "TablePart":
                    {

                        return;
                    }
                case "TablePartField":
                    {

                        return;
                    }
                default:
                    return;
            }
        }

        Box getbox() => new ConfiguratorConstantsTree(Kernel.Conf, Activate, new()
        {
            Add = async () =>
            {
                await PageDirectory("", true);
            },
            Edit = (group, name) => Activate(group, name),
            Copy = (group, name) =>
            {

            },
            Delete = (group, name) =>
            {

            },
            OpenNewTab = () =>
            {
                //Відкрити окремо
                NotebookFunc.CreatePage("Константи", getbox());
                popover.Hide();
            }
        }).Fill();
    }

    void Directory(LinkButton linkButton)
    {
        Popover popover = CreatePopover(linkButton);
        popover.Child = getbox();
        popover.Show();

        async void Activate(string group, string name)
        {
            switch (group)
            {
                case "Directories":
                    {
                        await PageDirectory(name);
                        return;
                    }
                case "Field":
                    {

                        return;
                    }
                case "TablePart":
                    {

                        return;
                    }
                case "TablePartField":
                    {

                        return;
                    }
                default:
                    return;
            }
        }

        Box getbox() => new ConfiguratorDirectoriesTree(Kernel.Conf, Activate, new()
        {
            Add = async () => await PageDirectory("", true),
            Edit = (group, name) => Activate(group, name),
            Copy = (group, name) =>
            {
                
            },
            Delete = (group, name) =>
            {

            },
            OpenNewTab = () =>
            {
                //Відкрити окремо
                NotebookFunc.CreatePage("Довідники", getbox());
                popover.Hide();
            }
        }).Fill();
    }

    void Documents(LinkButton linkButton)
    {
        Popover popover = CreatePopover(linkButton);
        popover.Child = getbox();
        popover.Show();

        async void Activate(string group, string name)
        {
            switch (group)
            {
                case "Documents":
                    {
                        await PageDocument(name);
                        return;
                    }
                case "Field":
                    {

                        return;
                    }
                case "TablePart":
                    {

                        return;
                    }
                case "TablePartField":
                    {

                        return;
                    }
                default:
                    return;
            }
        }

        Box getbox() => new ConfiguratorDocumentsTree(Kernel.Conf, Activate, new()
        {
            Add = async () => await PageDocument("", true),
            Edit = (group, name) => Activate(group, name),
            Copy = (group, name) =>
            {

            },
            Delete = (group, name) =>
            {

            },
            OpenNewTab = () =>
            {
                //Відкрити окремо
                NotebookFunc.CreatePage("Документи", getbox());
                popover.Hide();
            }
        }).Fill();
    }

    void Journals(LinkButton linkButton)
    {
        Popover popover = CreatePopover(linkButton);
        popover.Child = getbox();
        popover.Show();

        async void Activate(string group, string name)
        {
            switch (group)
            {
                case "Journals":
                    {
                        await PageJournal(name);
                        return;
                    }
                case "Field":
                    {

                        return;
                    }
                default:
                    return;
            }
        }

        Box getbox() => new ConfiguratorJournalsTree(Kernel.Conf, Activate, new ConfiguratorTree.ToolbarAction()
        {
            Add = async () =>
            {
                await PageJournal("", true);
            },
            Edit = (group, name) => Activate(group, name),
            Copy = (group, name) =>
            {

            },
            Delete = (group, name) =>
            {

            },
            OpenNewTab = () =>
            {
                //Відкрити окремо
                NotebookFunc.CreatePage("Журнали", getbox());
                popover.Hide();
            }
        }).Fill();
    }

    void Enums(LinkButton linkButton)
    {
        Popover popover = CreatePopover(linkButton);
        popover.Child = getbox();
        popover.Show();

        async void Activate(string group, string name)
        {
            switch (group)
            {
                case "Enums":
                    {
                        await PageEnum(name);
                        return;
                    }
                case "Field":
                    {

                        return;
                    }
                default:
                    return;
            }
        }

        Box getbox() => new ConfiguratorEnumsTree(Kernel.Conf, Activate, new ConfiguratorTree.ToolbarAction()
        {
            Add = async () => await PageEnum("", true),
            Edit = (group, name) => Activate(group, name),
            Copy = (group, name) =>
            {

            },
            Delete = (group, name) =>
            {

            },
            OpenNewTab = () =>
            {
                //Відкрити окремо
                NotebookFunc.CreatePage("Перелічення", getbox());
                popover.Hide();
            }
        }).Fill();
    }

    void RegistersInformation(LinkButton linkButton)
    {
        Popover popover = CreatePopover(linkButton);
        popover.Child = getbox();
        popover.Show();

        async void Activate(string group, string name)
        {
            switch (group)
            {
                case "RegistersInformation":
                    {
                        await PageRegisterInformation(name);
                        return;
                    }
                case "DimensionField":
                    {

                        return;
                    }
                case "ResourcesField":
                    {

                        return;
                    }
                case "PropertyField":
                    {

                        return;
                    }
                default:
                    return;
            }
        }

        Box getbox() => new ConfiguratorRegistersInformationTree(Kernel.Conf, Activate, new ConfiguratorTree.ToolbarAction()
        {
            Add = async () => await PageRegisterInformation("", true),
            Edit = (group, name) => Activate(group, name),
            Copy = (group, name) =>
            {

            },
            Delete = (group, name) =>
            {

            },
            OpenNewTab = () =>
            {
                //Відкрити в новій вкладці
                NotebookFunc.CreatePage("Регістри інформації", getbox());
                popover.Hide();
            }
        }).Fill();
    }

    void RegistersAccumulation(LinkButton linkButton)
    {
        Popover popover = CreatePopover(linkButton);
        popover.Child = getbox();
        popover.Show();

        async void Activate(string group, string name)
        {
            switch (group)
            {
                case "RegistersAccumulation":
                    {
                        await PageRegisterAccumulation(name);
                        return;
                    }
                case "DimensionField":
                    {

                        return;
                    }
                case "ResourcesField":
                    {

                        return;
                    }
                case "PropertyField":
                    {

                        return;
                    }
                case "TablePart":
                    {

                        return;
                    }
                case "TablePartField":
                    {

                        return;
                    }
                default:
                    return;
            }
        }

        Box getbox() => new ConfiguratorRegistersInformationTree(Kernel.Conf, Activate, new()
        {
            Add = async () => await PageRegisterAccumulation("", true),
            Edit = (group, name) => Activate(group, name),
            Copy = (group, name) =>
            {

            },
            Delete = (group, name) =>
            {

            },
            OpenNewTab = () =>
            {
                //Відкрити окремо
                NotebookFunc.CreatePage("Регістри накопичення", getbox());
                popover.Hide();
            }
        }).Fill();
    }

    #endregion

    #region StatusBar

    public void SetStatusBar()
    {
        StatusBar.Push(1, $" {Kernel.GetCurrentTypeForm()}, сервер: {OpenConfigurationParam?.DataBaseServer}, база даних: {OpenConfigurationParam?.DataBaseBaseName}");
    }

    #endregion
}