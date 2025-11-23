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

class FormConfigurationSelectionParam : Window
{
    public ConfigurationParam? OpenConfigurationParam { get; set; }
    public Action<ConfigurationParam>? CallBackUpdate { get; set; }

    #region Fields

    Entry ConfName, Server, Port, Login, Basename;
    PasswordEntry Password;

    #endregion

    public FormConfigurationSelectionParam(Application? app) : base()
    {
        Application = app;
        Title = "Параметри підключення PostgreSQL";
        Resizable = false;
        Modal = true;

        SetIconName("gtk");

        EventControllerKey eventControllerKey = EventControllerKey.New();
        eventControllerKey.OnKeyReleased += (_, args) => { if (args.Keycode == 9) OnCancel(this, new EventArgs()); };
        AddController(eventControllerKey);

        Box vBox = Box.New(Orientation.Vertical, 0);
        vBox.MarginStart = vBox.MarginEnd = vBox.MarginTop = vBox.MarginBottom = 10;

        //Таблиця з полями
        {
            Grid grid = Grid.New();
            grid.ColumnSpacing = grid.RowSpacing = 10;
            vBox.Append(grid);

            int row = 0;

            //Назва
            {
                Label label = Label.New("Назва:");
                label.Halign = Align.End;
                grid.Attach(label, 0, row, 1, 1);

                ConfName = new Entry() { WidthRequest = 300 };
                grid.Attach(ConfName, 1, row, 1, 1);
            }

            //Сервер
            {
                row++;

                Label label = Label.New("Сервер:");
                label.Halign = Align.End;
                grid.Attach(label, 0, row, 1, 1);

                Server = new Entry() { WidthRequest = 300 };
                grid.Attach(Server, 1, row, 1, 1);
            }

            //Порт
            {
                row++;

                Label label = Label.New("Порт:");
                label.Halign = Align.End;
                grid.Attach(label, 0, row, 1, 1);

                Port = new Entry() { WidthRequest = 300 };
                grid.Attach(Port, 1, row, 1, 1);
            }

            //Логін
            {
                row++;

                Label label = Label.New("Логін:");
                label.Halign = Align.End;
                grid.Attach(label, 0, row, 1, 1);

                Login = new Entry() { WidthRequest = 300 };
                grid.Attach(Login, 1, row, 1, 1);
            }

            //Пароль
            {
                row++;

                Label label = Label.New("Пароль:");
                label.Halign = Align.End;
                grid.Attach(label, 0, row, 1, 1);

                Password = new PasswordEntry() { WidthRequest = 300, ShowPeekIcon = true };
                grid.Attach(Password, 1, row, 1, 1);
            }

            //База даних
            {
                row++;

                Label label = Label.New("База даних:");
                label.Halign = Align.End;
                grid.Attach(label, 0, row, 1, 1);

                Basename = new Entry() { WidthRequest = 300 };
                grid.Attach(Basename, 1, row, 1, 1);
            }
        }

        Separator separator = Separator.New(Orientation.Vertical);
        separator.MarginTop = separator.MarginBottom = 10;
        vBox.Append(separator);

        //Кнопки
        {
            Box hBox = Box.New(Orientation.Horizontal, 0);
            hBox.Halign = Align.Center;
            vBox.Append(hBox);

            {
                Button button = Button.NewWithLabel("Зберегти");
                button.MarginStart = button.MarginEnd = 3;
                button.OnClicked += OnButtonSaveClicked;
                hBox.Append(button);
            }

            {
                Button button = Button.NewWithLabel("Створити базу даних");
                button.MarginStart = button.MarginEnd = 3;
                button.OnClicked += OnButtonCreateBaseClicked;
                hBox.Append(button);
            }

            {
                Button button = Button.NewWithLabel("Закрити");
                button.MarginStart = button.MarginEnd = 3;
                button.OnClicked += OnCancel;
                hBox.Append(button);
            }
        }

        Child = vBox;
    }

    public void SetValue()
    {
        if (OpenConfigurationParam != null)
        {
            ConfName.Text_ = OpenConfigurationParam.ConfigurationName;
            Server.Text_ = OpenConfigurationParam.DataBaseServer;
            Port.Text_ = OpenConfigurationParam.DataBasePort.ToString();
            Login.Text_ = OpenConfigurationParam.DataBaseLogin;
            Password.Text_ = OpenConfigurationParam.DataBasePassword;
            Basename.Text_ = OpenConfigurationParam.DataBaseBaseName;
        }
    }

    bool SaveConfParam()
    {
        if (!int.TryParse(Port.Text_, out int portInteger))
        {
            Message.Error(Application, this, "Помилка", "Порт має бути цілим числом!");
            return false;
        }

        if (OpenConfigurationParam != null && CallBackUpdate != null)
        {
            OpenConfigurationParam.ConfigurationName = ConfName.GetText();
            OpenConfigurationParam.DataBaseServer = Server.GetText();
            OpenConfigurationParam.DataBaseLogin = Login.GetText();
            OpenConfigurationParam.DataBasePassword = Password.GetText();
            OpenConfigurationParam.DataBasePort = portInteger;
            OpenConfigurationParam.DataBaseBaseName = Basename.GetText();

            CallBackUpdate.Invoke(OpenConfigurationParam);

            return true;
        }
        else
            return false;
    }

    void OnButtonSaveClicked(Button button, EventArgs args)
    {
        if (SaveConfParam())
            ThisClose();
    }

    async void OnButtonCreateBaseClicked(Button button, EventArgs args)
    {
        if (OpenConfigurationParam == null) return;

        if (SaveConfParam())
        {
            button.Sensitive = false;

            Kernel kernel = new();

            bool ifExistsDatabase = await kernel.IfExistDatabase(
                OpenConfigurationParam.DataBaseServer,
                OpenConfigurationParam.DataBaseLogin,
                OpenConfigurationParam.DataBasePassword,
                OpenConfigurationParam.DataBasePort,
                OpenConfigurationParam.DataBaseBaseName
            );

            if (ifExistsDatabase)
                Message.Info(Application, this, "Повідомлення", "База даних вже існує");
            else
            {
                bool result = await kernel.CreateDatabaseIfNotExist(
                    OpenConfigurationParam.DataBaseServer,
                    OpenConfigurationParam.DataBaseLogin,
                    OpenConfigurationParam.DataBasePassword,
                    OpenConfigurationParam.DataBasePort,
                    OpenConfigurationParam.DataBaseBaseName
                );

                if (result)
                    Message.Info(Application, this, "Повідомлення", "OK.\n\nБаза даних створена");
                else
                    Message.Error(Application, this, "Помилка", kernel.Exception?.Message);
            }

            button.Sensitive = true;
        }
    }

    void OnCancel(object? sender, EventArgs args)
    {
        ThisClose();
    }

    void ThisClose()
    {
        this.Close();
        this.Destroy();
    }
}