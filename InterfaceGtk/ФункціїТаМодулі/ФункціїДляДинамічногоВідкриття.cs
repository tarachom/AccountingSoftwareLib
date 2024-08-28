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

/*

Функції для журналів

*/

using Gtk;
using System.Reflection;
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class ФункціїДляДинамічногоВідкриття
    {
        private string NameSpageProgram { get; set; }
        private string NameSpageCodeGeneration { get; set; }
        private Assembly ExecutingAssembly { get; } = Assembly.GetCallingAssembly();

        public ФункціїДляДинамічногоВідкриття(string nameSpageProgram, string nameSpageCodeGeneration)
        {
            NameSpageProgram = nameSpageProgram;
            NameSpageCodeGeneration = nameSpageCodeGeneration;
        }

        #region Virtual & Abstract Function

        protected abstract void CreateNotebookPage(string tabName, Func<Widget>? pageWidget);

        #endregion

        /// <summary>
        /// Функція відкриває журнал
        /// </summary>
        /// <param name="typeJournal">Тип</param>
        /// <param name="unigueID">Елемент для позиціювання</param>
        public void ВідкритиЖурналВідповідноДоВиду(string typeJournal, UnigueID? unigueID)
        {
            object? journalInstance;

            try
            {
                journalInstance = ExecutingAssembly.CreateInstance($"{NameSpageProgram}.Журнал_{typeJournal}");
            }
            catch (Exception ex)
            {
                Message.Error(null, ex.Message);
                return;
            }

            if (journalInstance != null)
            {
                dynamic journal = journalInstance;

                //Документ який потрібно виділити в списку
                journal.SelectPointerItem = unigueID;
                CreateNotebookPage(typeJournal, () => journal);
                journal.SetValue();
            }
        }

        /// <summary>
        /// Функція відкриває список довідника
        /// </summary>
        /// <param name="typeDir">Тип</param>
        /// <param name="unigueID">Елемент для позиціонування</param>
        public void ВідкритиДовідникВідповідноДоВиду(string typeDir, UnigueID? unigueID)
        {
            object? directoryInstance;

            try
            {
                directoryInstance = ExecutingAssembly.CreateInstance($"{NameSpageProgram}.{typeDir}");
            }
            catch (Exception ex)
            {
                Message.Error(null, ex.Message);
                return;
            }

            if (directoryInstance != null)
            {
                dynamic directory = directoryInstance;

                //Елемент який потрібно виділити в списку
                directory.SelectPointerItem = unigueID;

                //Заголовок журналу
                string listName = "Список";

                Type? documentConst = ExecutingAssembly.GetType($"{NameSpageCodeGeneration}.Довідники.{typeDir}_Const");
                if (documentConst != null)
                    listName = documentConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;

                CreateNotebookPage(listName, () => directory);
                directory.SetValue();
            }
        }

        /// <summary>
        /// Функція відкриває список докуменів
        /// </summary>
        /// <param name="typeDoc">Тип документу</param>
        /// <param name="unigueID">Елемент для позиціювання</param>
        /// <param name="keyForSetting">Додатковий ключ для налаштуваннь користувача</param>
        public void ВідкритиДокументВідповідноДоВиду(string typeDoc, UnigueID? unigueID, string keyForSetting = "")
        {
            object? documentInstance;

            try
            {
                documentInstance = ExecutingAssembly.CreateInstance($"{NameSpageProgram}.{typeDoc}");
            }
            catch (Exception ex)
            {
                Message.Error(null, ex.Message);
                return;
            }

            if (documentInstance != null)
            {
                dynamic document = documentInstance;

                //Елемент який потрібно виділити в списку
                document.SelectPointerItem = unigueID;

                //Заголовок журналу
                string listName = "Список";
                Type? documentConst = ExecutingAssembly.GetType($"{NameSpageCodeGeneration}.Документи.{typeDoc}_Const");
                if (documentConst != null)
                    listName = documentConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;

                //Додатковий ключ для налаштувань
                if (!string.IsNullOrEmpty(keyForSetting))
                    document.KeyForSetting = keyForSetting;

                CreateNotebookPage(listName, () => document);

                document.SetValue();
            }
        }

        /// <summary>
        /// Список документів для журналу у спливаючому вікні
        /// </summary>
        /// <param name="relative_to"></param>
        /// <param name="allowDocument"></param>
        public void ВідкритиСписокДокументівДляЖурналу(Widget relative_to, Dictionary<string, string> allowDocument)
        {
            Box vBox = new Box(Orientation.Vertical, 0);

            foreach (KeyValuePair<string, string> typeDoc in allowDocument)
            {
                LinkButton lb = new LinkButton(typeDoc.Value, typeDoc.Value) { Halign = Align.Start };
                lb.Clicked += (object? sender, EventArgs args) => { ВідкритиДокументВідповідноДоВиду(typeDoc.Key, new UnigueID()); };
                vBox.PackStart(lb, false, false, 0);
            }

            Popover popover = new Popover(relative_to) { Position = PositionType.Bottom, BorderWidth = 2 };

            popover.Add(vBox);
            popover.ShowAll();
        }
    }
}
