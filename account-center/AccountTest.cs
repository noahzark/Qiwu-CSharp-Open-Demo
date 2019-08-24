using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace CShapeDemo.account_center.accountTest
{
    class AccountTest
    {
        private string apiToken;

        public AccountTest(string apiToken)
        {
            this.apiToken = apiToken;
        }

        /// <summary>
        /// 获取短信验证码
        /// </summary>
        /// <returns>返回的Json数据</returns>
        public JObject testCaptchaSms(string phone)
        {
            JObject param = new JObject();
            param.Add("Authorization", Utils.GetAuthString(phone + ":"));
            //param.Add("Authorization", Utils.GetAuthString("15907558676:"));
            param.Add("Timestamp", CommonUtils.GetTimestamp());
            if (apiToken != null) param.Add("Access-Token", apiToken);
            else
            {
                Console.WriteLine("商户token获取失败");
                return null;
            }
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/captcha/sms";
            JObject result = RequestUtils.HttpGetRequest(url, param, null);
            if (result == null)
            {
                Console.WriteLine("请求失败");
                return null;
            }
            if (result.Value<int>("retcode") == 0)
                Console.WriteLine("短信验证码发送成功！");
            return result;
        }

        /* 用户登录 */
        /// <summary>
        ///     商户下的用户登录
        /// </summary>
        /// <param name="apiToken"></param>
        /// <param name="account">手机号:验证码</param>
        /// <returns></returns>
        public JObject testUserSignIn(string phone, string sms)
        {
            JObject param = new JObject();
            param.Add("Authorization", Utils.GetAuthString(phone + ":" + sms));
            param.Add("Timestamp", CommonUtils.GetTimestamp());
            if (apiToken != null) param.Add("Access-Token", apiToken);
            else
            {
                Console.WriteLine("商户token获取失败");
                return null;
            }
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/user/token";
            JObject result = RequestUtils.HttpGetRequest(url, param, null);
            if (result == null)
            {
                Console.WriteLine("请求失败");
                return null;
            }
            if (result.Value<int>("retcode") == 0)
            {
                string payload = CommonUtils.DecodeAES(result.Value<string>("payload"));
                JObject payloadJson = JObject.Parse(payload);
                string accessToken = payloadJson.Value<string>("accessToken");
                string refreshToken = payloadJson.Value<string>("refreshToken");
                CommonUtils.WriteDataConfig("userToken", accessToken);
                CommonUtils.WriteDataConfig("userRefreshToken", refreshToken);
                result["payload"] = payloadJson;
            }
            return result;
        }

        /* 刷新用户Token */
        public JObject testRefreshToken(string refreshToken)
        {
            //string refreshToken = CommonUtils.GetDataConfig("userRefreshToken");
            if (refreshToken == null)
                return null;
            JObject param = new JObject();
            param.Add("Authorization", "Bearer " + refreshToken);
            param.Add("Timestamp", CommonUtils.GetTimestamp());
            if (apiToken != null) param.Add("Access-Token", apiToken);
            else
            {
                Console.WriteLine("商户token获取失败");
                return null;
            }
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/user/token";
            JObject result = CurRequestUtils.HttpPutRequest(url, param);
            if (result == null)
            {
                Console.WriteLine("请求失败");
                return null;
            }
            if (result.Value<int>("retcode") == 0)
            {
                string payload = CommonUtils.DecodeAES(result.Value<string>("payload"));
                JObject payloadJson = JObject.Parse(payload);
                string accessToken = payloadJson.Value<string>("accessToken");
                refreshToken = payloadJson.Value<string>("refreshToken");
                CommonUtils.WriteDataConfig("userToken", accessToken);
                CommonUtils.WriteDataConfig("userRefreshToken", refreshToken);
                result["payload"] = payloadJson;
            }
            return result;
        }

        /* 用户登出 */
        public JObject testUserSignOut(string userToken)
        {
            JObject param = CommonUtils.GetCommonSystem(apiToken, userToken);
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/user/token";
            JObject result = RequestUtils.HttpDeleteRequest(url, param, null);
            if (result == null)
            {
                Console.WriteLine("请求失败");
                return null;
            }
            return result;

        }

        /* 获取账户用户信息 */
        public JObject testUserInfo(string userToken)
        {
            JObject headParam = CommonUtils.GetCommonSystem(apiToken, userToken);
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/user";
            JObject result = RequestUtils.HttpGetRequest(url, headParam, null);
            if (result == null)
            {
                Console.WriteLine("请求失败");
                return null;
            }
            if (result.Value<int>("retcode") == 0)
            {
                string payload = CommonUtils.DecodeAES(result.Value<string>("payload"));
                result["payload"] = JObject.Parse(payload);
            }
            return result;
        }

        /* 新增子账号 */
        public JObject testAddSubAccount(string userToken,string businessId)
        {
            JObject headParam = CommonUtils.GetCommonSystem(apiToken, userToken);
            headParam.Add("businessId", businessId);
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/user/sub";
            JObject result = RequestUtils.HttpPostRequest(url, headParam, null);
            if (result == null)
            {
                Console.WriteLine("请求失败");
                return null;
            }
            if (result.Value<int>("retcode") == 0)
            {
                string payload = CommonUtils.DecodeAES(result.Value<string>("payload"));
                result["payload"] = JObject.Parse(payload);
            }
            return result;
        }

    }

    class CurRequestUtils
    {
        public static JObject HttpPutRequest(string url, JObject headParam)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            foreach (var item in headParam)
                request.Headers.Add(item.Key, item.Value.ToString());

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream repStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(repStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            repStream.Close();
            return JObject.Parse(retString);
        }

    }

    class Utils
    {
        public static string GetAuthString(string msg)
        {
            byte[] bytes = Encoding.Default.GetBytes(msg);
            string cipher = Convert.ToBase64String(bytes);
            StringBuilder stringBuilder = new StringBuilder("Basic ");
            stringBuilder.Append(cipher);
            return stringBuilder.ToString();
        }

    }
}
