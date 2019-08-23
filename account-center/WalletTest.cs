using CShapeDemo.account_center;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChatApiTest.account_center.walletTest
{
    class WalletTest
    {
        static void Main(string[] args)
        {
            WalletTest test = new WalletTest();
            Console.WriteLine(test.testGetBalance());
            Console.WriteLine(test.testWalletRecharge());
            Console.ReadKey();
        }

        public JObject testGetBalance()
        {
            JObject headParam = CommonUtils.GetCommonSystem();
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/balance";
            JObject result = RequestUtils.HttpGetRequest(url, headParam, null);
            if (result == null)
            {
                Console.WriteLine("请求失败");
                return null;
            }
            if (result.Value<int>("retcode") == 0)
            {
                string payload = CommonUtils.DecodeAES(result.Value<string>("payload"));
                try
                {
                    result["payload"] = JObject.Parse(payload);
                }
                catch
                {
                    result["payload"] = payload;
                }

            }
            return result;
        }

        public JObject testWalletRecharge()
        {
            JObject headParam = CommonUtils.GetCommonSystem();
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/balance/topup";
            JObject param = new JObject();
            param.Add("amount", "1");
            param.Add("paymentType", Utils.PaymentType.ALI_PAY.ToString());
            param.Add("mode", Utils.PaymentMode.QR_CODE.ToString());
            JObject result = RequestUtils.HttpPostRequest(url, headParam, param);
            if (result == null)
            {
                Console.WriteLine("请求失败");
                return null;
            }
            if (result.Value<int>("retcode") == 0)
            {
                string payload = CommonUtils.DecodeAES(result.Value<string>("payload"));
                JObject payloadJson = JObject.Parse(payload);
                try
                {
                    string signal = payloadJson.Value<string>("signal");
                    payloadJson["signal"] = HttpUtility.UrlDecode(signal);
                    result["payload"] = payloadJson; 
                }
                catch
                {
                    result["payload"] = payloadJson;
                }

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
                request.ContentType = "application/json";
                //request.ContentType = "text/plain";
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

    }

    class Utils
    {
        public enum PaymentType
        {
            ALI_PAY,
            WECHAT_PAY
        }

        public enum PaymentMode
        {
            APP,
            QR_CODE
        }
    }
}
