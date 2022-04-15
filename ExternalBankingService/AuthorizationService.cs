using ACBALibrary;
using ExternalBanking.ServiceClient;
using ExternalBankingService.InfSecServiceReference;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using infsec = ExternalBankingService.InfSecServiceReference;

namespace ExternalBankingService
{
    public static class AuthorizationService
    {
        public static infsec.AuthorizedUser AuthorizeUserBySessionToken(string userSessionToken)
        {
            var authorizedUser = new infsec.AuthorizedUser();

            Use(client =>
            {
                authorizedUser = client.AuthorizeUserBySession(userSessionToken);
            });

            return authorizedUser;
        }

        public static Dictionary<string, string> InitUserPagePermissions(string userSessionToken)
        {
            var advancedOptions = new Dictionary<string, string>();
            Use(client =>
            {
                advancedOptions = client.GetVarPermissionsForPageBySession(userSessionToken);
            });

            return advancedOptions;
        }

        public static User InitUser(infsec.AuthorizedUser authUser)
        {

            User user = new User();
            user.filialCode = authUser.filialCode;
            user.transRight = authUser.transRight;
            user.userCustomerNumber = authUser.userCustomerNumber;
            user.userID = authUser.userID;
            user.userName = authUser.userName;
            user.userPermissionId = authUser.userGroupID;
            user.DepartmentId = authUser.departmentID;
            user.TransactionLimit = authUser.transLimit;
            user.AccountGroup = authUser.userAccountGroup;
            if (authUser.isAutorized)
            {
                user.AdvancedOptions = InitUserPagePermissions(authUser.userSessionToken);
                string isChiefAcc = "";
                string isManager = "";
                if (user.AdvancedOptions.TryGetValue("Is_ChiefAcc", out isChiefAcc))
                {
                    if (isChiefAcc == "1")
                        user.IsChiefAcc = true;
                }
                if (user.AdvancedOptions.TryGetValue("Is_Manager", out isManager))
                {
                    if (isManager == "1")
                        user.IsManager = true;
                }
            }
            else
            {
                user.AdvancedOptions = new Dictionary<string, string>();
            }
            return user;
        }

        public static short GetVarPermissionForUser(short userID, string varName)
        {
            var cpInfo = new infsec.ClientPermissionsInfo();
            cpInfo.userID = userID;
            cpInfo.pageName = "-1";
            cpInfo.progName = "-1";
            cpInfo.varPropertyName = varName;

            var varPermissions = new infsec.ClientPermissions();

            Use(client =>
            {
                varPermissions = client.GetVarPermissionForPage(cpInfo);
            });

            return Convert.ToInt16(Convert.ToBoolean(varPermissions.valueOfPermission));

        }

        public static infsec.UserAccessForCustomer GetUserAccessForCustomer(string userSessionToken, string customerSessionToken)
        {
            var userAccessForCustomer = new infsec.UserAccessForCustomer();

            Use(client =>
            {
                userAccessForCustomer = client.GetUserAccessForCustomer(userSessionToken, customerSessionToken);
            });

            return userAccessForCustomer;
        }

        public static infsec.AuthorizedUser AuthorizeUserBySAPTicket(string ticket, string softName)
        {
            var authorizedUser = new infsec.AuthorizedUser();

            Use(client =>
            {
                authorizedUser = client.AuthorizeUserBySAPTicket(ticket, softName);
            });

            return authorizedUser;
        }
        /// <summary>
        /// Թույլատրված հաշվետվությունների ցանկի ստացում
        /// </summary>
        /// <param name="userReportPermissionInfo"></param>
        /// <returns></returns>
        public static List<infsec.ApplicationClientPermissions> GetPermittedReports(infsec.ApplicationClientPermissionsInfo userReportPermissionInfo)
        {
            List<infsec.ApplicationClientPermissions> appClientPermission = new List<infsec.ApplicationClientPermissions>();

            Use(client =>
            {
                appClientPermission = client.GetGridAccessListForForm(userReportPermissionInfo);
            });

            return appClientPermission;
        }

        public static void Use(Action<IInfSec> action)
        {
            IInfSec client = ProxyManager<IInfSec>.GetProxy(nameof(IInfSec));

            bool success = false;

            try
            {
                action(client);
                ((IClientChannel)client).Close();

                success = true;
            }

            catch (FaultException)

            {
                ((IClientChannel)client).Close();

                throw;
            }
            catch (TimeoutException)
            {

            }
            catch (Exception)
            {
                ((IClientChannel)client).Abort();
                throw;
            }
            finally
            {
                if (!success)
                {
                    ((IClientChannel)client).Abort();

                }
                ((IClientChannel)client).Dispose();
            }
        }


        public static string CreateLogonTicket(string userSessionToken)
        {
            string logonTicket = null;
            Use(client =>
            {
                logonTicket = client.CreateLogonTicket(userSessionToken);
            });
            return logonTicket;

        }

        public static infsec.AuthorizedUser AuthorizeUser(LoginInfo loginInfo)
        {
            infsec.AuthorizedUser authorizedUser = null;

            Use(client =>
            {
                authorizedUser = client.AutorizeUser(loginInfo);
            });

            return authorizedUser;
        }

    }
}
