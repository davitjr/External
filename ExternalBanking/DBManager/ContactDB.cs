using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ExternalBanking.DBManager
{
	internal class ContactDB
	{
		internal static void Add(Contact contact, ulong customerNumber, SqlConnection conn)
		{
			//bool isTestVersion = bool.Parse(ConfigurationManager.AppSettings["TestVersion"].ToString());
			//if (isTestVersion)
			//{
			using (SqlCommand cmd = new SqlCommand())
			{
				cmd.Connection = conn;
				cmd.CommandText = @"INSERT INTO tbl_Contacts (Description,customerNumber,PhoneNumber,Email,Picture,IsNew,PhoneCountryCode) Values(@Description,@customerNumber,@phoneNumber,@email,@picture,1,@PhoneCountryCode) Select SCOPE_IDENTITY() as ID ";
				cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = contact.Description;
				cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
				cmd.Parameters.Add("@phoneNumber", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(contact.PhoneNumber) ? (object)DBNull.Value : contact.PhoneNumber;
				cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(contact.Email) ? (object)DBNull.Value : contact.Email;
				cmd.Parameters.Add("@picture", SqlDbType.VarBinary).Value = string.IsNullOrEmpty(contact.Picture) ? (object)DBNull.Value : Encoding.UTF8.GetBytes(contact.Picture);
				cmd.Parameters.Add("@PhoneCountryCode", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(contact.PhoneCountryCode) ? (object)DBNull.Value : contact.PhoneCountryCode;

				contact.Id = Convert.ToUInt64(cmd.ExecuteScalar());
			}
			//}
			//else
			//{
			//	using (SqlCommand cmd1 = new SqlCommand())
			//	{
			//		cmd1.Connection = conn;
			//		cmd1.CommandText = @"INSERT INTO tbl_Contacts (Description,customerNumber) Values(@Description,@customerNumber) Select SCOPE_IDENTITY() as ID ";
			//		cmd1.Parameters.Add("@Description", SqlDbType.NVarChar).Value = contact.Description;
			//		cmd1.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
			//		contact.Id = Convert.ToUInt64(cmd1.ExecuteScalar());
			//	}
			//}

			ulong changeLogID = UtilityDB.InsertChangedLog(ObjectTypes.Contact, contact.Id, Action.Add, conn);
			AddContactHistory(contact.Id, changeLogID, conn);

		}

		internal static void Update(Contact contact, SqlConnection connection)
		{

			//bool isTestVersion = bool.Parse(ConfigurationManager.AppSettings["TestVersion"].ToString());
			//if (isTestVersion)
			//{
			using (SqlCommand command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = @"UPDATE tbl_Contacts SET Description = @Description, email = @email, phonenumber = @phonenumber, Picture = @picture, isnew = 1, PhoneCountryCode = @PhoneCountryCode WHERE ID =@ID ";
				command.Parameters.Add("@Description", SqlDbType.NVarChar).Value = contact.Description;
				command.Parameters.Add("@ID", SqlDbType.NVarChar).Value = contact.Id;
				command.Parameters.Add("@email", SqlDbType.NVarChar, 250).Value = string.IsNullOrEmpty(contact.Email) ? (object)DBNull.Value : contact.Email;
				command.Parameters.Add("@phonenumber", SqlDbType.NVarChar, 50).Value = string.IsNullOrEmpty(contact.PhoneNumber) ? (object)DBNull.Value : contact.PhoneNumber;
				command.Parameters.Add("@picture", SqlDbType.VarBinary).Value = string.IsNullOrEmpty(contact.Picture) ? (object)DBNull.Value : Encoding.UTF8.GetBytes(contact.Picture);
				command.Parameters.Add("@PhoneCountryCode", SqlDbType.NVarChar, 20).Value = string.IsNullOrEmpty(contact.PhoneCountryCode) ? (object)DBNull.Value : contact.PhoneCountryCode;

				command.ExecuteNonQuery();
			}
			//}
			//else
			//{
			//        using (SqlCommand command1 = new SqlCommand())
			//{
			//	command1.Connection = connection;
			//	command1.CommandText = @"UPDATE tbl_Contacts SET Description = @Description WHERE ID =@ID ";
			//	command1.Parameters.Add("@Description", SqlDbType.NVarChar).Value = contact.Description;
			//	command1.Parameters.Add("@ID", SqlDbType.NVarChar).Value = contact.Id;
			//	command1.ExecuteNonQuery();
			//}
			//}
			ulong changeLogID = UtilityDB.InsertChangedLog(ObjectTypes.Contact, contact.Id, Action.Update, connection);
			AddContactHistory(contact.Id, changeLogID, connection);
		}

		internal static void Delete(ulong ContactId, SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = @"DELETE FROM tbl_Contacts WHERE ID =@ID";
				command.Parameters.Add("@ID", SqlDbType.BigInt).Value = ContactId;
				command.ExecuteNonQuery();
			}
			ulong changeLogID = UtilityDB.InsertChangedLog(ObjectTypes.Contact, ContactId, Action.Delete, connection);
		}

		internal static Contact GetContact(ulong contactId)
		{
			Contact contact = null;

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
			{
				conn.Open();
				//            bool isTestVersion = bool.Parse(ConfigurationManager.AppSettings["TestVersion"].ToString());
				//            if (isTestVersion)
				//            {
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = @"SELECT ID,Description,phonenumber,email,picture,PhoneCountryCode FROM tbl_Contacts WHERE ID =@ID ";
					cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = contactId;

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						if (dr.Read())
						{
							contact = new Contact();
							contact.Id = Convert.ToUInt64(dr["id"]);
							contact.Description = dr["Description"].ToString();
							contact.PhoneNumber = dr["Phonenumber"].ToString();
							contact.Email = dr["email"].ToString();
							contact.Picture = dr["picture"] == DBNull.Value ? "" : Convert.ToBase64String((byte[])dr["picture"]);
							var base64EncodedBytes = System.Convert.FromBase64String(contact.Picture);
							contact.Picture = Encoding.UTF8.GetString(base64EncodedBytes);
							contact.PhoneCountryCode = dr["PhoneCountryCode"].ToString();
						}
					}
				}
				//            }
				//            else
				//            {
				//                using (SqlCommand cmd = new SqlCommand())
				//	{
				//		cmd.Connection = conn;
				//		cmd.CommandText = @"SELECT ID,Description FROM tbl_Contacts WHERE ID =@ID ";
				//		cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = contactId;

				//		using (SqlDataReader dr = cmd.ExecuteReader())
				//		{
				//			if (dr.Read())
				//			{
				//				contact = new Contact();
				//				contact.Id = Convert.ToUInt64(dr["id"]);
				//				contact.Description = dr["Description"].ToString();
				//			}
				//		}
				//	}
				//}
				//}
				return contact;
			}
		}

		internal static void AddContactHistory(ulong contactId, ulong changeLogID, SqlConnection connection)
		{

			//bool isTestVersion = bool.Parse(ConfigurationManager.AppSettings["TestVersion"].ToString());
			//if (isTestVersion)
			//{
			using (SqlCommand command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = @"INSERT INTO tbl_ContactsHistory (ContactId,Description,CustomerNumber,ChangeLogID,PhoneNumber,Email,Picture,IsNew)
									SELECT ID as ContactId,Description,CustomerNumber,@ChangeLogID,PhoneNumber,Email,Picture,IsNew from tbl_Contacts where id  = @Id";
				command.Parameters.Add("@ChangeLogID", SqlDbType.BigInt).Value = changeLogID;
				command.Parameters.Add("@Id", SqlDbType.BigInt).Value = contactId;
				command.ExecuteNonQuery();

			}
			//}
			//else
			//{
			//	using (SqlCommand command = new SqlCommand())
			//	{
			//		command.Connection = connection;
			//		command.CommandText = @"INSERT INTO tbl_ContactsHistory (ContactId,Description,CustomerNumber,ChangeLogID)
			//						SELECT ID as ContactId,Description,CustomerNumber,@ChangeLogID from tbl_Contacts where id  = @Id";
			//		command.Parameters.Add("@ChangeLogID", SqlDbType.BigInt).Value = changeLogID;
			//		command.Parameters.Add("@Id", SqlDbType.BigInt).Value = contactId;
			//		command.ExecuteNonQuery();



			//	}
			//}



		}

		internal static List<Contact> GetContacts(ulong customerNumber, SqlConnection connection)
		{
			List<Contact> contactList = new List<Contact>();

			//bool isTestVersion = bool.Parse(ConfigurationManager.AppSettings["TestVersion"].ToString());
			//if (isTestVersion)
			//{
			using (SqlCommand cmd = new SqlCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = @"SELECT ID,Description,phonenumber,email,picture, PhoneCountryCode FROM tbl_Contacts WHERE customerNumber =@customerNumber ";
				cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						Contact contact = new Contact();
						contactList.Add(contact);
						contact.Id = Convert.ToUInt64(dr["ID"]);
						contact.Description = dr["Description"].ToString();
						contact.PhoneNumber = dr["Phonenumber"].ToString();
						contact.Email = dr["email"].ToString();
						contact.Picture = dr["picture"] == DBNull.Value ? "" : Convert.ToBase64String((byte[])dr["picture"]);
						var base64EncodedBytes = System.Convert.FromBase64String(contact.Picture);
						contact.Picture = Encoding.UTF8.GetString(base64EncodedBytes);
						contact.PhoneCountryCode = dr["PhoneCountryCode"].ToString();
					}
				}
			}
			//}
			//else
			//{
			//	using (SqlCommand cmd = new SqlCommand())
			//	{
			//		cmd.Connection = connection;
			//		cmd.CommandText = @"SELECT ID,Description FROM tbl_Contacts WHERE customerNumber =@customerNumber ";
			//		cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

			//		using (SqlDataReader dr = cmd.ExecuteReader())
			//		{
			//			while (dr.Read())
			//			{
			//				Contact contact = new Contact();
			//				contactList.Add(contact);
			//				contact.Id = Convert.ToUInt64(dr["id"]);
			//				contact.Description = dr["Description"].ToString();
			//			}
			//		}
			//	}
			//}


			return contactList;
		}

		internal static int GetContactsCount(ulong customerNumber)
		{
			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
			{
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = @"SELECT Count(ID) as count FROM tbl_Contacts WHERE customerNumber = @customerNumber ";
					cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
					conn.Open();
					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						if (dr.Read())
						{
							return Convert.ToInt32(dr["count"]);
						}
					}
				}
			}
			return 0;
		}
	}
}