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

public abstract class FormConfigurationSelection : Window
{
    TypeForm TypeOpenForm { get; set; } = TypeForm.Configurator;
    Kernel? ProgramKernel { get; set; }
    Kernel? ConfiguratorKernel { get; set; }

    Box? toolbarBox;
    ListBox listBox;
    Button? buttonOpen;
    Button buttonConfigurator;
    Spinner spinner;

    public FormConfigurationSelection(Application? app, Kernel? programKernel, Kernel? configuratorKernel, TypeForm typeOpenForm) : base()
    {
        Application = app;
        Title = "Вибір бази даних";
        Resizable = false;

        SetIconName("gtk");

        ProgramKernel = programKernel;
        ConfiguratorKernel = configuratorKernel;
        TypeOpenForm = typeOpenForm;

        Box vBox = Box.New(Orientation.Vertical, 0);
        vBox.MarginStart = vBox.MarginEnd = vBox.MarginTop = vBox.MarginBottom = 10;

        Toolbar(vBox);

        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        //Список
        {
            ScrolledWindow scroll = new();
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.SetSizeRequest(500, 300);
            scroll.HasFrame = true;

            GestureClick gesture = GestureClick.New();
            gesture.OnPressed += (_, args) => { if (args.NPress >= 2) OnEdit(null, new EventArgs()); };

            listBox = new ListBox { SelectionMode = SelectionMode.Single };
            listBox.AddController(gesture);
            listBox.OnRowActivated += (_, args) => { };
            scroll.Child = listBox;

            hBox.Append(scroll);
        }

        //Кнопки
        {
            Box vBoxButton = Box.New(Orientation.Vertical, 0);
            vBoxButton.MarginStart = 5;
            hBox.Append(vBoxButton);

            //Відкрити
            if (TypeOpenForm == TypeForm.WorkingProgram)
            {
                buttonOpen = Button.NewWithLabel("Відкрити");
                buttonOpen.MarginBottom = 5;
                buttonOpen.OnClicked += OnOpen;

                vBoxButton.Append(buttonOpen);

                //Фокус для кнопки
                OnShow += (_, _) => buttonOpen.GrabFocus();
            }

            //Конфігуратор
            {
                buttonConfigurator = Button.NewWithLabel("Конфігуратор");
                buttonConfigurator.MarginBottom = 5;
                buttonConfigurator.OnClicked += OnOpenConfigurator;

                vBoxButton.Append(buttonConfigurator);

                if (TypeOpenForm == TypeForm.Configurator)
                    OnShow += (_, _) => buttonConfigurator.GrabFocus(); //Фокус для кнопки
            }

            //Spinner
            {
                Box hBoxSpinner = Box.New(Orientation.Horizontal, 0);
                hBoxSpinner.Vexpand = true;
                hBoxSpinner.Halign = Align.Center;
                vBoxButton.Append(hBoxSpinner);

                spinner = new Spinner();
                hBoxSpinner.Append(spinner);
            }
        }

        Child = vBox;

        LoadConfigurationParam();
        FillListBoxDataBase();
    }

    #region Virtual Functions

    public virtual async ValueTask<bool> OpenProgram(ConfigurationParam? openConfigurationParam) { return await ValueTask.FromResult(false); }
    public abstract ValueTask<bool> OpenConfigurator(ConfigurationParam? openConfigurationParam);

    #endregion

    void Toolbar(Box vBox)
    {
        toolbarBox = Box.New(Orientation.Horizontal, 0);
        toolbarBox.MarginBottom = 5;
        vBox.Append(toolbarBox);

        {
            Button button = Button.NewFromIconName("new");
            button.MarginEnd = 5;
            button.TooltipText = "Додати";
            button.OnClicked += OnAdd;
            toolbarBox.Append(button);
        }

        {
            Button button = Button.NewFromIconName("edit");
            button.MarginEnd = 5;
            button.TooltipText = "Редагувати";
            button.OnClicked += OnEdit;
            toolbarBox.Append(button);
        }

        {
            Button button = Button.NewFromIconName("copy");
            button.MarginEnd = 5;
            button.TooltipText = "Копіювати";
            button.OnClicked += OnCopy;
            toolbarBox.Append(button);
        }

        {
            Button button = Button.NewFromIconName("delete");
            button.MarginEnd = 5;
            button.TooltipText = "Видалити";
            button.OnClicked += OnDelete;
            toolbarBox.Append(button);
        }
    }

    static void LoadConfigurationParam()
    {
        ConfigurationParamCollection.PathToXML = Path.Combine(AppContext.BaseDirectory, "ConfigurationParam.xml");
        ConfigurationParamCollection.LoadConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);
    }

    void FillListBoxDataBase(string? selectConfKey = null)
    {
        //Очистка списку
        {
            Widget? child = listBox.GetFirstChild();
            while (child != null)
            {
                Widget? next = child.GetNextSibling();
                listBox.Remove(child);
                child = next;
            }
        }

        //Заповнення списку
        foreach (ConfigurationParam itemConfigurationParam in ConfigurationParamCollection.ListConfigurationParam!)
        {
            Label label = Label.New(itemConfigurationParam.ToString());
            label.Halign = Align.Start;

            ListBoxRow row = new() { Name = itemConfigurationParam.ConfigurationKey, Child = label };
            listBox.Append(row);

            if (!string.IsNullOrEmpty(selectConfKey))
            {
                if (itemConfigurationParam.ConfigurationKey == selectConfKey)
                    listBox.SelectRow(row);
            }
            else if (itemConfigurationParam.Select)
                listBox.SelectRow(row);
        }

        //Виділення першого елементу в списку
        {
            Widget? child = listBox.GetFirstChild();
            if (child != null && listBox.GetSelectedRow() == null)
                listBox.SelectRow((ListBoxRow)child);
        }
    }

    void CallBackUpdate(ConfigurationParam itemConfigurationParam)
    {
        ConfigurationParamCollection.UpdateConfigurationParam(itemConfigurationParam);
        ConfigurationParamCollection.SaveConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);

        FillListBoxDataBase(itemConfigurationParam.ConfigurationKey);
    }

    async void OnOpen(object? sender, EventArgs args)
    {
        if (ProgramKernel == null) return;

        SensitiveWidgets(false);

        ListBoxRow? selectedRow = listBox.GetSelectedRow();
        if (selectedRow != null && selectedRow.Name != null)
        {
            ConfigurationParam? OpenConfigurationParam = ConfigurationParamCollection.GetConfigurationParam(selectedRow.Name);
            if (OpenConfigurationParam != null)
            {
                ConfigurationParamCollection.SelectConfigurationParam(selectedRow.Name);
                ConfigurationParamCollection.SaveConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);

                string PathToConfXML = Path.Combine(AppContext.BaseDirectory, "Confa.xml");

                bool result = await ProgramKernel.Open(PathToConfXML,
                    OpenConfigurationParam.DataBaseServer,
                    OpenConfigurationParam.DataBaseLogin,
                    OpenConfigurationParam.DataBasePassword,
                    OpenConfigurationParam.DataBasePort,
                    OpenConfigurationParam.DataBaseBaseName
                );

                if (result)
                {
                    if (await CheckSystemTables(ProgramKernel)) //Перевірка наявності системних таблиць
                    {
                        FormLogIn windowFormLogIn = new(Application)
                        {
                            TypeOpenForm = TypeForm.WorkingProgram,
                            ProgramKernel = ProgramKernel,
                            TransientFor = this,
                            CallBack_ResponseOk = async () =>
                            {
                                await OpenProgram(ConfigurationParamCollection.GetConfigurationParam(selectedRow.Name));
                                Close();
                            },
                            CallBack_ResponseCancel = ProgramKernel.Close
                        };

                        await windowFormLogIn.SetValue();
                        windowFormLogIn.Show();
                    }
                }
                else
                    Message.Error(Application, this, "Помилка", ProgramKernel.Exception?.Message);
            }
        }

        SensitiveWidgets(true);
    }

    void OnOpenConfigurator(object? sender, EventArgs args)
    {
        /*
        if (ConfiguratorKernel == null) return;

        SensitiveWidgets(false);

        ListBoxRow[] selectedRows = listBox.SelectedRows;
        if (selectedRows.Length == 0)
        {
            SensitiveWidgets(true);
            return;
        }

        ConfigurationParam? OpenConfigurationParam = ConfigurationParamCollection.GetConfigurationParam(selectedRows[0].Name);
        if (OpenConfigurationParam == null)
        {
            SensitiveWidgets(true);
            return;
        }

        ConfigurationParamCollection.SelectConfigurationParam(selectedRows[0].Name);
        ConfigurationParamCollection.SaveConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);

        string PathToConfXML = System.IO.Path.Combine(AppContext.BaseDirectory, "Confa.xml");

        bool result = await ConfiguratorKernel.Open(PathToConfXML,
            OpenConfigurationParam.DataBaseServer,
            OpenConfigurationParam.DataBaseLogin,
            OpenConfigurationParam.DataBasePassword,
            OpenConfigurationParam.DataBasePort,
            OpenConfigurationParam.DataBaseBaseName
        );

        if (result)
        {
            //Перевірка і створення системних таблиць
            await ConfiguratorKernel.DataBase.CreateSpecialTables();

            // Авторизація
            ResponseType ModalResult = ResponseType.None;

            using (FormLogIn windowFormLogIn = new()
            {
                TypeOpenForm = TypeForm.Configurator,
                ProgramKernel = ConfiguratorKernel,
                TransientFor = this,
                Modal = true,
                Resizable = false,
                TypeHint = Gdk.WindowTypeHint.Dialog
            })
            {
                await windowFormLogIn.SetValue();
                windowFormLogIn.Show();

                while (ModalResult == ResponseType.None)
                {
                    ModalResult = windowFormLogIn.ModalResult;
                    Application.RunIteration(true);
                }
            }

            if (ModalResult == ResponseType.Cancel)
            {
                ConfiguratorKernel.Close();
                SensitiveWidgets(true);
                return;
            }

            if (await OpenConfigurator(ConfigurationParamCollection.GetConfigurationParam(selectedRows[0].Name)))
                Hide();
        }
        else
            Message.Error(this, "Error: " + ConfiguratorKernel.Exception?.Message);

        SensitiveWidgets(true);
        */
    }

    /// <summary>
    /// Перевірка наявності системних таблиць
    /// </summary>
    /// <param name="kernel">Ядро</param>
    /// <returns>true якщо всі перевірки пройдено</returns>
    async ValueTask<bool> CheckSystemTables(Kernel kernel)
    {
        string help = "\n\nПотрібно відкрити Конфігуратор і зберегти конфігурацію - " +
        "(Меню: Конфігурація/Зберегти конфігурацію - дальше Збереження змін. Крок 1, Збереження змін. Крок 2)";

        if (!await kernel.DataBase.IfExistsTable(SpecialTables.Constants))
        {
            Message.Error(Application, this, "Помилка", $"Відсутня таблиця tab_constants.{help}");
            return false;
        }

        //Список системних таблиць
        List<string> specialTable = await kernel.DataBase.GetSpecialTableList();

        foreach (string table in SpecialTables.SpecialTablesList)
            if (!specialTable.Contains(SpecialTables.Users))
            {
                Message.Error(Application, this, "Помилка", $"Відсутня системна таблиця {table}.{help}");
                return false;
            }

        return true;
    }

    void SensitiveWidgets(bool value)
    {
        spinner.Spinning = !value;
        toolbarBox?.Sensitive = value;
        listBox.Sensitive = value;
        buttonOpen?.Sensitive = value;
        buttonConfigurator.Sensitive = value;
    }

    void OnAdd(Button button, EventArgs args)
    {
        ConfigurationParam itemConfigurationParam = ConfigurationParam.New();
        ConfigurationParamCollection.ListConfigurationParam?.Add(itemConfigurationParam);

        ConfigurationParamCollection.SaveConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);
        FillListBoxDataBase(itemConfigurationParam.ConfigurationKey);
    }

    void OnCopy(Button button, EventArgs args)
    {
        ListBoxRow? selectedRow = listBox.GetSelectedRow();
        if (selectedRow != null && selectedRow.Name != null)
        {
            ConfigurationParam? itemConfigurationParam = ConfigurationParamCollection.GetConfigurationParam(selectedRow.Name);
            if (itemConfigurationParam != null)
            {
                ConfigurationParam copyConfigurationParam = itemConfigurationParam.Clone();
                ConfigurationParamCollection.ListConfigurationParam?.Add(copyConfigurationParam);

                ConfigurationParamCollection.SaveConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);
                FillListBoxDataBase(itemConfigurationParam.ConfigurationKey);
            }
        }
    }

    void OnDelete(Button button, EventArgs args)
    {
        ListBoxRow? selectedRow = listBox.GetSelectedRow();
        if (selectedRow != null && selectedRow.Name != null)
            Message.Request(Application, this, "Видалити?", "Видалити підключення до бази даних?", x =>
            {
                if (x == Message.YesNo.Yes)
                    if (ConfigurationParamCollection.RemoveConfigurationParam(selectedRow.Name))
                    {
                        ConfigurationParamCollection.SaveConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);
                        FillListBoxDataBase();
                    }
            });
    }

    void OnEdit(Button? button, EventArgs args)
    {
        ListBoxRow? selectedRow = listBox.GetSelectedRow();
        if (selectedRow != null && selectedRow.Name != null)
        {
            FormConfigurationSelectionParam configurationSelectionParam = new(Application)
            {
                TransientFor = this,
                OpenConfigurationParam = ConfigurationParamCollection.GetConfigurationParam(selectedRow.Name),
                CallBackUpdate = CallBackUpdate
            };

            configurationSelectionParam.SetValue();
            configurationSelectionParam.Show();
        }
    }
}