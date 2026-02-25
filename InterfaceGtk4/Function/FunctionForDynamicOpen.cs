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

/*

Функції для динамічного відкриття

*/

using Gtk;
using System.Reflection;
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Динамічне відкриття довідників, документів, журналів і регістрів
/// </summary>
/// <param name="namespaceProgram">Простір імен програми</param>
/// <param name="namespaceCodeGeneration">Простір імен згенерованого коду</param>
/// <param name="notebookFunc">Функції управління блокнотом</param>
public abstract class FunctionForDynamicOpen(string namespaceProgram, string namespaceCodeGeneration, NotebookFunction? notebookFunc)
{
    string NamespaceProgram { get; set; } = namespaceProgram;
    string NamespaceCodeGeneration { get; set; } = namespaceCodeGeneration;
    NotebookFunction? NotebookFunc { get; set; } = notebookFunc;
    Assembly ExecutingAssembly { get; } = Assembly.GetCallingAssembly();

    /// <summary>
    /// Функція відкриває журнал
    /// </summary>
    /// <param name="typeJournal">Тип</param>
    /// <param name="uniqueID">Елемент для позиціювання</param>
    public bool OpenJournalByType(string typeJournal, UniqueID? uniqueID)
    {
        object? journalInstance;

        try
        {
            journalInstance = ExecutingAssembly.CreateInstance($"{NamespaceProgram}.Журнал_{typeJournal}");
        }
        catch (Exception ex)
        {
            Message.Error(NotebookFunc?.BasicForm?.Application, NotebookFunc?.BasicForm, ex.Message);
            return false;
        }

        if (journalInstance != null)
        {
            dynamic journal = journalInstance;

            //Документ який потрібно виділити в списку
            journal.SelectPointerItem = uniqueID;

            NotebookFunc?.CreatePage(typeJournal, journal);
            journal.SetValue();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Функція відкриває список довідника або сам елемент
    /// </summary>
    /// <param name="typeDir">Тип</param>
    /// <param name="uniqueID">Елемент для позиціонування</param>
    /// <param name="typeForm">Тип форми</param>
    public bool OpenDirectoryByType(string typeDir, UniqueID? uniqueID, TypeForm typeForm = TypeForm.Journal)
    {
        switch (typeForm)
        {
            case TypeForm.Journal:
                {
                    object? directoryInstance;

                    try
                    {
                        directoryInstance = ExecutingAssembly.CreateInstance($"{NamespaceProgram}.{typeDir}_Список");
                    }
                    catch (Exception ex)
                    {
                        Message.Error(NotebookFunc?.BasicForm?.Application, NotebookFunc?.BasicForm, ex.Message);
                        return false;
                    }

                    if (directoryInstance != null)
                    {
                        dynamic directory = directoryInstance;

                        //Елемент який потрібно виділити в списку
                        directory.SelectPointerItem = uniqueID;

                        //Заголовок журналу
                        string listName = "Список";

                        Type? directoryConst = ExecutingAssembly.GetType($"{NamespaceProgram}.Довідники.{typeDir}_Const");
                        if (directoryConst != null)
                            listName = directoryConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;

                        NotebookFunc?.CreatePage(listName, directory);
                        directory.SetValue();

                        return true;
                    }
                    else
                        return false;
                }
            case TypeForm.Element:
                {
                    Type? directoryFunction = ExecutingAssembly.GetType($"{NamespaceProgram}.{typeDir}_Функції");
                    directoryFunction?.GetMethod("OpenPageElement", BindingFlags.Public | BindingFlags.Static)?.Invoke(directoryFunction, [false, uniqueID, null, null]);

                    return true;
                }
        }

        return false;
    }

    /// <summary>
    /// Функція відкриває список докуменів або сам документ
    /// </summary>
    /// <param name="typeDoc">Тип документу</param>
    /// <param name="uniqueID">Елемент для позиціювання</param>
    /// <param name="keyForSetting">Додатковий ключ для налаштуваннь користувача</param>
    /// <param name="typeForm">Тип форми</param>
    public bool OpenDocumentByType(string typeDoc, UniqueID? uniqueID, string keyForSetting = "", TypeForm typeForm = TypeForm.Journal)
    {
        switch (typeForm)
        {
            case TypeForm.Journal:
                {
                    object? documentInstance;

                    try
                    {
                        documentInstance = ExecutingAssembly.CreateInstance($"{NamespaceProgram}.{typeDoc}_Список");
                    }
                    catch (Exception ex)
                    {
                        Message.Error(NotebookFunc?.BasicForm?.Application, NotebookFunc?.BasicForm, ex.Message);
                        return false;
                    }

                    if (documentInstance != null)
                    {
                        dynamic document = documentInstance;

                        //Елемент який потрібно виділити в списку
                        document.SelectPointerItem = uniqueID;

                        //Заголовок журналу
                        string listName = "Список";
                        Type? documentConst = ExecutingAssembly.GetType($"{NamespaceCodeGeneration}.Документи.{typeDoc}_Const");
                        if (documentConst != null)
                            listName = documentConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;

                        //Додатковий ключ для налаштувань
                        if (!string.IsNullOrEmpty(keyForSetting))
                            document.KeyForSetting = keyForSetting;

                        NotebookFunc?.CreatePage(listName, document);
                        document.SetValue();

                        return true;
                    }
                    else
                        return false;
                }
            case TypeForm.Element:
                {
                    Type? documentFunction = ExecutingAssembly.GetType($"{NamespaceProgram}.{typeDoc}_Функції");
                    documentFunction?.GetMethod("OpenPageElement", BindingFlags.Public | BindingFlags.Static)?.Invoke(documentFunction, [false, uniqueID, null]);

                    return true;
                }
        }

        return false;
    }

    /// <summary>
    /// Функція відкриває регістр відомостей
    /// </summary>
    /// <param name="typeReg">Назва</param>
    /// <param name="uniqueID">Елемент який потрібно виділити в списку</param>
    public bool OpenRegisterInformationByType(string typeReg, UniqueID? uniqueID)
    {
        object? registerInstance;

        try
        {
            registerInstance = ExecutingAssembly.CreateInstance($"{NamespaceProgram}.РегістриВідомостей.{typeReg}_Список");
        }
        catch (Exception ex)
        {
            Message.Error(NotebookFunc?.BasicForm?.Application, NotebookFunc?.BasicForm, ex.Message);
            return false;
        }

        if (registerInstance != null)
        {
            dynamic register = registerInstance;

            //Елемент який потрібно виділити в списку
            register.SelectPointerItem = uniqueID;

            //Заголовок
            string listName = "Список";

            Type? documentConst = ExecutingAssembly.GetType($"{NamespaceCodeGeneration}.РегістриВідомостей.{typeReg}_Const");
            if (documentConst != null)
                listName = documentConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;

            NotebookFunc?.CreatePage(listName, () => register);
            register.SetValue();

            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Функція відкриває регістр накопичення
    /// </summary>
    /// <param name="typeReg">Назва</param>
    /// <param name="uniqueID">Елемент який потрібно виділити в списку</param>
    public bool OpenRegisterAccumulationByType(string typeReg, UniqueID? uniqueID)
    {
        object? registerInstance;

        try
        {
            registerInstance = ExecutingAssembly.CreateInstance($"{NamespaceProgram}.РегістриНакопичення.{typeReg}_Список");
        }
        catch (Exception ex)
        {
            Message.Error(NotebookFunc?.BasicForm?.Application, NotebookFunc?.BasicForm, ex.Message);
            return false;
        }

        if (registerInstance != null)
        {
            dynamic register = registerInstance;

            //Елемент який потрібно виділити в списку
            register.SelectPointerItem = uniqueID;

            //Заголовок
            string listName = "Список";

            Type? documentConst = ExecutingAssembly.GetType($"{NamespaceCodeGeneration}.РегістриНакопичення.{typeReg}_Const");
            if (documentConst != null)
                listName = documentConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;

            NotebookFunc?.CreatePage(listName, () => register);
            register.SetValue();

            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Список документів для журналу у спливаючому вікні
    /// </summary>
    /// <param name="parent">Прив'язка Popover</param>
    /// <param name="allowDocument">Колекція</param>
    public void OpenDocumentListForJournal(Widget parent, Dictionary<string, string> allowDocument)
    {
        Box vBox = Box.New(Orientation.Vertical, 0);

        foreach (KeyValuePair<string, string> typeDoc in allowDocument)
        {
            LinkButton lb = LinkButton.New(typeDoc.Value);
            lb.Halign = Align.Start;
            lb.OnActivateLink += (sender, args) =>
            {
                OpenDocumentByType(typeDoc.Key, new UniqueID());
                return true;
            };

            vBox.Append(lb);
        }

        Popover popover = Popover.New();
        popover.SetParent(parent);
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