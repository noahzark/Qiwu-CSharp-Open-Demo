using CShapeDemo.account_center;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ChatApiTest.account_center
{
    class AccountTest
    {
        static void Main(string[] args)
        {
            AccountTest test = new AccountTest();
            Console.WriteLine(test.testCaptchaSms());
            Console.ReadKey();
        }

        public JObject testCaptchaSms()
        {
            JObject param = new JObject();
            param.Add("Authorization", Utils.GetAuthString("15907558676"));
            string url = Utils.GetFromConfig("token-host") + "/api/open/v1/captcha/sms";
            string result = RequestUtils.HttpGetRequest(url, param);
            if (result == null) return null;
            result = Utils.DecodeAES(result);
            return JObject.Parse(result);
        }
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
    }

    class Utils
    {
        public static JObject GetSystemParams()
        {
            TokenTest tokenTest
        }

        public static string GetAuthString(string phone)
        {
            byte[] bytes = Encoding.Default.GetBytes(phone+":");
            string cipher = Convert.ToBase64String(bytes);
            StringBuilder stringBuilder = new StringBuilder("Auth ");
            stringBuilder.Append(cipher);
            return stringBuilder.ToString();
        }

    }
}
