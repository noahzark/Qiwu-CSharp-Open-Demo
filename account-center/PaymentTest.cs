using Newtonsoft.Json.Linq;
using System;

namespace CShapeDemo.account_center.paymentTest
{
    class PaymentTest
    {
        private string apiToken;
        private string userToken;

        public PaymentTest(string apiToken, string userToken)
        {
            this.apiToken = apiToken;
            this.userToken = userToken;
        }

        /* 订单支付 */
        public JObject testOrderPay(string orderId, int paymentType)
        {
            JObject param = new JObject();
            param.Add("orderId", orderId);
            param.Add("paymentType", paymentType);
            JObject headParam = CommonUtils.GetCommonSystem(apiToken, userToken);
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/order/pay";
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

        /* 获取订单二维码 */
        public JObject testOrderPayQR(string orderId, int paymentType)
        {
            JObject param = new JObject();
            param.Add("orderId", orderId);
            param.Add("paymentType", paymentType);
            JObject headParam = CommonUtils.GetCommonSystem(apiToken, userToken);
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/order/pay/qr";
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

}
