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

using GObject;
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Ієрархічний рядок
/// </summary>
[Subclass<GObject.Object>]
public partial class DirectoryHierarchicalRow : IRowSubclassJournal
{
    public static DirectoryHierarchicalRow New() => NewWithProperties([]);

    #region Довантаження

    /// <summary>
    /// Цей елемент для завантаження даних
    /// </summary>
    public bool IsLoading { get; set; }

    /// <summary>
    /// Сховище
    /// </summary>
    public Gio.ListStore? Store { get; set; }

    /// <summary>
    /// Доступний тип контенту довідника
    /// </summary>
    public ConfigurationDirectories.HierarchicalContentType AllowedContent
    {
        get => allowedContent_;
        set
        {
            allowedContent_ = value;

            //Базове значення поля IsFolder в залежності від типу контенту
            //Для контенту FoldersAndElements значення задається в згенерованому модулі
            if (allowedContent_ == ConfigurationDirectories.HierarchicalContentType.Folders)
                IsFolder = true;
            else if (allowedContent_ == ConfigurationDirectories.HierarchicalContentType.Elements)
                IsFolder = false;
        }
    }
    ConfigurationDirectories.HierarchicalContentType allowedContent_ = ConfigurationDirectories.HierarchicalContentType.Folders;

    #endregion

    /// <summary>
    /// Унікальний ідентифікатор
    /// </summary>
    public UniqueID UniqueID { get; set; } = UniqueID.NewEmpty();

    /// <summary>
    /// Помітка на видалення
    /// </summary>
    public bool DeletionLabel { get; set; } = false;

    /// <summary>
    /// Це папка
    /// </summary>
    public bool IsFolder { get; set; }

    /// <summary>
    /// Колекція полів
    /// </summary>
    public Dictionary<string, string?> Fields { get; set; } = [];
}
