using ChatApiTest.account_center;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CShapeDemo.account_center
{
    class TokenTest
    {
        public void main(string[] args)
        {
            TokenTest test = new TokenTest();
            Console.WriteLine(test.testGetToken());
            //Console.WriteLine(test.testRefreshToken());
            Console.ReadKey();
        }

        private JObject testGetToken()
        {
            string appChannel = CommonUtils.GetFromConfig("appChannel");
            string timestamp = CommonUtils.GetTimestamp(DateTime.UtcNow);
            JObject param = new JObject();
            param.Add("App-Channel", appChannel);
            param.Add("Timestamp", timestamp);
            string sortedParams = Utils.GetSortedParamsString(param);
            string privateKey = CommonUtils.GetFromConfig("privateKey");
            string sign = Utils.GetSHA1Cipher(sortedParams + privateKey);
            param.Add("Sign", sign);
            string url = CommonUtils.GetFromConfig("token-host") + "/api/open/v1/channel/token";
            string token = RequestUtils.HttpGetRequest(url, param);
            if (token == null) return null;
            token = CommonUtils.DecodeAES(token);
            return JObject.Parse(token);
        }

        private JObject testRefreshToken()
        {
            JObject tokens = testGetToken();
            if (tokens == null) return null;
            string refreshToken = tokens.Value<string>("refreshToken");
            string timestamp = CommonUtils.GetTimestamp(DateTime.UtcNow);
            JObject param = new JObject();
            param.Add("Refresh-Token", refreshToken);
            param.Add("Timestamp", timestamp);
            string url = CommonUtils.GetFromConfig("token-host") + "/api/open/v1/channel/token";
            string token = RequestUtils.HttpPostRequest(url, param);
            if (token == null) return null;
            token = CommonUtils.DecodeAES(token);
            return JObject.Parse(token);
        }

        //需要获取用户Authorization才能
        //private string testDeleteToken()
        //{
        //    JObject tokens = testGetToken();
        //    if (tokens != null) return null;
        //    string token = tokens.Value<string>("accessToken");
        //    JObject param = new JObject();
        //    param.Add("Refresh-Token", refreshToken);
        //    param.Add("Timestamp", timestamp);
        //}

    }

    class RequestUtils
    {
        public static string HttpGetRequest(string url, JObject param)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            foreach (var item in param)
                request.Headers.Add(item.Key, item.Value.ToString());

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream repStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(repStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            repStream.Close();

            JObject result = JObject.Parse(retString);
            if (result.Value<int>("retcode") == 0)
                return result.Value<string>("payload");
            else return null;
        }

        public static string HttpPostRequest(string url, JObject param)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "text/plain";
            foreach (var item in param)
                request.Headers.Add(item.Key, item.Value.ToString());

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream repStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(repStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            repStream.Close();

            JObject result = JObject.Parse(retString);
            if (result.Value<int>("retcode") == 0)
                return result.Value<string>("payload");
            else return null;
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
