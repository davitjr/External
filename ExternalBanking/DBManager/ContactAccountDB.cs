using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
	internal class ContactAccountDB
	{
		internal static void Add(ContactAccount contactAccount, ulong contractId, SqlConnection connection)
		{
			ulong changeLogID;
			bool isTestVersion = bool.Parse(ConfigurationManager.AppSettings["TestVersion"].ToString());
			if (isTestVersion)
			{
				using (SqlCommand command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText = @"INSERT INTO tbl_ContactAccounts (ContactId,Description,AccountNumber,ObjectType,ObjectValue) Values(@ContactId,@Description,@ObjectValue,@ObjectType,@ObjectValue) Select SCOPE_IDENTITY() as ID ";
					command.Parameters.Add("@Description", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(contactAccount.Description) ? (object)DBNull.Value : contactAccount.Description;
					command.Parameters.Add("@ObjectType", SqlDbType.TinyInt).Value = contactAccount.ObjectType;
					command.Parameters.Add("@ObjectValue", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(contactAccount.ObjectValue) ? (object)DBNull.Value : contactAccount.ObjectValue;
					command.Parameters.Add("@ContactId", SqlDbType.BigInt).Value = contractId;
					contactAccount.Id = Convert.ToUInt64(command.ExecuteScalar());
				}
			}
			else
			{
				using (SqlCommand command1 = new SqlCommand())
				{
					command1.Connection = connection;
					command1.CommandText = @"INSERT INTO tbl_ContactAccounts (ContactId,Description,AccountNumber) Values(@ContactId,@Description,@AccountNumber) Select SCOPE_IDENTITY() as ID ";
					command1.Parameters.Add("@Description", SqlDbType.NVarChar).Value = contactAccount.Description;
					command1.Parameters.Add("@AccountNumber", SqlDbType.NVarChar).Value = contactAccount.AccountNumber;
					command1.Parameters.Add("@ContactId", SqlDbType.BigInt).Value = contractId;
					contactAccount.Id = Convert.ToUInt64(command1.ExecuteScalar());
				}
			}

			changeLogID = UtilityDB.InsertChangedLog(ObjectTypes.ContactAccount, contactAccount.Id, Action.Add, connection);
			AddContactAccountsHistory(contactAccount.Id, changeLogID, connection);

		}

		internal static void Update(ContactAccount contactAccount, SqlConnection connection)
		{
			ulong changeLogID;
			bool isTestVersion = bool.Parse(ConfigurationManager.AppSettings["TestVersion"].ToString());
			if (isTestVersion)
			{
				using (SqlCommand command1 = new SqlCommand())
				{
					command1.Connection = connection;
					command1.CommandText = "UPDATE tbl_ContactAccounts SET  Description = @Description, ObjectType = @ObjectType, ObjectValue = @ObjectValue WHERE ID = @ID ";
					command1.Parameters.Add("@Description", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(contactAccount.Description) ? (object)DBNull.Value : contactAccount.Description;
					command1.Parameters.Add("@ObjectType", SqlDbType.SmallInt).Value = contactAccount.ObjectType;
					command1.Parameters.Add("@ObjectValue", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(contactAccount.ObjectValue) ? (object)DBNull.Value : contactAccount.ObjectValue;
					command1.Parameters.Add("@ID", SqlDbType.BigInt).Value = contactAccount.Id;
					command1.ExecuteNonQuery();
				}
			}
			else
			{
				using (SqlCommand command1 = new SqlCommand())
				{
					command1.Connection = connection;
					command1.CommandText = "UPDATE tbl_ContactAccounts SET AccountNumber = @AccountNumber , Description = @Description WHERE ID = @ID ";
					command1.Parameters.Add("@AccountNumber", SqlDbType.NVarChar).Value = contactAccount.AccountNumber;
					command1.Parameters.Add("@Description", SqlDbType.NVarChar).Value = contactAccount.Description;
					command1.Parameters.Add("@ID", SqlDbType.BigInt).Value = contactAccount.Id;
					command1.ExecuteNonQuery();
				}
			}

			changeLogID = UtilityDB.InsertChangedLog(ObjectTypes.ContactAccount, contactAccount.Id, Action.Update, connection);
			AddContactAccountsHistory(contactAccount.Id, changeLogID, connection);
		}

		internal static void Delete(ulong Id, SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = @"DELETE FROM tbl_ContactAccounts WHERE ID =@ID";
				command.Parameters.Add("@ID", SqlDbType.BigInt).Value = Id;
				command.ExecuteNonQuery();
			}
			ulong changeLogID = UtilityDB.InsertChangedLog(ObjectTypes.ContactAccount, Id, Action.Delete, connection);
		}

		internal static void DeleteContactAccounts(ulong contractId, SqlConnection connection)
		{
			List<ulong> ContactAccountIdList = new List<ulong>();

			using (SqlCommand command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = @"Select ID FROM tbl_ContactAccounts WHERE ContactId =@ContactId";
				command.Parameters.Add("@ContactId", SqlDbType.BigInt).Value = contractId;

				using (SqlDataReader dataReader = command.ExecuteReader())
					while (dataReader.Read())
						ContactAccountIdList.Add(Convert.ToUInt64(dataReader["ID"]));
			}

			foreach (ulong id in ContactAccountIdList)
				UtilityDB.InsertChangedLog(ObjectTypes.ContactAccount, id, Action.Delete, connection);

			using (SqlCommand command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = @"DELETE FROM tbl_ContactAccounts WHERE ContactId =@ContactId";
				command.Parameters.Add("@ContactId", SqlDbType.BigInt).Value = contractId;
				command.ExecuteNonQuery();
			}
		}

		internal static List<ContactAccount> GetContactAccounts(ulong contactId)
		{
			List<ContactAccount> ContactAccountList = new List<ContactAccount>();

			using (SqlConnection connection = new SqlConnection())
			{
				connection.ConnectionString = ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString();
				connection.Open();

				bool isTestVersion = bool.Parse(ConfigurationManager.AppSettings["TestVersion"].ToString());
				if (isTestVersion)
				{
					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = @"SELECT Id,ContactId,AccountNumber,Description,ObjectType,ObjectValue FROM tbl_ContactAccounts WHERE ContactId = @ContactId";
						command.Parameters.Add("@ContactId", SqlDbType.BigInt).Value = contactId;

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							ContactAccount contactAccount;

							while (dataReader.Read())
							{
								contactAccount = new ContactAccount();
								ContactAccountList.Add(contactAccount);

								contactAccount.Id = Convert.ToUInt64(dataReader["ID"]);
								contactAccount.AccountNumber = dataReader["AccountNumber"].ToString();
								contactAccount.Description = dataReader["Description"] == DBNull.Value ? null : dataReader["Description"].ToString();
								if (dataReader["ObjectType"] != DBNull.Value)
								{
									contactAccount.ObjectType = short.Parse(dataReader["ObjectType"].ToString());
									contactAccount.ObjectValue = dataReader["ObjectValue"].ToString();

								}
							}
						}
					}
				}
				else
				{
					using (SqlCommand command1 = new SqlCommand())
					{
						command1.Connection = connection;
						command1.CommandText = @"SELECT Id,ContactId,AccountNumber,Description FROM tbl_ContactAccounts WHERE ContactId = @ContactId";
						command1.Parameters.Add("@ContactId", SqlDbType.BigInt).Value = contactId;

						using (SqlDataReader dataReader = command1.ExecuteReader())
						{
							ContactAccount contactAccount;

							while (dataReader.Read())
							{
								contactAccount = new ContactAccount();
								ContactAccountList.Add(contactAccount);

								contactAccount.Id = Convert.ToUInt64(dataReader["ID"]);
								contactAccount.AccountNumber = dataReader["AccountNumber"].ToString();
								contactAccount.Description = dataReader["Description"] == DBNull.Value ? null : dataReader["Description"].ToString();
							}
						}
					}
				}
			}
			return ContactAccountList;
		}

		internal static void AddContactAccountsHistory(ulong contactAccountId, ulong changeLogID, SqlConnection connection)
		{

			bool isTestVersion = bool.Parse(ConfigurationManager.AppSettings["TestVersion"].ToString());
			if (isTestVersion)
			{
				using (SqlCommand command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText = @"INSERT INTO tbl_ContactAccountsHistory (ContactAccountId,ContactId,AccountNumber,Description,ChangeLogID,ObjectType,ObjectValue)
									SELECT ID as ContactAccountId,ContactId,AccountNumber,Description,@ChangeLogID,ObjectType,ObjectValue from tbl_ContactAccounts where id  = @Id";
					command.Parameters.Add("@ChangeLogID", SqlDbType.BigInt).Value = changeLogID;
					command.Parameters.Add("@Id", SqlDbType.BigInt).Value = contactAccountId;
					command.ExecuteNonQuery();
				}
			}
			else
			{
				using (SqlCommand command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText = @"INSERT INTO tbl_ContactAccountsHistory (ContactAccountId,ContactId,AccountNumber,Description,ChangeLogID)
									SELECT ID as ContactAccountId,ContactId,AccountNumber,Description,@ChangeLogID from tbl_ContactAccounts where id  = @Id";
					command.Parameters.Add("@ChangeLogID", SqlDbType.BigInt).Value = changeLogID;
					command.Parameters.Add("@Id", SqlDbType.BigInt).Value = contactAccountId;
					command.ExecuteNonQuery();
				}
			}
		}
	}
}