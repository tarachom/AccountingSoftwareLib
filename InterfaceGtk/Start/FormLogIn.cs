/*
Copyright (C) 2019-2023 TARAKHOMYN YURIY IVANOVYCH
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
    class FormLogIn : Window
    {
        public ResponseType ModalResult { get; set; } = ResponseType.None;
        public Kernel? ProgramKernel { get; set; }

        ComboBoxText comboBoxAllUsers = new ComboBoxText() { WidthRequest = 200 };
        Entry passwordUser = new Entry() { WidthRequest = 200 };
        Button bLogIn = new Button("Авторизація") { Sensitive = false };
        Button bCancel = new Button("Відмінити");

        public FormLogIn() : base("Авторизація")
        {
            SetPosition(WindowPosition.Center);
            BorderWidth = 5;
            DeleteEvent += delegate { OnCancel(null, new EventArgs()); };

            string ico_file_name = AppContext.BaseDirectory + "images/form.ico";

            if (File.Exists(ico_file_name))
                SetDefaultIconFromFile(ico_file_name);

            VBox vBox = new VBox();

            HBox hBoxLogin = new HBox();
            hBoxLogin.PackStart(new Label("Користувач:"), false, false, 5);
            hBoxLogin.PackEnd(comboBoxAllUsers, false, false, 5);
            vBox.PackStart(hBoxLogin, false, false, 5);

            HBox hBoxPassword = new HBox();
            hBoxPassword.PackStart(new Label("Пароль:"), false, false, 5);
            hBoxPassword.PackEnd(passwordUser, false, false, 5);
            passwordUser.KeyReleaseEvent += OnKeyReleaseEvent;
            vBox.PackStart(hBoxPassword, false, false, 5);

            bLogIn.Clicked += OnLogIn;
            bCancel.Clicked += OnCancel;

            HBox hBoxButton = new HBox();
            hBoxButton.PackStart(bLogIn, false, false, 5);
            hBoxButton.PackStart(bCancel, false, false, 5);
            vBox.PackStart(hBoxButton, false, false, 5);

            Add(vBox);
            ShowAll();
        }

        public async ValueTask SetValue()
        {
            if (ProgramKernel != null)
            {
                Dictionary<string, string> allUsers = await ProgramKernel.DataBase.SpetialTableUsersShortSelect();

                foreach (KeyValuePair<string, string> user in allUsers)
                    comboBoxAllUsers.Append(user.Key, user.Value);

                comboBoxAllUsers.Active = 0;
                bLogIn.Sensitive = true;
            }
        }

        async void OnLogIn(object? sender, EventArgs args)
        {
            if (ProgramKernel != null)
            {
                if (await ProgramKernel.UserLogIn(comboBoxAllUsers.ActiveId, passwordUser.Text))
                {
                    ModalResult = ResponseType.Ok;
                    ThisClose();
                }
                else
                    Message.Error(this, "Невірний пароль");
            }
        }

        void OnCancel(object? sender, EventArgs args)
        {
            ModalResult = ResponseType.Cancel;
            ThisClose();
        }

        void OnKeyReleaseEvent(object? sender, KeyReleaseEventArgs args)
        {
            switch (args.Event.Key)
            {
                case Gdk.Key.KP_Enter:
                case Gdk.Key.Return:
                    {
                        OnLogIn(this, new EventArgs());
                        break;
                    }
            }
        }

        void ThisClose()
        {
            this.Hide();
            this.Dispose();
            this.Destroy();
        }
    }
}