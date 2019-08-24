using Newtonsoft.Json.Linq;
using System;
using System.Web;

namespace CShapeDemo.account_center.walletTest
{
    class WalletTest
    {
        private string apiToken;
        private string userToken;

        public WalletTest(string apiToken, string userToken)
        {
            this.apiToken = apiToken;
            this.userToken = userToken;
        }
        public JObject testGetBalance()
        {
            JObject headParam = CommonUtils.GetCommonSystem(apiToken, userToken);
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/balance";
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

        public JObject testWalletRecharge(string amount, string payType, string mode)
        {
            JObject headParam = CommonUtils.GetCommonSystem(apiToken, userToken);
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/balance/topup";
            JObject param = new JObject();
            param.Add("amount", amount);
            param.Add("paymentType", payType);
            param.Add("mode", mode);
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

}
