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



*/

using Gtk;

namespace InterfaceGtk4;

/// <summary>
/// Функції для стандартних інтерфейсів довідників або документів
/// </summary>
public static class FunctionForInterfaces
{
    static Builder GetBuilder(Group group, string file) =>
        Builder.NewFromFile(Path.Combine(AppContext.BaseDirectory, $"Interfaces{Path.DirectorySeparatorChar}{group}", file));

    enum Group
    {
        Document,
        Directory
    }

    #region Document

    /// <summary>
    /// Для документу базовий
    /// </summary>
    /// <returns>record DocumentElement</returns>
    public static DocumentElement ForDocument()
    {
        Builder builder = GetBuilder(Group.Document, "DocumentBase.xml");

        Box MainBox = builder.GetObject("MainBox") as Box ?? throw new Exception();
        Box TopBox = builder.GetObject("TopBox") as Box ?? throw new Exception();
        Box TopStartBox = builder.GetObject("TopStartBox") as Box ?? throw new Exception();
        Box TopEndBox = builder.GetObject("TopEndBox") as Box ?? throw new Exception();
        Box BottomStartBox = builder.GetObject("BottomStartBox") as Box ?? throw new Exception();
        Box BottomEndBox = builder.GetObject("BottomEndBox") as Box ?? throw new Exception();
        Box CommentBox = builder.GetObject("CommentBox") as Box ?? throw new Exception();
        Notebook Notebook = builder.GetObject("Notebook") as Notebook ?? throw new Exception();

        return new()
        {
            MainBox = MainBox,
            TopBox = TopBox,
            TopStartBox = TopStartBox,
            TopEndBox = TopEndBox,
            BottomStartBox = BottomStartBox,
            BottomEndBox = BottomEndBox,
            CommentBox = CommentBox,
            Notebook = Notebook
        };
    }

    /// <summary>
    /// Для документу мінімальний
    /// </summary>
    /// <returns>record DocumentElementSmall</returns>
    public static DocumentElementSmall ForDocumentSmall()
    {
        Builder builder = GetBuilder(Group.Document, "DocumentSmall.xml");

        Box MainBox = builder.GetObject("MainBox") as Box ?? throw new Exception();
        Box TopBox = builder.GetObject("TopBox") as Box ?? throw new Exception();
        Box TopStartBox = builder.GetObject("TopStartBox") as Box ?? throw new Exception();
        Box TopEndBox = builder.GetObject("TopEndBox") as Box ?? throw new Exception();
        Box CommentBox = builder.GetObject("CommentBox") as Box ?? throw new Exception();

        return new()
        {
            MainBox = MainBox,
            TopBox = TopBox,
            TopStartBox = TopStartBox,
            TopEndBox = TopEndBox,
            CommentBox = CommentBox,
        };
    }

    public record DocumentElement()
    {
        public required Box MainBox;
        public required Box TopBox;
        public required Box TopStartBox;
        public required Box TopEndBox;
        public required Box BottomStartBox;
        public required Box BottomEndBox;
        public required Box CommentBox;
        public required Notebook Notebook;
    }

    public record DocumentElementSmall()
    {
        public required Box MainBox;
        public required Box TopBox;
        public required Box TopStartBox;
        public required Box TopEndBox;
        public required Box CommentBox;
    }

    #endregion

    #region Directory

    /// <summary>
    /// Для довідника базовий
    /// </summary>
    /// <returns>record DirectoryElement</returns>
    public static DirectoryElement ForDirectory()
    {
        Builder builder = GetBuilder(Group.Directory, "DirectoryBase.xml");

        Box MainBox = builder.GetObject("MainBox") as Box ?? throw new Exception();
        Paned Paned = builder.GetObject("Paned") as Paned ?? throw new Exception();
        Box TopStartBox = builder.GetObject("TopStartBox") as Box ?? throw new Exception();
        Box TopEndBox = builder.GetObject("TopEndBox") as Box ?? throw new Exception();

        return new(MainBox, Paned, TopStartBox, TopEndBox);
    }

    /// <summary>
    /// Для довідника мінімальний
    /// </summary>
    /// <returns>record DirectoryElementSmall</returns>
    public static DirectoryElementSmall ForDirectorySmall()
    {
        Builder builder = GetBuilder(Group.Directory, "DirectorySmall.xml");

        Box MainBox = builder.GetObject("MainBox") as Box ?? throw new Exception();
        Box TopBox = builder.GetObject("TopBox") as Box ?? throw new Exception();
        Box TopStartBox = builder.GetObject("TopStartBox") as Box ?? throw new Exception();

        return new(MainBox, TopBox, TopStartBox);
    }

    /// <summary>
    /// Для довідника з двома блоками
    /// </summary>
    /// <returns>record DirectoryElementTwoBoxes</returns>
    public static DirectoryElementTwoBoxes ForDirectoryTwoBoxes()
    {
        Builder builder = GetBuilder(Group.Directory, "DirectoryTwoBoxes.xml");

        Box MainBox = builder.GetObject("MainBox") as Box ?? throw new Exception();
        Box TopBox = builder.GetObject("TopBox") as Box ?? throw new Exception();
        Box TopStartBox = builder.GetObject("TopStartBox") as Box ?? throw new Exception();
        Box TopEndBox = builder.GetObject("TopEndBox") as Box ?? throw new Exception();

        return new(MainBox, TopBox, TopStartBox, TopEndBox);
    }

    public record DirectoryElement(Box MainBox, Paned Paned, Box TopStartBox, Box TopEndBox);
    public record DirectoryElementSmall(Box MainBox, Box TopBox, Box TopStartBox);
    public record DirectoryElementTwoBoxes(Box MainBox, Box TopBox, Box TopStartBox, Box TopEndBox);

    #endregion
}