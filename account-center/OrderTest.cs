using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace CShapeDemo.account_center.orderTest
{
    class OrderTest
    {

        public void main(string[] args)
        {
            OrderTest test = new OrderTest();
            Console.WriteLine(test.testGetOneOrder());
            Console.WriteLine(test.testGetOrderList());
            Console.WriteLine(test.testDeleteOneOrder());
            Console.ReadKey();
        }

        public JObject testGetOneOrder()
        {
            string oid = "f2019082214255679014155";
            JObject headParam = CommonUtils.GetCommonSystem();
            JObject param = new JObject { { "orderId", oid } };
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/order";
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

        public JObject testGetOrderList()
        {
            string id = "5d5e3574805fdf0001b16e7c";
            JObject headParam = CommonUtils.GetCommonSystem();
            JObject param = new JObject();
            param.Add("state", (int)Utils.StateType.CHUPIAOZHONG);
            param.Add("lastId", id);
            param.Add("pageSize", 30);
            param.Add("orderType", (int)Utils.OrderType.FLIGHT);
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/orders";
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

        public JObject testDeleteOneOrder()
        {
            string oid = "f2019073011383932530193";
            JObject headParam = CommonUtils.GetCommonSystem();
            JObject param = new JObject { { "orderId", oid } };
            string url = CommonUtils.GetFromConfig("account-host") + "/api/open/v1/order";
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

        public static JObject HttpDeleteRequest(string url, JObject headParam, JObject param)
        {
            url += CommonUtils.GetQueryStringCipher(param);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "DELETE";
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
        public enum StateType
        {
            CHUPIAOZHONG = 1,//出票中
            TUIKUANZHONG = 5,//退款中
            DAIZHIFU = 0,//待支付
            DAICHUXING = 2,//待出行
            CHUPIAOSHIBAI = 4,//出票失败
            YIQUXIAO = 7, //已取消
            YITUIKUAN = 6, //已退款
            YICHUXING = 3, //已出行
            JIAOYIGUANBI = 8, //交易关闭
            GAIQIANZHONG = 9, //改签中
            ZHANZUOZHONG = 10, //占座中
            QUXIAOZHONG = 11, //取消中
            TUIPIAOZHONG = 12, //退票中
            CHUPIAOSHIBAITUIKUANZHONG = 13, //出票失败退款中
            TUIKUANSHIBAI = 14, //退款失败
            SIJIDAODA = 15, //司机已到达
            CHAOSHIQUXIAO = 16 //超时取消
        }


        public enum OrderType
        {
            FLIGHT=0,   //机票
            FLIGHT_RETURN = 1,//机票返程
            TRAIN = 2,  //火车
            HOTEL =3,   //酒店
            TAXI_ORDER = 4,
            PHONE_CHARGE=5, //充话费
            PAIR= 6 , //恋爱运势
            FORTUN=7,   //月运势
            EXPRESS= 8, //闪送
            MOVIE=9   //电影
        }

    }
}
