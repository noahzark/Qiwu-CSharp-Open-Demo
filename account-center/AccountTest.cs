using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace CShapeDemo.account_center.accountTest
{
    class AccountTest
    {
        public void main(string[] args)
        {
            AccountTest test = new AccountTest();
            Console.WriteLine(test.testCaptchaSms());
            Console.WriteLine(test.testUserSignIn());
            Console.WriteLine(test.testRefreshToken());
            Console.WriteLine(test.testUserSignOut());
            Console.WriteLine(test.testUserInfo());
            Console.WriteLine(test.testAddSubAccount());
            Console.ReadKey();
        }

        /// <summary>
        /// 获取短信验证码
        /// </summary>
        /// <returns>返回的Json数据</returns>
        public JObject testCaptchaSms()
        {
            JObject param = new JObject();
            param.Add("Authorization", Utils.GetAuthString("15907558676:"));
            param.Add("Timestamp", CommonUtils.GetTimestamp());
            string token = CommonUtils.GetApiToken();
            if (token != null) param.Add("Access-Token", token);
            else
            {
                Console.WriteLine("商户token获取失败");
                return null;
            }
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/captcha/sms";
            JObject result = RequestUtils.HttpGetRequest(url, param, null);
            if (result == null)
            {
                Console.WriteLine("请求失败");
                return null;
            }
            if (result.Value<int>("retcode") == 0)
                Console.WriteLine("请求成功！");
            return result;
        }

        /* 用户登录 */
        public JObject testUserSignIn()
        {
            JObject param = new JObject();
            string account = CommonUtils.GetFromConfig("defaultAccount");
            param.Add("Authorization", Utils.GetAuthString(account));
            param.Add("Timestamp", CommonUtils.GetTimestamp());
            string token = CommonUtils.GetApiToken();
            if (token != null) param.Add("Access-Token", token);
            else
            {
                Console.WriteLine("商户token获取失败");
                return null;
            }
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/user/token";
            JObject result = RequestUtils.HttpGetRequest(url, param, null);
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
            result.Add("token", token);
            return result;
        }

        /* 刷新用户Token */
        public JObject testRefreshToken()
        {
            string refreshToken = CommonUtils.GetUserRefreshToken();
            if (refreshToken == null)
                return null;
            JObject param = new JObject();
            param.Add("Authorization", "Bearer " + refreshToken);
            param.Add("Timestamp", CommonUtils.GetTimestamp());
            string token = CommonUtils.GetApiToken();
            if (token != null) param.Add("Access-Token", token);
            else
            {
                Console.WriteLine("商户token获取失败");
                return null;
            }
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/user/token";
            JObject result = RequestUtils.HttpPutRequest(url, param);
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

        /* 用户登出 */
        public JObject testUserSignOut()
        {
            JObject param = CommonUtils.GetCommonSystem();

            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/user/token";
            JObject result = RequestUtils.HttpDeleteRequest(url, param);
            if (result == null)
            {
                Console.WriteLine("请求失败");
                return null;
            }
            return result;

        }

        /* 获取账户用户信息 */
        public JObject testUserInfo()
        {
            JObject headParam = CommonUtils.GetCommonSystem();
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/user";
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
        public JObject testAddSubAccount()
        {
            JObject headParam = CommonUtils.GetCommonSystem();
            headParam.Add("businessId", "testBusinessId");
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/user/sub";
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

    class RequestUtils
    {
        public static JObject HttpGetRequest(string url, JObject headParam, JObject param)
        {
            url += CommonUtils.GetQueryStringCipher(param);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            if (headParam != null)
            {
                foreach (var item in headParam)
                    request.Headers.Add(item.Key, item.Value.ToString());
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream repStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(repStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            repStream.Close();
            return JObject.Parse(retString);
        }

        public static JObject HttpPostRequest(string url, JObject headParam, JObject param)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            if (headParam != null)
            {
                foreach (var item in headParam)
                    request.Headers.Add(item.Key, item.Value.ToString());
            }
            if (param != null)
            {
                request.ContentType = "text/plain";
                string postStr = CommonUtils.GetBodyStringCipher(param);
                byte[] postData = Encoding.UTF8.GetBytes(postStr);
                request.ContentLength = postData.Length;
                Stream reqStream = request.GetRequestStream();
                reqStream.Write(postData, 0, postData.Length);
                reqStream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream repStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(repStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            repStream.Close();
            return JObject.Parse(retString);
        }

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

        public static JObject HttpDeleteRequest(string url, JObject headParam)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "DELETE";
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
