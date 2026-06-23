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
/// 
/// </summary>
public static class FunctionForInterfaces
{
    static Builder GetBuilder(string file) =>
        Builder.NewFromFile(Path.Combine(AppContext.BaseDirectory, "Interfaces/Directory/", file));

    public class FunctionForInterfacesException(string name) : Exception($"Не знайдено об'єкт {name} при побудові інтерфейсу")  {}

    #region Document

    public static DocumentElement ForDocument()
    {
        Builder builder = GetBuilder("DocumentBase.xml");

        Box MainBox = builder.GetObject("MainBox") as Box ?? throw new FunctionForInterfacesException("MainBox");
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

    public static DocumentElementSmall ForDocumentSmall()
    {
        Builder builder = GetBuilder("DocumentSmall.xml");

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

    public static DirectoryElement ForDirectory()
    {
        Builder builder = GetBuilder("DirectoryBase.xml");

        Box MainBox = builder.GetObject("MainBox") as Box ?? throw new Exception();
        Paned Paned = builder.GetObject("Paned") as Paned ?? throw new Exception();
        Box TopStartBox = builder.GetObject("TopStartBox") as Box ?? throw new Exception();
        Box TopEndBox = builder.GetObject("TopEndBox") as Box ?? throw new Exception();

        return new(MainBox, Paned, TopStartBox, TopEndBox);
    }

    public static DirectoryElementSmall ForDirectorySmall()
    {
        Builder builder = GetBuilder("DirectorySmall.xml");

        Box MainBox = builder.GetObject("MainBox") as Box ?? throw new Exception();
        Box TopBox = builder.GetObject("TopBox") as Box ?? throw new Exception();
        Box TopStartBox = builder.GetObject("TopStartBox") as Box ?? throw new Exception();

        return new(MainBox, TopBox, TopStartBox);
    }

    public static DirectoryElementTwoBoxes ForDirectoryTwoBoxes()
    {
        Builder builder = GetBuilder("DirectoryTwoBoxes.xml");

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