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

namespace AccountingSoftware
{
    /// <summary>
    /// Ядро
    /// </summary>
    public class Kernel
    {
        /// <summary>
        /// Підключення до сервера баз даних і завантаження конфігурації
        /// </summary>
        /// <param name="PathToXmlFileConfiguration">Шлях до файлу конфігурації</param>
        /// <param name="Server">Адреса сервера баз даних</param>
        /// <param name="UserId">Користувач</param>
        /// <param name="Password">Пароль</param>
        /// <param name="Port">Порт</param>
        /// <param name="Database">База даних</param>
        /// <param name="variantLoadConf">Варіант завантаження конфігурації</param>
        /// <returns>True якщо підключення відбулось нормально</returns>
        public async ValueTask<bool> Open(string PathToXmlFileConfiguration, string Server, string UserId, string Password, int Port, string Database,
            Configuration.VariantLoadConf variantLoadConf = Configuration.VariantLoadConf.Full)
        {
            bool result = await OpenOnlyDataBase(Server, UserId, Password, Port, Database);

            try
            {
                Configuration.Load(PathToXmlFileConfiguration, out Configuration conf, variantLoadConf);
                Conf = conf;
            }
            catch
            {
                return false;
            }

            Conf.PathToXmlFileConfiguration = PathToXmlFileConfiguration;

            //Додаткова перевірка наявності словника в базі даних
            if (result && Conf.DictTSearch != Configuration.DefaultDictTSearch && !await DataBase.SpetialTableFullTextSearchIfExistDict(Conf.DictTSearch))
                Conf.DictTSearch = Configuration.DefaultDictTSearch;

            return result;
        }

        /// <summary>
        /// Підключення до сервера баз даних без конфігурації
        /// </summary>
        /// <param name="Server">Адреса сервера баз даних</param>
        /// <param name="UserId">Користувач</param>
        /// <param name="Password">Пароль</param>
        /// <param name="Port">Порт</param>
        /// <param name="Database">База даних</param>
        /// <param name="exception"Помилка></param>
        /// <returns>True якщо підключення відбулось нормально</returns>
        public async ValueTask<bool> OpenOnlyDataBase(string Server, string UserId, string Password, int Port, string Database)
        {
            DataBase = new PostgreSQL();
            bool result = await DataBase.Open(Server, UserId, Password, Port, Database);
            Exception = result ? null : DataBase.Exception;

            DataBase_Server = Server;
            DataBase_UserId = UserId;
            DataBase_Port = Port.ToString();
            DataBase_BaseName = Database;

            return result;
        }

        /// <summary>
        /// Перевірити підключення до сервера
        /// </summary>
        /// <param name="Server">Адреса сервера баз даних</param>
        /// <param name="UserId">Користувач</param>
        /// <param name="Password">Пароль</param>
        /// <param name="Port">Порт</param>
        /// <param name="Database">База даних</param>
        /// <returns>True якщо все ок</returns>
        public async ValueTask<bool> TryConnectToServer(string Server, string UserId, string Password, int Port, string Database)
        {
            DataBase = new PostgreSQL();
            bool result = await DataBase.TryConnectToServer(Server, UserId, Password, Port, Database);
            Exception = result ? null : DataBase.Exception;

            return result;
        }

        /// <summary>
        /// Чи вже є база даних?
        /// </summary>
        /// <param name="Server">Адреса сервера баз даних</param>
        /// <param name="UserId">Користувач</param>
        /// <param name="Password">Пароль</param>
        /// <param name="Port">Порт</param>
        /// <param name="Database">База даних</param>
        /// <returns>True якщо все ок</returns>
        public async ValueTask<bool> IfExistDatabase(string Server, string UserId, string Password, int Port, string Database)
        {
            DataBase = new PostgreSQL();
            bool result = await DataBase.IfExistDatabase(Server, UserId, Password, Port, Database);
            Exception = result ? null : DataBase.Exception;

            return result;
        }

        /// <summary>
        /// Створити базу даних
        /// </summary>
        /// <param name="Server">Адреса сервера баз даних</param>
        /// <param name="UserId">Користувач</param>
        /// <param name="Password">Пароль</param>
        /// <param name="Port">Порт</param>
        /// <param name="Database">База даних</param>
        /// <returns>True якщо все ок</returns>
        public async ValueTask<bool> CreateDatabaseIfNotExist(string Server, string UserId, string Password, int Port, string Database)
        {
            DataBase = new PostgreSQL();
            bool result = await DataBase.CreateDatabaseIfNotExist(Server, UserId, Password, Port, Database);
            Exception = result ? null : DataBase.Exception;

            return result;
        }

        /// <summary>
        /// Закриття
        /// </summary>
        public void Close()
        {
            DataBase.Close();
            Conf = new Configuration();
            User = Session = Guid.Empty;
        }

        /// <summary>
        /// Конфігурація
        /// </summary>
        public Configuration Conf { get; set; } = new Configuration();

        /// <summary>
        /// Інтерфейс для роботи з базою даних
        /// </summary>
        public IDataBase DataBase { get; set; } = new PostgreSQL();

        /// <summary>
        /// Авторизований користувач
        /// </summary>
        public Guid User { get; private set; } = Guid.Empty;

        /// <summary>
        /// Сесія користувача
        /// </summary>
        public Guid Session { get; private set; } = Guid.Empty;

        /// <summary>
        /// Авторизація 
        /// </summary>
        /// <param name="user">Користувач</param>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        public async Task<bool> UserLogIn(string user, string password, TypeForm typeForm)
        {
            (Guid User, Guid Session)? userSession = await DataBase.SpetialTableUsersLogIn(user, password, typeForm);
            if (userSession != null)
            {
                User = userSession.Value.User;
                Session = userSession.Value.Session;
                return true;
            }
            else
            {
                User = Session = Guid.Empty;
                return false;
            }
        }

        /// <summary>
        /// Дата останньої вибірки тригерів оновлення обєктів
        /// </summary>
        DateTime AfterUpdateSession { get; set; } = DateTime.Now;

        /// <summary>
        /// Чи вже запущене автоматичне оновлення сесії?
        /// </summary>
        bool IsRunLoopUpdateSession = false;

        /// <summary>
        /// Фонове обновлення сесії
        /// </summary>
        public async void LoopUpdateSession()
        {
            if (IsRunLoopUpdateSession)
                return;

            IsRunLoopUpdateSession = true;

            //Очищення устарівших тригерів оновлення об’єктів
            await DataBase.SpetialTableObjectUpdateTrigerClearOld();
            AfterUpdateSession = await DataBase.SelectCurrentTimestamp();

            while (true)
            {
                await DataBase.SpetialTableActiveUsersUpdateSession(Session);
                UpdateSession?.Invoke(this, new());

                //Тригери оновлення обєктів
                if (DirectoryObjectChanged != null || DocumentObjectChanged != null)
                {
                    SelectRequest_Record resultRecords = await DataBase.SpetialTableObjectUpdateTrigerSelect(AfterUpdateSession);
                    if (resultRecords.Result)
                    {
                        AfterUpdateSession = (DateTime)resultRecords.ListRow[0]["after"];

                        Dictionary<string, List<ObjectChanged>> directory = [];
                        Dictionary<string, List<ObjectChanged>> document = [];

                        foreach (var record in resultRecords.ListRow)
                        {
                            UuidAndText obj = (UuidAndText)record["obj"];
                            TypeObjectChanged operation = (char)record["operation"] switch { 'A' => TypeObjectChanged.Add, 'U' => TypeObjectChanged.Update, 'D' => TypeObjectChanged.Delete, _ => TypeObjectChanged.Update };
                            var (_, pointerGroup, pointerType) = Configuration.PointerParse(obj.Text, out Exception? _);

                            if (pointerGroup == "Довідники")
                            {
                                if (directory.TryGetValue(pointerType, out List<ObjectChanged>? list))
                                    list.Add(new(obj.Uuid, operation));
                                else
                                    directory.Add(pointerType, [new(obj.Uuid, operation)]);
                            }
                            else if (pointerGroup == "Документи")
                            {
                                if (document.TryGetValue(pointerType, out List<ObjectChanged>? list))
                                    list.Add(new(obj.Uuid, operation));
                                else
                                    document.Add(pointerType, [new(obj.Uuid, operation)]);
                            }
                        }

                        if (DirectoryObjectChanged != null && directory.Count > 0)
                            DirectoryObjectChanged.Invoke(this, directory);

                        if (DocumentObjectChanged != null && document.Count > 0)
                            DocumentObjectChanged.Invoke(this, document);
                    }
                }

                //Затримка на 5 сек
                await Task.Delay(5000);
            }
        }

        /// <summary>
        /// Псевдонім типу відкритої форми
        /// </summary>
        /// <param name="typeForm">Тип форми</param>
        /// <returns>Псевдонім</returns>
        public static string TypeForm_Alias(TypeForm typeForm)
        {
            return typeForm switch
            {
                TypeForm.Configurator => "Конфігуратор",
                TypeForm.WorkingProgram => "Робоча програма",
                TypeForm.WorkingWeb => "Сайт",
                TypeForm.WorkingBot => "Бот",
                _ => ""
            };
        }

        #region DataBase Info

        public string DataBase_Server { get; private set; } = "";
        public string DataBase_UserId { get; private set; } = "";
        public string DataBase_Port { get; private set; } = "";
        public string DataBase_BaseName { get; private set; } = "";

        #endregion

        #region Exception

        /// <summary>
        /// Інформація про помилки
        /// </summary>
        public Exception? Exception { get; private set; }

        #endregion

        #region Messages

        /* Запис і зчитування повідомлень про помилки та інформаційних повідомлень */

        public async ValueTask MessageInfoAdd(string nameProcess, Guid? objectUid, string? typeObject, string nameObject, string message)
        {
            await DataBase.SpetialTableMessageErrorAdd
            (
                User,
                nameProcess,
                objectUid != null ? (Guid)objectUid : Guid.Empty,
                typeObject ?? "",
                nameObject,
                message,
                'I'
            );

            await ClearOutdatedMessages();
        }

        public async ValueTask MessageErrorAdd(string nameProcess, Guid? objectUid, string? typeObject, string nameObject, string message)
        {
            await DataBase.SpetialTableMessageErrorAdd
            (
                User,
                nameProcess,
                objectUid != null ? (Guid)objectUid : Guid.Empty,
                typeObject ?? "",
                nameObject,
                message,
                'E'
            );

            await ClearOutdatedMessages();
        }

        public async ValueTask ClearAllMessages()
        {
            await DataBase.SpetialTableMessageErrorClear(User);
        }

        public async ValueTask ClearOutdatedMessages()
        {
            await DataBase.SpetialTableMessageErrorClearOld(User);
        }

        public async ValueTask<SelectRequest_Record> SelectMessages(UnigueID? objectUnigueID = null, int? limit = null)
        {
            return await DataBase.SpetialTableMessageErrorSelect(User, objectUnigueID, limit);
        }

        #endregion

        #region Events

        /// <summary>
        /// Після оновлення сесії
        /// </summary>
        public event EventHandler? UpdateSession;

        /// <summary>
        /// Зміни в довідниках
        /// </summary>
        public event EventHandler<Dictionary<string, List<ObjectChanged>>>? DirectoryObjectChanged;

        /// <summary>
        /// Зміни в документах
        /// </summary>
        public event EventHandler<Dictionary<string, List<ObjectChanged>>>? DocumentObjectChanged;

        #endregion
    }

    /// <summary>
    /// Варіант запуску програми
    /// </summary>
    public enum TypeForm
    {
        /// <summary>
        /// Конфігуратор
        /// </summary>
        Configurator,

        /// <summary>
        /// Програма
        /// </summary>
        WorkingProgram,

        /// <summary>
        /// Web
        /// </summary>
        WorkingWeb,

        /// <summary>
        /// Bot
        /// </summary>
        WorkingBot,
    }

    /// <summary>
    /// Тип оновлення об'єктів
    /// </summary>
    public enum TypeObjectChanged
    {
        Add,
        Update,
        Delete
    }

    public record ObjectChanged(Guid Uid, TypeObjectChanged Type);
}