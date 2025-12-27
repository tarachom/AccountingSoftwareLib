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

/*

Функції для журналів

*/

using Gtk;
using System.Reflection;
using AccountingSoftware;

namespace InterfaceGtk4;

public abstract class FunctionForDynamicOpen(string namespaceProgram, string namespaceCodeGeneration)
{
    string NamespaceProgram { get; set; } = namespaceProgram;
    string NamespaceCodeGeneration { get; set; } = namespaceCodeGeneration;
    Assembly ExecutingAssembly { get; } = Assembly.GetCallingAssembly();

    #region Virtual & Abstract Function

    protected abstract void CreateNotebookPage(string tabName, Func<Widget>? pageWidget);

    #endregion

    /// <summary>
    /// Функція відкриває журнал
    /// </summary>
    /// <param name="typeJournal">Тип</param>
    /// <param name="unigueID">Елемент для позиціювання</param>
    public void OpenJournalByType(string typeJournal, UnigueID? unigueID)
    {
        object? journalInstance;

        try
        {
            journalInstance = ExecutingAssembly.CreateInstance($"{NamespaceProgram}.Журнал_{typeJournal}");
        }
        catch (Exception ex)
        {
            Message.Error(null, null, ex.Message);
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
    /// Функція відкриває список довідника або сам елемент
    /// </summary>
    /// <param name="typeDir">Тип</param>
    /// <param name="unigueID">Елемент для позиціонування</param>
    /// <param name="typeForm">Тип форми</param>
    public void OpenDirectoryByType(string typeDir, UnigueID? unigueID, TypeForm typeForm = TypeForm.Journal)
    {
        object? directoryInstance;

        switch (typeForm)
        {
            case TypeForm.Journal:
                {
                    try
                    {
                        directoryInstance = ExecutingAssembly.CreateInstance($"{NamespaceProgram}.{typeDir}");
                    }
                    catch (Exception ex)
                    {
                        Message.Error(null, null, ex.Message);
                        return;
                    }

                    if (directoryInstance != null)
                    {
                        dynamic directory = directoryInstance;

                        //Елемент який потрібно виділити в списку
                        directory.SelectPointerItem = unigueID;

                        //Заголовок журналу
                        string listName = "Список";

                        Type? directoryConst = ExecutingAssembly.GetType($"{NamespaceProgram}.Довідники.{typeDir}_Const");
                        if (directoryConst != null)
                            listName = directoryConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;

                        CreateNotebookPage(listName, () => directory);
                        directory.SetValue();
                    }
                    break;
                }
            case TypeForm.Element:
                {
                    Type? directoryFunction = ExecutingAssembly.GetType($"{NamespaceProgram}.{typeDir}_Функції");
                    directoryFunction?.GetMethod("OpenPageElement", BindingFlags.Public | BindingFlags.Static)?.Invoke(directoryFunction, [false, unigueID, null, null]);

                    break;
                }
        }
    }

    /// <summary>
    /// Функція відкриває список докуменів або сам документ
    /// </summary>
    /// <param name="typeDoc">Тип документу</param>
    /// <param name="unigueID">Елемент для позиціювання</param>
    /// <param name="keyForSetting">Додатковий ключ для налаштуваннь користувача</param>
    /// <param name="typeForm">Тип форми</param>
    public void OpenDocumentByType(string typeDoc, UnigueID? unigueID, string keyForSetting = "", TypeForm typeForm = TypeForm.Journal)
    {
        object? documentInstance;

        switch (typeForm)
        {
            case TypeForm.Journal:
                {
                    try
                    {
                        documentInstance = ExecutingAssembly.CreateInstance($"{NamespaceProgram}.{typeDoc}");
                    }
                    catch (Exception ex)
                    {
                        Message.Error(null, null, ex.Message);
                        return;
                    }

                    if (documentInstance != null)
                    {
                        dynamic document = documentInstance;

                        //Елемент який потрібно виділити в списку
                        document.SelectPointerItem = unigueID;

                        //Заголовок журналу
                        string listName = "Список";
                        Type? documentConst = ExecutingAssembly.GetType($"{NamespaceCodeGeneration}.Документи.{typeDoc}_Const");
                        if (documentConst != null)
                            listName = documentConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;

                        //Додатковий ключ для налаштувань
                        if (!string.IsNullOrEmpty(keyForSetting))
                            document.KeyForSetting = keyForSetting;

                        CreateNotebookPage(listName, () => document);

                        document.SetValue();
                    }
                    break;
                }
            case TypeForm.Element:
                {
                    Type? documentFunction = ExecutingAssembly.GetType($"{NamespaceProgram}.{typeDoc}_Функції");
                    documentFunction?.GetMethod("OpenPageElement", BindingFlags.Public | BindingFlags.Static)?.Invoke(documentFunction, [false, unigueID, null]);

                    break;
                }
        }
    }

    /// <summary>
    /// Функція відкриває регістр відомостей
    /// </summary>
    /// <param name="typeReg">Назва</param>
    /// <param name="unigueID">Елемент який потрібно виділити в списку</param>
    public void OpenRegisterInformationByType(string typeReg, UnigueID? unigueID)
    {
        object? registerInstance;

        try
        {
            registerInstance = ExecutingAssembly.CreateInstance($"{NamespaceProgram}.РегістриВідомостей.{typeReg}");
        }
        catch (Exception ex)
        {
            Message.Error(null, null, ex.Message);
            return;
        }

        if (registerInstance != null)
        {
            dynamic register = registerInstance;

            //Елемент який потрібно виділити в списку
            register.SelectPointerItem = unigueID;

            //Заголовок
            string listName = "Список";

            Type? documentConst = ExecutingAssembly.GetType($"{NamespaceCodeGeneration}.РегістриВідомостей.{typeReg}_Const");
            if (documentConst != null)
                listName = documentConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;

            CreateNotebookPage(listName, () => register);
            register.SetValue();
        }
    }

    /// <summary>
    /// Функція відкриває регістр накопичення
    /// </summary>
    /// <param name="typeReg">Назва</param>
    /// <param name="unigueID">Елемент який потрібно виділити в списку</param>
    public void OpenRegisterAccumulationByType(string typeReg, UnigueID? unigueID)
    {
        object? registerInstance;

        try
        {
            registerInstance = ExecutingAssembly.CreateInstance($"{NamespaceProgram}.РегістриНакопичення.{typeReg}");
        }
        catch (Exception ex)
        {
            Message.Error(null, null, ex.Message);
            return;
        }

        if (registerInstance != null)
        {
            dynamic register = registerInstance;

            //Елемент який потрібно виділити в списку
            register.SelectPointerItem = unigueID;

            //Заголовок
            string listName = "Список";

            Type? documentConst = ExecutingAssembly.GetType($"{NamespaceCodeGeneration}.РегістриНакопичення.{typeReg}_Const");
            if (documentConst != null)
                listName = documentConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;

            CreateNotebookPage(listName, () => register);
            register.SetValue();
        }
    }

    /// <summary>
    /// Список документів для журналу у спливаючому вікні
    /// </summary>
    /// <param name="relative_to">Прив'язка Popover</param>
    /// <param name="allowDocument">Колекція</param>
    public void OpenDocumentListForJournal(Widget relative_to, Dictionary<string, string> allowDocument)
    {
        Box vBox = Box.New(Orientation.Vertical, 0);

        foreach (KeyValuePair<string, string> typeDoc in allowDocument)
        {
            LinkButton lb = LinkButton.New(typeDoc.Value);
            lb.Halign = Align.Start;
            lb.OnActivateLink += (sender, args) =>
            {
                OpenDocumentByType(typeDoc.Key, new UnigueID());
                return true;
            };

            vBox.Append(lb);
        }

        Popover popover = Popover.New();
        popover.SetParent(relative_to);
        popover.Position = PositionType.Bottom;
        popover.MarginTop = popover.MarginEnd = popover.MarginBottom = popover.MarginStart = 2;
        popover.SetChild(vBox);

        popover.Show();
    }

    /// <summary>
    /// Тип форми яка відкривається
    /// </summary>
    public enum TypeForm
    {
        Journal,
        Element
    }
}