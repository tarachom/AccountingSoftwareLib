/*
Copyright (C) 2019-2024 TARAKHOMYN YURIY IVANOVYCH
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

namespace InterfaceGtk
{
    public abstract class FormGeneral : Window
    {
        public ConfigurationParam? OpenConfigurationParam { get; set; }

        public Notebook Notebook = NotebookFunction.CreateNotebook();
        protected Statusbar StatusBar = new Statusbar();

        public Button ButtonMessage;
        protected abstract void ButtonMessageClicked();
        protected abstract void ButtonFindClicked(string text);

        public FormGeneral() : base("")
        {
            SetDefaultSize(1200, 900);
            SetPosition(WindowPosition.Center);
            Maximize();

            DeleteEvent += delegate { Application.Quit(); };

            if (File.Exists(Іконки.ДляФорми.General))
                SetDefaultIconFromFile(Іконки.ДляФорми.General);

            //Блок кнопок у шапці головного вікна
            {
                HeaderBar headerBar = new HeaderBar()
                {
                    Title = "\"Зберігання та Торгівля\" для України",
                    Subtitle = "Облік складу, торгівлі та фінансів",
                    ShowCloseButton = true
                };

                //Повідомлення
                ButtonMessage = new Button() { Image = new Image(Stock.Index, IconSize.Button), TooltipText = "Повідомлення" };
                ButtonMessage.Clicked += (object? sender, EventArgs args) => ButtonMessageClicked();
                headerBar.PackEnd(ButtonMessage);

                //Повнотекстовий пошук
                Button buttonFind = new Button() { Image = new Image(Stock.Find, IconSize.Button), TooltipText = "Пошук" };
                buttonFind.Clicked += OnButtonFindClicked;
                headerBar.PackEnd(buttonFind);

                Titlebar = headerBar;
            }

            Box vBox = new Box(Orientation.Vertical, 0);
            Add(vBox);

            Box hBox = new Box(Orientation.Horizontal, 0);
            vBox.PackStart(hBox, true, true, 0);

            CreateLeftMenu(hBox);

            hBox.PackStart(Notebook, true, true, 0);

            vBox.PackStart(StatusBar, false, false, 0);
            ShowAll();
        }

        public void SetStatusBar()
        {
            StatusBar.Halign = Align.Start;
            StatusBar.Add(new Label($" Сервер: {OpenConfigurationParam?.DataBaseServer} ") { UseUnderline = false });
            StatusBar.Add(new Separator(Orientation.Vertical));
            StatusBar.Add(new Label($" База даних: {OpenConfigurationParam?.DataBaseBaseName} ") { UseUnderline = false });

            StatusBar.ShowAll();
        }

        #region FullTextSearch

        void OnButtonFindClicked(object? sender, EventArgs args)
        {
            Popover PopoverFind = new Popover((Button)sender!) { Position = PositionType.Bottom, BorderWidth = 5 };

            SearchEntry entryFullTextSearch = new SearchEntry() { WidthRequest = 500 };
            entryFullTextSearch.KeyReleaseEvent += (object? sender, KeyReleaseEventArgs args) =>
            {
                if (args.Event.Key == Gdk.Key.Return || args.Event.Key == Gdk.Key.KP_Enter)
                    ButtonFindClicked(((SearchEntry)sender!).Text);
            };

            PopoverFind.Add(entryFullTextSearch);
            PopoverFind.ShowAll();
        }

        #endregion

        #region LeftMenu

        void CreateLeftMenu(Box hbox)
        {
            Box vbox = new Box(Orientation.Vertical, 0) { BorderWidth = 0 };

            ScrolledWindow scrolLeftMenu = new ScrolledWindow();
            scrolLeftMenu.SetPolicy(PolicyType.Never, PolicyType.Never);
            scrolLeftMenu.Add(vbox);

            CreateItemLeftMenu(vbox, "Документи", Документи, "images/documents.png");
            CreateItemLeftMenu(vbox, "Журнали", Журнали, "images/journal.png");
            CreateItemLeftMenu(vbox, "Звіти", Звіти, "images/report.png");
            CreateItemLeftMenu(vbox, "Довідники", Довідники, "images/directory.png");           
            CreateItemLeftMenu(vbox, "Регістри", Регістри, "images/register.png");
            CreateItemLeftMenu(vbox, "Сервіс", Сервіс, "images/service.png");
            CreateItemLeftMenu(vbox, "Налаштування", Налаштування, "images/preferences.png");

            hbox.PackStart(scrolLeftMenu, false, false, 0);
        }

        void CreateItemLeftMenu(Box vBox, string name, System.Action<LinkButton> ClikAction, string image)
        {
            LinkButton lb = new LinkButton(name, name)
            {
                Halign = Align.Start,
                Image = new Image($"{AppContext.BaseDirectory}{image}"),
                AlwaysShowImage = true
            };

            lb.Image.Valign = Align.End;
            lb.Clicked += (object? sender, EventArgs args) => { ClikAction.Invoke(lb); };

            vBox.PackStart(lb, false, false, 10);
        }

        protected abstract void Документи(LinkButton lb);
        protected abstract void Журнали(LinkButton lb);
        protected abstract void Звіти(LinkButton lb);
        protected abstract void Довідники(LinkButton lb);
        protected abstract void Регістри(LinkButton lb);
        protected abstract void Налаштування(LinkButton lb);
        protected abstract void Сервіс(LinkButton lb);

        #endregion
    }
}