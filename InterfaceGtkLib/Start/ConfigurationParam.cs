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

using System.Xml;
using System.Xml.XPath;

namespace InterfaceGtkLib;

public class ConfigurationParamCollection
{
    public static List<ConfigurationParam> ListConfigurationParam { get; set; } = [];
    public static string PathToXML { get; set; } = "";

    public static void LoadConfigurationParamFromXML(string pathToXML)
    {
        ListConfigurationParam.Clear();

        if (File.Exists(pathToXML))
        {
            XPathDocument xPathDoc = new XPathDocument(pathToXML);
            XPathNavigator xPathDocNavigator = xPathDoc.CreateNavigator();

            XPathNodeIterator? ConfigurationParamNodes = xPathDocNavigator.Select("/root/Configuration");
            while (ConfigurationParamNodes!.MoveNext())
            {
                ConfigurationParam ItemConfigurationParam = new ConfigurationParam();

                XPathNavigator? currentNode = ConfigurationParamNodes.Current;

                string SelectAttribute = currentNode?.GetAttribute("Select", "") ?? "";
                if (!string.IsNullOrEmpty(SelectAttribute))
                    ItemConfigurationParam.Select = bool.Parse(SelectAttribute);

                ItemConfigurationParam.ConfigurationKey = currentNode?.SelectSingleNode("Key")?.Value ?? "";
                ItemConfigurationParam.ConfigurationName = currentNode?.SelectSingleNode("Name")?.Value ?? "";
                ItemConfigurationParam.DataBaseServer = currentNode?.SelectSingleNode("Server")?.Value ?? "";
                ItemConfigurationParam.DataBasePort = int.Parse(currentNode?.SelectSingleNode("Port")?.Value ?? "0");
                ItemConfigurationParam.DataBaseLogin = currentNode?.SelectSingleNode("Login")?.Value ?? "";
                ItemConfigurationParam.DataBasePassword = currentNode?.SelectSingleNode("Password")?.Value ?? "";
                ItemConfigurationParam.DataBaseBaseName = currentNode?.SelectSingleNode("BaseName")?.Value ?? "";

                XPathNodeIterator? otherItemParamNodeList = currentNode?.Select("OtherParam/Param");

                if (otherItemParamNodeList != null)
                    while (otherItemParamNodeList.MoveNext())
                    {
                        XPathNavigator? currentItemParamNode = otherItemParamNodeList.Current;
                        string paramName = currentItemParamNode?.GetAttribute("name", "") ?? "";
                        string paramValue = currentItemParamNode?.Value ?? "";

                        if (!string.IsNullOrEmpty(paramName))
                            ItemConfigurationParam.OtherParam.Add(paramName, paramValue);
                    }

                ListConfigurationParam.Add(ItemConfigurationParam);
            }
        }
    }

    public static void SaveConfigurationParamFromXML(string pathToXML)
    {
        XmlDocument xmlConfParamDocument = new XmlDocument();
        xmlConfParamDocument.AppendChild(xmlConfParamDocument.CreateXmlDeclaration("1.0", "utf-8", ""));

        XmlElement rootNode = xmlConfParamDocument.CreateElement("root");
        xmlConfParamDocument.AppendChild(rootNode);

        foreach (ConfigurationParam ItemConfigurationParam in ListConfigurationParam)
        {
            XmlElement configurationNode = xmlConfParamDocument.CreateElement("Configuration");
            rootNode.AppendChild(configurationNode);

            XmlAttribute selectAttribute = xmlConfParamDocument.CreateAttribute("Select");
            selectAttribute.Value = ItemConfigurationParam.Select.ToString();
            configurationNode.Attributes.Append(selectAttribute);

            XmlElement nodeKey = xmlConfParamDocument.CreateElement("Key");
            nodeKey.InnerText = ItemConfigurationParam.ConfigurationKey;
            configurationNode.AppendChild(nodeKey);

            XmlElement nodeName = xmlConfParamDocument.CreateElement("Name");
            nodeName.InnerText = ItemConfigurationParam.ConfigurationName;
            configurationNode.AppendChild(nodeName);

            XmlElement nodeServer = xmlConfParamDocument.CreateElement("Server");
            nodeServer.InnerText = ItemConfigurationParam.DataBaseServer;
            configurationNode.AppendChild(nodeServer);

            XmlElement nodePort = xmlConfParamDocument.CreateElement("Port");
            nodePort.InnerText = ItemConfigurationParam.DataBasePort.ToString();
            configurationNode.AppendChild(nodePort);

            XmlElement nodeLogin = xmlConfParamDocument.CreateElement("Login");
            nodeLogin.InnerText = ItemConfigurationParam.DataBaseLogin;
            configurationNode.AppendChild(nodeLogin);

            XmlElement nodePassword = xmlConfParamDocument.CreateElement("Password");
            nodePassword.InnerText = ItemConfigurationParam.DataBasePassword;
            configurationNode.AppendChild(nodePassword);

            XmlElement nodeBaseName = xmlConfParamDocument.CreateElement("BaseName");
            nodeBaseName.InnerText = ItemConfigurationParam.DataBaseBaseName;
            configurationNode.AppendChild(nodeBaseName);

            XmlElement nodeOtherParam = xmlConfParamDocument.CreateElement("OtherParam");
            configurationNode.AppendChild(nodeOtherParam);

            foreach (KeyValuePair<string, string> itemParam in ItemConfigurationParam.OtherParam)
            {
                XmlElement nodeItemParam = xmlConfParamDocument.CreateElement("Param");
                nodeItemParam.SetAttribute("name", itemParam.Key);
                nodeItemParam.InnerText = itemParam.Value;
                nodeOtherParam.AppendChild(nodeItemParam);
            }
        }

        xmlConfParamDocument.Save(pathToXML);
    }

    public static ConfigurationParam? GetConfigurationParam(string key)
    {
        return ListConfigurationParam.Find(x => x.ConfigurationKey == key);
    }

    public static bool RemoveConfigurationParam(string key)
    {
        ConfigurationParam? selectConfigurationParam = GetConfigurationParam(key);
        return selectConfigurationParam != null && ListConfigurationParam.Remove(selectConfigurationParam);
    }

    public static void SelectConfigurationParam(string key)
    {
        foreach (ConfigurationParam itemConfigurationParam in ListConfigurationParam)
            itemConfigurationParam.Select = itemConfigurationParam.ConfigurationKey == key;
    }

    public static void UpdateConfigurationParam(ConfigurationParam configurationParam)
    {
        ConfigurationParam? itemConfigurationParam = GetConfigurationParam(configurationParam.ConfigurationKey);
        if (itemConfigurationParam != null)
        {
            itemConfigurationParam.ConfigurationName = configurationParam.ConfigurationName;
            itemConfigurationParam.DataBaseServer = configurationParam.DataBaseServer;
            itemConfigurationParam.DataBaseLogin = configurationParam.DataBaseLogin;
            itemConfigurationParam.DataBasePassword = configurationParam.DataBasePassword;
            itemConfigurationParam.DataBasePort = configurationParam.DataBasePort;
            itemConfigurationParam.DataBaseBaseName = configurationParam.DataBaseBaseName;
        }
    }
}

public class ConfigurationParam
{
    public string ConfigurationKey { get; set; } = "";
    public string ConfigurationName { get; set; } = "";
    public string DataBaseServer { get; set; } = "localhost";
    public int DataBasePort { get; set; } = 5432;
    public string DataBaseLogin { get; set; } = "postgres";
    public string DataBasePassword { get; set; } = "";
    public string DataBaseBaseName { get; set; } = "";
    public bool Select { get; set; } = false;
    public Dictionary<string, string> OtherParam { get; set; } = [];

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(ConfigurationName) ? "[]" : ConfigurationName;
    }

    public static ConfigurationParam New()
    {
        return new ConfigurationParam() { ConfigurationKey = Guid.NewGuid().ToString(), ConfigurationName = "* Новий" };
    }

    public ConfigurationParam Clone()
    {
        ConfigurationParam configurationParam = new ConfigurationParam
        {
            ConfigurationKey = Guid.NewGuid().ToString(),
            ConfigurationName = ConfigurationName + " - Копія",
            DataBaseServer = DataBaseServer,
            DataBaseLogin = DataBaseLogin,
            DataBasePassword = DataBasePassword,
            DataBaseBaseName = DataBaseBaseName,
            DataBasePort = DataBasePort,
            OtherParam = OtherParam
        };

        return configurationParam;
    }
}