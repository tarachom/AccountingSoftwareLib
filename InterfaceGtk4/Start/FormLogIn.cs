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

namespace InterfaceGtk4;

class FormLogIn : Window
{
    public TypeForm TypeOpenForm { get; set; } = TypeForm.Configurator;
    public Kernel? ProgramKernel { get; set; }
    public Action? CallBack_ResponseOk { get; set; }
    public Action? CallBack_ResponseCancel { get; set; }

    ComboBoxText comboBoxAllUsers;
    PasswordEntry passwordUser;

    public FormLogIn(Application? app) : base()
    {
        Application = app;
        Title = "Авторизація";
        Resizable = false;
        Modal = true;

        SetIconName("gtk");

        Box vBox = Box.New(Orientation.Vertical, 0);
        vBox.MarginStart = vBox.MarginEnd = vBox.MarginTop = vBox.MarginBottom = 10;

        Grid grid = Grid.New();
        grid.ColumnSpacing = grid.RowSpacing = 10;
        vBox.Append(grid);

        int row = 0;

        //Користувач
        {
            Label label = Label.New("Користувач:");
            label.Halign = Align.End;
            grid.Attach(label, 0, row, 1, 1);

            comboBoxAllUsers = new ComboBoxText() { WidthRequest = 200 };

            //Заборона прокрутки
            EventControllerScroll controller = EventControllerScroll.New(EventControllerScrollFlags.BothAxes);
            comboBoxAllUsers.AddController(controller);
            controller.OnScroll += (sender, args) => true;

            grid.Attach(comboBoxAllUsers, 1, row, 1, 1);
        }

        //Пароль
        {
            row++;

            Label label = Label.New("Пароль:");
            label.Halign = Align.End;
            grid.Attach(label, 0, row, 1, 1);

            passwordUser = new PasswordEntry() { WidthRequest = 200, ShowPeekIcon = true };

            EventControllerKey eventControllerKey = EventControllerKey.New();
            passwordUser.AddController(eventControllerKey);
            eventControllerKey.OnKeyReleased += (sender, args) =>
            {
                /*
                
                */
            };

            grid.Attach(passwordUser, 1, row, 1, 1);
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
                Button button = Button.NewWithLabel("Авторизація");
                button.MarginStart = button.MarginEnd = 3;
                button.OnClicked += OnLogIn;
                hBox.Append(button);

                //Фокус для кнопки
                OnShow += (_, _) => button.GrabFocus();
            }

            {
                Button button = Button.NewWithLabel("Відмінити");
                button.MarginStart = button.MarginEnd = 3;
                button.OnClicked += OnCancel;
                hBox.Append(button);
            }
        }

        Child = vBox;
    }

    public async ValueTask SetValue()
    {
        if (ProgramKernel != null)
        {
            Dictionary<string, string> allUsers = await ProgramKernel.DataBase.SpetialTableUsersShortSelect();

            foreach (KeyValuePair<string, string> user in allUsers)
                comboBoxAllUsers.Append(user.Key, user.Value);

            comboBoxAllUsers.Active = 0;
        }
    }

    async void OnLogIn(Button button, EventArgs args)
    {
        if (ProgramKernel != null && comboBoxAllUsers.ActiveId != null)
        {
            button.Sensitive = false;

            if (await ProgramKernel.UserLogIn(comboBoxAllUsers.ActiveId, passwordUser.GetText(), TypeOpenForm))
            {
                ProgramKernel.LoopUpdateSession();
                CallBack_ResponseOk?.Invoke();

                ThisClose();
            }
            else
                Message.Error(Application, this, "Помилка", "Невірний пароль");

            button.Sensitive = true;
        }
    }

    void OnCancel(Button button, EventArgs args)
    {
        CallBack_ResponseCancel?.Invoke();
        ThisClose();
    }

    void ThisClose()
    {
        this.Hide();
        this.Destroy();
    }
}