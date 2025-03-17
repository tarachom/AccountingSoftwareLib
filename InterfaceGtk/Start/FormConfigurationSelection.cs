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

namespace InterfaceGtk
{
    public class FormConfigurationSelection : Window
    {
        TypeForm TypeOpenForm { get; set; } = TypeForm.Configurator;
        Kernel? ProgramKernel { get; set; }
        Kernel? ConfiguratorKernel { get; set; }

        Toolbar? toolBar;
        ListBox listBox;
        Button? buttonOpen;
        Button buttonConfigurator;
        Spinner spinner;

        public FormConfigurationSelection(Kernel? programKernel, Kernel? configuratorKernel, TypeForm typeOpenForm) : base("Вибір бази даних")
        {
            TypeOpenForm = typeOpenForm;
            ProgramKernel = programKernel;
            ConfiguratorKernel = configuratorKernel;

            SetPosition(WindowPosition.Center);
            Resizable = false;
            BorderWidth = 4;

            if (File.Exists(Іконки.ДляФорми.General))
                SetDefaultIconFromFile(Іконки.ДляФорми.General);

            DeleteEvent += delegate { Application.Quit(); };

            Box vBox = new Box(Orientation.Vertical, 0);
            Add(vBox);

            CreateToolbar(vBox);

            Box hBoxContainer = new Box(Orientation.Horizontal, 0);
            vBox.PackStart(hBoxContainer, false, false, 0);

            Box vBoxContainerLeft = new Box(Orientation.Vertical, 0) { WidthRequest = 500 };
            hBoxContainer.PackStart(vBoxContainerLeft, false, false, 0);

            Box vBoxContainerRight = new Box(Orientation.Vertical, 0) { WidthRequest = 100 };
            hBoxContainer.PackStart(vBoxContainerRight, false, false, 0);

            //Список
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                vBoxContainerLeft.PackStart(hBox, false, false, 4);

                ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In };
                scroll.SetSizeRequest(500, 280);
                scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

                listBox = new ListBox { SelectionMode = SelectionMode.Single };
                listBox.ButtonPressEvent += OnListBoxDataBaseButtonPress;
                scroll.Add(listBox);

                hBox.PackStart(scroll, false, false, 4);
            }

            //Кнопка Відкрити
            if (TypeOpenForm == TypeForm.WorkingProgram)
            {
                Box hBoxOpen = new Box(Orientation.Horizontal, 0) { Halign = Align.Start };
                vBoxContainerRight.PackStart(hBoxOpen, false, false, 2);

                buttonOpen = new Button("Відкрити") { WidthRequest = 140 };
                buttonOpen.Clicked += OnButtonOpenClicked;

                hBoxOpen.PackStart(buttonOpen, false, false, 2);

                //Фокус для кнопки Відкрити після відкриття форми
                Shown += (sender, args) => buttonOpen.GrabFocus();
            }

            //Кнопка Конфігуратор
            {
                Box hBoxConfigurator = new Box(Orientation.Horizontal, 0) { Halign = Align.Start };
                vBoxContainerRight.PackStart(hBoxConfigurator, false, false, 2);

                buttonConfigurator = new Button("Конфігуратор") { WidthRequest = 140 };
                buttonConfigurator.Clicked += OnButtonOpenConfiguratorClicked;

                hBoxConfigurator.PackStart(buttonConfigurator, false, false, 2);

                if (TypeOpenForm == TypeForm.Configurator)
                    //Фокус для кнопки Конфігуратор після відкриття форми
                    Shown += (sender, args) => buttonConfigurator.GrabFocus();
            }

            //Spinner
            {
                Box hBoxSpinner = new Box(Orientation.Horizontal, 0) { Halign = Align.Center };
                vBoxContainerRight.PackStart(hBoxSpinner, true, false, 0);

                spinner = new Spinner();
                hBoxSpinner.PackStart(spinner, true, false, 0);
            }

            ShowAll();

            LoadConfigurationParam();
            FillListBoxDataBase();
        }

        #region Virtual Functions

        public virtual async ValueTask<bool> OpenProgram(ConfigurationParam? openConfigurationParam) { return await ValueTask.FromResult(false); }
        public virtual async ValueTask<bool> OpenConfigurator(ConfigurationParam? openConfigurationParam) { return await ValueTask.FromResult(false); }

        #endregion

        void CreateToolbar(Box vBox)
        {
            toolBar = new Toolbar();

            Box hBoxContainerToolbar = new Box(Orientation.Horizontal, 0);
            hBoxContainerToolbar.PackStart(toolBar, true, true, 0);

            ToolButton addButton = new ToolButton(new Image(Stock.Add, IconSize.Menu), "Додати") { TooltipText = "Додати" };
            addButton.Clicked += OnButtonAddClicked;
            toolBar.Add(addButton);

            ToolButton upButton = new ToolButton(new Image(Stock.Edit, IconSize.Menu), "Редагувати") { TooltipText = "Редагувати" };
            upButton.Clicked += OnButtonEditClicked;
            toolBar.Add(upButton);

            ToolButton refreshButton = new ToolButton(new Image(Stock.Copy, IconSize.Menu), "Копіювати") { TooltipText = "Копіювати" };
            refreshButton.Clicked += OnButtonCopyClicked;
            toolBar.Add(refreshButton);

            ToolButton deleteButton = new ToolButton(new Image(Stock.Delete, IconSize.Menu), "Видалити") { TooltipText = "Видалити" };
            deleteButton.Clicked += OnButtonDeleteClicked;
            toolBar.Add(deleteButton);

            vBox.PackStart(hBoxContainerToolbar, false, false, 0);
        }

        void LoadConfigurationParam()
        {
            ConfigurationParamCollection.PathToXML = System.IO.Path.Combine(AppContext.BaseDirectory, "ConfigurationParam.xml");
            ConfigurationParamCollection.LoadConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);
        }

        void FillListBoxDataBase(string selectConfKey = "")
        {
            //Очищення у зворотньому напрямку
            for (int i = listBox.Children.Length - 1; i >= 0; i--)
                listBox.Remove(listBox.Children[i]);

            //Заповнення списку
            foreach (ConfigurationParam itemConfigurationParam in ConfigurationParamCollection.ListConfigurationParam!)
            {
                ListBoxRow row = new ListBoxRow() { Name = itemConfigurationParam.ConfigurationKey };
                row.Add(new Label(itemConfigurationParam.ToString()) { Halign = Align.Start, UseUnderline = false });

                listBox.Add(row);

                if (!string.IsNullOrEmpty(selectConfKey))
                {
                    if (itemConfigurationParam.ConfigurationKey == selectConfKey)
                        listBox.SelectRow(row);
                }
                else
                {
                    if (itemConfigurationParam.Select)
                        listBox.SelectRow(row);
                }
            }

            listBox.ShowAll();

            //Виділення першого елементу в списку
            if (listBox.Children.Length != 0 && listBox.SelectedRow == null)
            {
                ListBoxRow row = (ListBoxRow)listBox.Children[0];
                listBox.SelectRow(row);
            }
        }

        void CallBackUpdate(ConfigurationParam itemConfigurationParam)
        {
            ConfigurationParamCollection.UpdateConfigurationParam(itemConfigurationParam);
            ConfigurationParamCollection.SaveConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);

            FillListBoxDataBase(itemConfigurationParam.ConfigurationKey);
        }

        async void OnButtonOpenClicked(object? sender, EventArgs args)
        {
            if (ProgramKernel == null) return;

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

            bool result = await ProgramKernel.Open(PathToConfXML,
                OpenConfigurationParam.DataBaseServer,
                OpenConfigurationParam.DataBaseLogin,
                OpenConfigurationParam.DataBasePassword,
                OpenConfigurationParam.DataBasePort,
                OpenConfigurationParam.DataBaseBaseName
            );

            if (result)
            {
                // Авторизація
                ResponseType ModalResult = ResponseType.None;

                using (FormLogIn windowFormLogIn = new()
                {
                    TypeOpenForm = TypeForm.WorkingProgram,
                    ProgramKernel = ProgramKernel,
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
                    ProgramKernel.Close();
                    SensitiveWidgets(true);
                    return;
                }

                if (await OpenProgram(ConfigurationParamCollection.GetConfigurationParam(selectedRows[0].Name)))
                    Hide();
            }
            else
                Message.Error(this, "Error: " + ProgramKernel.Exception?.Message);

            SensitiveWidgets(true);
        }

        async void OnButtonOpenConfiguratorClicked(object? sender, EventArgs args)
        {
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
        }

        void SensitiveWidgets(bool value)
        {
            spinner.Active = !value;

            if (toolBar != null)
                toolBar.Sensitive = value;

            listBox.Sensitive = value;

            if (buttonOpen != null)
                buttonOpen.Sensitive = value;

            buttonConfigurator.Sensitive = value;
        }

        void OnListBoxDataBaseButtonPress(object? sender, ButtonPressEventArgs args)
        {
            if (args.Event.Type == Gdk.EventType.DoubleButtonPress)
                OnButtonEditClicked(null, new EventArgs());
        }

        void OnButtonAddClicked(object? sender, EventArgs args)
        {
            ConfigurationParam itemConfigurationParam = ConfigurationParam.New();
            ConfigurationParamCollection.ListConfigurationParam?.Add(itemConfigurationParam);

            ConfigurationParamCollection.SaveConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);
            FillListBoxDataBase(itemConfigurationParam.ConfigurationKey);
        }

        void OnButtonCopyClicked(object? sender, EventArgs args)
        {
            ListBoxRow[] selectedRows = listBox.SelectedRows;

            if (selectedRows.Length != 0)
            {
                ConfigurationParam? itemConfigurationParam = ConfigurationParamCollection.GetConfigurationParam(selectedRows[0].Name);
                if (itemConfigurationParam != null)
                {
                    ConfigurationParam copyConfigurationParam = itemConfigurationParam.Clone();
                    ConfigurationParamCollection.ListConfigurationParam?.Add(copyConfigurationParam);

                    ConfigurationParamCollection.SaveConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);
                    FillListBoxDataBase(itemConfigurationParam.ConfigurationKey);
                }
            }
        }

        void OnButtonDeleteClicked(object? sender, EventArgs args)
        {
            ListBoxRow[] selectedRows = listBox.SelectedRows;

            if (selectedRows.Length != 0)
            {
                if (Message.Request(this, "Видалити?") == ResponseType.Yes)
                    if (ConfigurationParamCollection.RemoveConfigurationParam(selectedRows[0].Name))
                    {
                        ConfigurationParamCollection.SaveConfigurationParamFromXML(ConfigurationParamCollection.PathToXML);
                        FillListBoxDataBase();
                    }
            }
        }

        void OnButtonEditClicked(object? sender, EventArgs args)
        {
            ListBoxRow[] selectedRows = listBox.SelectedRows;

            if (selectedRows.Length != 0)
            {
                FormConfigurationSelectionParam configurationSelectionParam = new()
                {
                    Modal = true,
                    TransientFor = this,
                    Resizable = false,
                    TypeHint = Gdk.WindowTypeHint.Dialog,
                    OpenConfigurationParam = ConfigurationParamCollection.GetConfigurationParam(selectedRows[0].Name),
                    CallBackUpdate = CallBackUpdate
                };

                configurationSelectionParam.Show();
            }
        }
    }
}