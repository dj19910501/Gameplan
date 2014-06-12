using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Helper
{
    class Common
    {
        public static string UnableToUpdate = "Unable to update";
        public static string UnableToDelete = "Unable to delete";

        /// <summary>
        /// Decrypt string
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string Decrypt(string strText)
        {
            byte[] byKey = null;
            byte[] IV = { 0X12, 0X34, 0X56, 0X78, 0X90, 0XAB, 0XCD, 0XEF };
            byte[] inputByteArray = new byte[strText.Length + 1];

            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(((string)("&%#@?,:*")).Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                inputByteArray = Convert.FromBase64String(strText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);

                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = System.Text.Encoding.UTF8;

                return encoding.GetString(ms.ToArray());

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string GetErrorMessage(Exception e)
        {
            if (e.InnerException != null)
            {
                return string.Format("{0}: {1}",e.Message, e.InnerException.Message);
            }
            else
            {
                return e.Message;
            }
            
        }

        internal static List<string> GetStatusListAfterApproved()
        {
            List<string> tacticStatus = new List<string>();
            tacticStatus.Add(ExternalIntegration.TacticStatusValues[TacticStatus.Approved.ToString()].ToString());
            tacticStatus.Add(ExternalIntegration.TacticStatusValues[TacticStatus.InProgress.ToString()].ToString());
            tacticStatus.Add(ExternalIntegration.TacticStatusValues[TacticStatus.Complete.ToString()].ToString());
            return tacticStatus;
        }

        internal static Enums.Mode GetMode(bool isDeleted, bool isDeployedToIntegration, string integrationInstanceTacticId, string status)
        {
            // delete reject status from list function
            List<string> statusList = Common.GetStatusListAfterApproved();
            // Status = After Approve - Is Deploy = true -  integrationInstanceTacticId = null - isDeleted = false
            if (statusList.Contains(status) && isDeployedToIntegration && string.IsNullOrWhiteSpace(integrationInstanceTacticId) && !isDeleted)
            {
                return Enums.Mode.Create;
            }
            // Status = After Approve - Is Deploy = true -  integrationInstanceTacticId = yes - isDeleted = false
            else if (statusList.Contains(status) && isDeployedToIntegration && !string.IsNullOrWhiteSpace(integrationInstanceTacticId) && !isDeleted)
            {
                return Enums.Mode.Update;
            }
            else if (!string.IsNullOrWhiteSpace(integrationInstanceTacticId))
            {
                return Enums.Mode.Delete;
            }
            else
            {
                return Enums.Mode.None;
            }
        }
    }
}
