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
    class FormConfigurationSelectionParam : Window
    {
        #region Fields
        Entry ConfName = new Entry();
        Entry Server = new Entry();
        Entry Port = new Entry();
        Entry Login = new Entry();
        Entry Password = new Entry();
        Entry Basename = new Entry();
        #endregion

        Button buttonCreateBase;

        public FormConfigurationSelectionParam() : base("Параметри підключення PostgreSQL")
        {
            SetDefaultSize(420, 0);
            SetPosition(WindowPosition.Center);

            DeleteEvent += delegate { ThisClose(); };
            KeyReleaseEvent += OnKeyReleaseEventWindow;
            BorderWidth = 5;

            if (File.Exists(Іконки.ДляФорми.Configurator))
                SetDefaultIconFromFile(Іконки.ДляФорми.Configurator);            

            Box vbox = new Box(Orientation.Vertical, 0);

            AddNameAndField(vbox, "Назва", ConfName);
            AddNameAndField(vbox, "Сервер", Server);
            AddNameAndField(vbox, "Порт", Port);
            AddNameAndField(vbox, "Логін", Login);
            AddNameAndField(vbox, "Пароль", Password);
            AddNameAndField(vbox, "База даних", Basename);

            Separator separator = new Separator(Orientation.Vertical);
            vbox.PackStart(separator, false, false, 5);

            Box hBoxButton = new Box(Orientation.Horizontal, 0);

            Button buttonSave = new Button("Зберегти");
            buttonSave.SetSizeRequest(0, 35);
            buttonSave.Clicked += OnButtonSaveClicked;

            buttonCreateBase = new Button("Створити базу даних");
            buttonCreateBase.SetSizeRequest(0, 35);
            buttonCreateBase.Clicked += OnButtonCreateBaseClicked;

            Button buttonClose = new Button("Закрити");
            buttonClose.SetSizeRequest(0, 35);
            buttonClose.Clicked += OnCancel;

            hBoxButton.PackStart(buttonSave, false, false, 5);
            hBoxButton.PackStart(buttonCreateBase, false, false, 5);
            hBoxButton.PackStart(buttonClose, false, false, 5);

            vbox.PackStart(hBoxButton, false, false, 5);

            Add(vbox);
            ShowAll();
        }

        public System.Action<ConfigurationParam>? CallBackUpdate { get; set; }

        private ConfigurationParam? mOpenConfigurationParam;
        public ConfigurationParam? OpenConfigurationParam
        {
            get { return mOpenConfigurationParam; }
            set
            {
                mOpenConfigurationParam = value;

                if (mOpenConfigurationParam != null)
                {
                    ConfName.Text = mOpenConfigurationParam.ConfigurationName;
                    Server.Text = mOpenConfigurationParam.DataBaseServer;
                    Port.Text = mOpenConfigurationParam.DataBasePort.ToString();
                    Login.Text = mOpenConfigurationParam.DataBaseLogin;
                    Password.Text = mOpenConfigurationParam.DataBasePassword;
                    Basename.Text = mOpenConfigurationParam.DataBaseBaseName;
                }
            }
        }

        private void AddNameAndField(Box vbox, string name, Entry field)
        {
            Fixed fix = new Fixed();
            Label label = new Label(name);

            field.SetSizeRequest(300, 0);

            fix.Put(label, 5, 8);
            fix.Put(field, 100, 0);

            vbox.PackStart(fix, false, false, 5);
        }

        bool SaveConfParam()
        {
            if (!int.TryParse(Port.Text, out int rezult))
            {
                Message.Error(this, "Порт має бути цілим числом!");
                return false;
            }

            if (OpenConfigurationParam != null && CallBackUpdate != null)
            {
                OpenConfigurationParam.ConfigurationName = ConfName.Text;
                OpenConfigurationParam.DataBaseServer = Server.Text;
                OpenConfigurationParam.DataBaseLogin = Login.Text;
                OpenConfigurationParam.DataBasePassword = Password.Text;
                OpenConfigurationParam.DataBasePort = int.Parse(Port.Text);
                OpenConfigurationParam.DataBaseBaseName = Basename.Text;

                CallBackUpdate.Invoke(OpenConfigurationParam);

                return true;
            }
            else
                return false;
        }

        void OnButtonSaveClicked(object? sender, EventArgs args)
        {
            if (SaveConfParam())
                ThisClose();
        }

        async void OnButtonCreateBaseClicked(object? sender, EventArgs args)
        {
            if (OpenConfigurationParam == null)
                return;

            if (SaveConfParam())
            {
                buttonCreateBase.Sensitive = false;

                Kernel kernel = new();

                bool ifExistsDatabase = await kernel.IfExistDatabase(
                    OpenConfigurationParam.DataBaseServer,
                    OpenConfigurationParam.DataBaseLogin,
                    OpenConfigurationParam.DataBasePassword,
                    OpenConfigurationParam.DataBasePort,
                    OpenConfigurationParam.DataBaseBaseName
                );

                if (ifExistsDatabase)
                    Message.Info(this, "База даних вже існує");
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
                        Message.Info(this, "OK.\n\nБаза даних створена");
                    else
                        Message.Error(this, "Error: " + kernel.Exception?.Message);
                }

                buttonCreateBase.Sensitive = true;
            }
        }

        void OnKeyReleaseEventWindow(object? sender, KeyReleaseEventArgs args)
        {
            switch (args.Event.Key)
            {
                case Gdk.Key.Escape:
                    {
                        OnCancel(this, new EventArgs());
                        break;
                    }
            }
        }


        void OnCancel(object? sender, EventArgs args)
        {
            ThisClose();
        }

        void ThisClose()
        {
            this.Close();
            this.Dispose();
            this.Destroy();
        }
    }
}