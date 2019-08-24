using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CShapeDemo.account_center.tokenTest
{
    class TokenTest
    {
        /* 获取商户Token */
        public JObject testGetToken(string appChannel, string privateKey)
        {
            string timestamp = CommonUtils.GetTimestamp();
            JObject param = new JObject();
            param.Add("App-Channel", appChannel);
            param.Add("Timestamp", timestamp);
            string sortedParams = Utils.GetSortedParamsString(param);
            string sign = Utils.GetSHA1Cipher(sortedParams + privateKey);
            param.Add("Sign", sign);
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/channel/token";
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
                CommonUtils.WriteDataConfig("apiToken", accessToken);
                CommonUtils.WriteDataConfig("apiRefreshToken", refreshToken);
                result["payload"] = payloadJson;
            }
            return result;
        }

        /* 刷新商户Token */
        public JObject testRefreshToken(string apiRefreshToken)
        {
            string timestamp = CommonUtils.GetTimestamp();
            JObject headParam = new JObject();
            headParam.Add("Refresh-Token", apiRefreshToken);
            headParam.Add("Timestamp", timestamp);
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/channel/token";
            JObject result = RequestUtils.HttpPostRequest(url, headParam, null);
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
                CommonUtils.WriteDataConfig("apiToken", accessToken);
                CommonUtils.WriteDataConfig("apiRefreshToken", refreshToken);
                result["payload"] = payloadJson;
            }
            return result;
        }

        /* 注销token */
        public JObject testDeleteToken(string apiToken)
        {
            JObject headParam = new JObject();
            headParam.Add("Access-Token", apiToken);
            headParam.Add("Timestamp", CommonUtils.GetTimestamp());
            //JObject headParam = CommonUtils.GetCommonSystem();
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/channel/token";
            JObject result = RequestUtils.HttpDeleteRequest(url, headParam, null);
            if (result == null)
            {
                Console.WriteLine("请求失败！");
                return null;
            }
            CommonUtils.DeleteDataConfig("apiToken");
            CommonUtils.DeleteDataConfig("apiRefreshToken");
            return result;
        }

    }

    class Utils
    {
        public static string GetSortedParamsString(JObject param)
        {
            List<string> list = new List<string>();
            foreach (var item in param)
                list.Add(item.Key);
            string[] keys = list.ToArray();
            Array.Sort(keys, string.CompareOrdinal);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string key in keys)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append("&");
                stringBuilder.Append(key + "=" + param.Value<string>(key));
            }
            return stringBuilder.ToString();
        }

        public static string GetSHA1Cipher(string str)
        {
            SHA1 sha1 = SHA1.Create();
            byte[] cipher = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
            string ciphertext = "";
            foreach (byte b in cipher)
            {
                ciphertext += b.ToString("x2");
            }
            return ciphertext;
        }

    }
}
