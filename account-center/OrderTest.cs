using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace CShapeDemo.account_center.orderTest
{
    class OrderTest
    {
        private string apiToken;
        private string userToken;

        public OrderTest(string apiToken, string userToken)
        {
            this.apiToken = apiToken;
            this.userToken = userToken;
        }

        /* 获取订单信息 */
        public JObject testGetOneOrder(string orderId)
        {
            JObject headParam = CommonUtils.GetCommonSystem(apiToken, userToken);
            JObject param = new JObject { { "orderId", orderId } };
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/order";
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

        /* 获取账号下订单 */
        public JObject testGetOrderList(string state, string lastId, string pageSize, string orderType)
        {
            string id = CommonUtils.GetDataConfig("order_id");
            JObject headParam = CommonUtils.GetCommonSystem(apiToken, userToken);
            JObject param = new JObject();
            param.Add("state", state);
            param.Add("lastId", lastId);
            param.Add("pageSize", pageSize);
            param.Add("orderType", orderType);
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/orders";
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

        /* 取消订单 */
        public JObject testDeleteOneOrder(string orderId)
        {
            JObject headParam = CommonUtils.GetCommonSystem(apiToken, userToken);
            JObject param = new JObject { { "orderId", orderId } };
            string url = CommonUtils.GetAppConfig("account-host") + "/api/open/v1/order";
            JObject result = RequestUtils.HttpDeleteRequest(url, headParam, param);
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
