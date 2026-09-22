
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

namespace InterfaceGtk4;

/// <summary>
/// 
/// </summary>
public class ConfiguratorItemOwner
{
    public ConfiguratorItemOwner(ConfiguratorItemOwnerType group, object? obj = null)
    {
        Group = group;
        Obj = obj;
    }

    /*
    public ConfiguratorItemOwner(ConfiguratorItemOwnerType group, string name, object? obj = null)
    {
        Group = group;
        Name = name;
        Obj = obj;
    }
    */

    /// <summary>
    /// Група до якої належить власник
    /// </summary>
    public ConfiguratorItemOwnerType Group { get; set; }

    /// <summary>
    /// Назва власника
    /// </summary>
    //public string Name { get; set; } = "";

    /// <summary>
    /// Об'єкт власник
    /// </summary>
    public object? Obj { get; set; }
}

public enum ConfiguratorItemOwnerType
{
    Constant,
    Directory,
    Document,
    TablePart,
    RegisterAccumulation,
    RegisterInformation
}