/*
Copyright (C) 2019-2023 TARAKHOMYN YURIY IVANOVYCH
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
	/// Унікальний ідентифікатор
	/// </summary>
	public class UnigueID
	{
		/// <summary>
		/// Пустий вказівник
		/// </summary>
		public UnigueID()
		{
			UGuid = Guid.Empty;
        }

        /// <summary>
        /// Унікальний ідентифікатор
        /// </summary>
        /// <param name="uGuid">Унікальний ідентифікатор</param>
        public UnigueID(Guid uGuid)
		{
			UGuid = uGuid;
		}

		/// <summary>
		/// Унікальний ідентифікатор
		/// </summary>
		/// <param name="uGuid">Унікальний ідентифікатор як object</param>
		public UnigueID(object? uGuid)
		{
			if (uGuid != null && uGuid != DBNull.Value)
				UGuid = (Guid)uGuid;
			else
				UGuid = Guid.Empty;
		}

		/// <summary>
		/// Унікальний ідентифікатор у формі тексту. Використовується Guid.Parse(uGuid).
		/// </summary>
		/// <param name="uGuid">Унікальний ідентифікатор</param>
		public UnigueID(string uGuid)
		{
			Guid resultUGuid;

			if (Guid.TryParse(uGuid, out resultUGuid))
				UGuid = resultUGuid;
			else
				UGuid = Guid.Empty;
		}

		/// <summary>
		/// Чи це пустий вказівник?
		/// </summary>
		/// <returns></returns>
		public bool IsEmpty()
		{
			return UGuid == Guid.Empty;
        }

		/// <summary>
		/// Згенерувати новий ідентифікатор
		/// </summary>
		public void New()
		{
			UGuid = Guid.NewGuid();
        }

        public static UnigueID NewUnigueID()
        {
			return new UnigueID(Guid.NewGuid());
        }

        /// <summary>
        /// Унікальний ідентифікатор
        /// </summary>
        public Guid UGuid { get; private set; }

		public override string ToString()
		{
			return UGuid.ToString();
		}
	}
}