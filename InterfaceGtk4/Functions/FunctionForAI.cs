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

using Google.GenAI;
using Microsoft.Extensions.AI;

namespace InterfaceGtk4;

/// <summary>
/// AI
/// </summary>
public static class FunctionForAI
{
    public static IChatClient? Client { get; private set; } = null;

    public static void CreateClient(string apiKey, string modelId)
    {
        Client?.Dispose();

        Client = new Client(apiKey: apiKey)
            .AsIChatClient(modelId)
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();
    }
}