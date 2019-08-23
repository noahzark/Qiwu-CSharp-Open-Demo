using CShapeDemo.account_center;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace ChatApiTest.account_center.paymentTest
{
    class PaymentTest
    {
        public void main(string[] args)
        {
            PaymentTest test = new PaymentTest();
            Console.WriteLine(test.testOrderPay());
            Console.WriteLine(test.testOrderPayQR());
            Console.ReadKey();
        }

        public JObject testOrderPay()
        {
            string oid = "f2019073011321849782193";
            JObject param = new JObject();
            param.Add("orderId", oid);
            param.Add("paymentType", (int)Utils.PaymentType.ALIPAY);
            JObject headParam = CommonUtils.GetCommonSystem();
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/order/pay";
            JObject result = RequestUtils.HttpGetRequest(url, headParam, param);
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

        public JObject testOrderPayQR()
        {
            string oid = "f2019073011321849782193";
            JObject param = new JObject();
            param.Add("orderId", oid);
            param.Add("paymentType", (int)Utils.PaymentType.ALIPAY);
            JObject headParam = CommonUtils.GetCommonSystem();
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/order/pay/qr";
            JObject result = RequestUtils.HttpGetRequest(url, headParam, param);
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
    }

    class Utils
    {
        public enum PaymentType
        {
            ALIPAY = 1,
            WECHATPAY = 2,
            WALLETPAY = 3
        }
    }
}
