/*
Copyright (C) 2019-2020 TARAKHOMYN YURIY IVANOVYCH
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

using System;

namespace AccountingSoftware
{
	/// <summary>
	/// Ядро
	/// </summary>
	public class Kernel
	{
		public Kernel() { /**/ }

		/// <summary>
		/// Перевірити підключення до сервера
		/// </summary>
		/// <param name="Server">Адреса сервера баз даних</param>
		/// <param name="UserId">Користувач</param>
		/// <param name="Password">Пароль</param>
		/// <param name="Port">Порт</param>
		/// <param name="Database">База даних</param>
		/// <param name="exception">Помилка</param>
		/// <returns></returns>
		public bool TryConnectToServer(string Server, string UserId, string Password, int Port, string Database, out Exception exception)
		{
			DataBase = new PostgreSQL();
			return DataBase.TryConnectToServer(Server, UserId, Password, Port, Database, out exception);
		}

		/// <summary>
		/// Створити базу даних
		/// </summary>
		/// <param name="Server">Адреса сервера баз даних</param>
		/// <param name="UserId">Користувач</param>
		/// <param name="Password">Пароль</param>
		/// <param name="Port">Порт</param>
		/// <param name="Database">База даних</param>
		/// <param name="exception">Помилка</param>
		/// <param name="IsExistsDatabase">Чи вже є?</param>
		/// <returns>True якщо все ок</returns>
		public bool CreateDatabaseIfNotExist(string Server, string UserId, string Password, int Port, string Database, out Exception exception, out bool IsExistsDatabase)
		{
			DataBase = new PostgreSQL();
			return DataBase.CreateDatabaseIfNotExist(Server, UserId, Password, Port, Database, out exception, out IsExistsDatabase);
		}

		/// <summary>
		/// Підключення до сервера баз даних і завантаження конфігурації
		/// </summary>
		/// <param name="PathToXmlFileConfiguration">Шлях до файлу конфігурації</param>
		/// <param name="Server">Адреса сервера баз даних</param>
		/// <param name="UserId">Користувач</param>
		/// <param name="Password">Пароль</param>
		/// <param name="Port">Порт</param>
		/// <param name="Database">База даних</param>
		/// <param name="exception">Помилка</param>
		/// <returns>True якщо підключення відбулось нормально</returns>
		public bool Open(string PathToXmlFileConfiguration, string Server, string UserId, string Password, int Port, string Database, out Exception exception)
		{
			DataBase = new PostgreSQL();
			bool flagConnect = DataBase.Open2(Server, UserId, Password, Port, Database, out exception);

			DataBase_Server = Server;
			DataBase_UserId = UserId;
			DataBase_Port = Port.ToString();
			DataBase_BaseName = Database;

			try
			{
				Configuration conf;
				Configuration.Load(PathToXmlFileConfiguration, out conf);
				Conf = conf;
			}
            catch
            {
				return false;
            }

			Conf.PathToXmlFileConfiguration = PathToXmlFileConfiguration;

			return flagConnect;
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
		public bool OpenOnlyDataBase(string Server, string UserId, string Password, int Port, string Database, out Exception exception)
        {
			DataBase = new PostgreSQL();
			bool flagConnect = DataBase.Open2(Server, UserId, Password, Port, Database, out exception);

			DataBase_Server = Server;
			DataBase_UserId = UserId;
			DataBase_Port = Port.ToString();
			DataBase_BaseName = Database;

			return flagConnect;
		}

		/// <summary>
		/// Закрити підключення
		/// </summary>
		public void Close()
		{
			DataBase.Close();
			Conf = null;
		}

		/// <summary>
		/// Конфігурація
		/// </summary>
		public Configuration Conf { get; set; }

		/// <summary>
		/// Інтерфейс для роботи з базою даних
		/// </summary>
		public IDataBase DataBase { get; set; }

		#region DataBase Info

		public string DataBase_Server { get; private set; }
		public string DataBase_UserId { get; private set; }
		public string DataBase_Port { get; private set; }
		public string DataBase_BaseName { get; private set; }

        #endregion
    }
}
