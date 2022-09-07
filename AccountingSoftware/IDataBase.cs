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
using System.Collections.Generic;

namespace AccountingSoftware
{
	/// <summary>
	/// Інтерфейс для роботи з базою даних.
	/// </summary>
	public interface IDataBase
	{
        #region Open

        void Open(string connectionString);
		void Close();

		bool Open2(string Server, string UserId, string Password, int Port, string Database, out Exception exception);
		bool TryConnectToServer(string Server, string UserId, string Password, int Port, string Database, out Exception exception);
		bool CreateDatabaseIfNotExist(string Server, string UserId, string Password, int Port, string Database, out Exception exception, out bool IsExistsDatabase);

        #endregion

        #region Transaction

        void BeginTransaction();
		void CommitTransaction();
		void RollbackTransaction();

		#endregion

		#region Constants

		bool SelectAllConstants(string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		bool SelectConstants(string table, string field, Dictionary<string, object> fieldValue);
		void SaveConstants(string table, string field, object fieldValue);

		void SelectConstantsTablePartRecords(string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
		void InsertConstantsTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		void DeleteConstantsTablePartRecords(string table);

		#endregion

		#region Directory

		void InsertDirectoryObject(DirectoryObject directoryObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		void UpdateDirectoryObject(DirectoryObject directoryObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		bool SelectDirectoryObject(DirectoryObject directoryObject, UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		void DeleteDirectoryObject(UnigueID unigueID, string table);

		void SelectDirectoryPointers(Query QuerySelect, List<DirectoryPointer> listDirectoryPointer);
		bool FindDirectoryPointer(Query QuerySelect, ref DirectoryPointer directoryPointer);
		string GetDirectoryPresentation(Query QuerySelect, string[] fieldPresentation);
		void DeleteDirectoryTempTable(DirectorySelect directorySelect);

		void SelectDirectoryTablePartRecords(UnigueID ownerUnigueID, string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
		void InsertDirectoryTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		void DeleteDirectoryTablePartRecords(UnigueID ownerUnigueID, string table);

		#endregion

		#region Document

		void InsertDocumentObject(UnigueID unigueID, bool spend, DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		void UpdateDocumentObject(UnigueID unigueID, bool spend, DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		bool SelectDocumentObject(UnigueID unigueID, ref bool spend, ref DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		void DeleteDocumentObject(UnigueID unigueID, string table);

		void SelectDocumentPointer(Query QuerySelect, List<DocumentPointer> listDocumentPointer);
		string GetDocumentPresentation(Query QuerySelect, string[] fieldPresentation);

		void SelectDocumentTablePartRecords(UnigueID ownerUnigueID, string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
		void SelectDocumentTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList);

		void InsertDocumentTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		void DeleteDocumentTablePartRecords(UnigueID ownerUnigueID, string table);

		#endregion

		#region Journal

		void SelectJournalDocumentPointer(string[] tables, string[] typeDocument, List<JournalDocument> listDocumentPointer, 
			DateTime periodStart, DateTime periodEnd, string[] typeDocSelect = null);

		#endregion

		#region RegisterInformation

		void SelectRegisterInformationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList);
		void InsertRegisterInformationRecords(Guid UID, string table, DateTime period, Guid owner, string[] fieldArray, Dictionary<string, object> fieldValue);
		void DeleteRegisterInformationRecords(string table, Guid owner);

		void InsertRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		void UpdateRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		bool SelectRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
		void DeleteRegisterInformationObject(string table, UnigueID uid);

		#endregion

		#region RegisterAccumulation

		void SelectRegisterAccumulationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList);
		void InsertRegisterAccumulationRecords(Guid UID, string table, DateTime period, bool income, Guid owner, string[] fieldArray, Dictionary<string, object> fieldValue);
		void DeleteRegisterAccumulationRecords(string table, Guid owner);

		#endregion

		#region InformationShema

		ConfigurationInformationSchema SelectInformationSchema();
		bool IfExistsTable(string tableName);
		bool IfExistsColumn(string tableName, string columnName);

        #endregion

        #region SQL

        int InsertSQL(string table, Dictionary<string, object> paramQuery);
		int ExecuteSQL(string sqlQuery);
		int ExecuteSQL(string sqlQuery, Dictionary<string, object> paramQuery);
		void SelectRequest(string selectQuery, Dictionary<string, object> paramQuery, out string[] columnsName, out List<object[]> listRow);
		void SelectRequest(string selectQuery, Dictionary<string, object> paramQuery, out string[] columnsName, out List<Dictionary<string, object>> listRow);

		#endregion
	}
}
