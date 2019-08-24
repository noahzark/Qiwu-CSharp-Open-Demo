using CShapeDemo.account_center;
using CShapeDemo.account_center.accountTest;
using CShapeDemo.account_center.orderTest;
using CShapeDemo.account_center.paymentTest;
using CShapeDemo.account_center.tokenTest;
using CShapeDemo.account_center.walletTest;
using CShapeDemo.chatApiTest;
using System;

namespace CShapeDemo
{

    class main
    {
        static void Main(string[] args)
        {
            string appkey = CommonUtils.GetDataConfig("appkey");
            string appsecret = CommonUtils.GetDataConfig("appsecret");
            string nickname = CommonUtils.GetDataConfig("nickname");

            ChatApiTest chatApi = new ChatApiTest(appkey, appsecret, nickname);
            Console.WriteLine("------ChatTest-----");
            chatApi.testChatApiFrom("hello!!");
            chatApi.testChatApiJson("hello!!");
            chatApi.testApiChatGeoFrom("113.979399", "22.544891");
            chatApi.testApiChatGeoJson("113.979399", "22.544891");
            chatApi.testApiSpeechChatStream("../../chat-api/test.amr");
            chatApi.testApiSpeechChatData("../../chat-api/test.amr");

            Console.WriteLine("------TokenTest-----");
            string appChannel = CommonUtils.GetDataConfig("appChannel");
            string privateKey = CommonUtils.GetDataConfig("privateKey");

            TokenTest token = new TokenTest();
            Console.WriteLine(token.testGetToken(appChannel,privateKey));

            string apiRefreshToken = CommonUtils.GetDataConfig("apiRefreshToken");
            Console.WriteLine(token.testRefreshToken(apiRefreshToken));

            string apiToken = CommonUtils.GetDataConfig("apiToken");
            Console.WriteLine(token.testDeleteToken(apiToken));

            token.testGetToken(appChannel, privateKey);   //重新获取
            apiToken = CommonUtils.GetDataConfig("apiToken");
            apiRefreshToken = CommonUtils.GetDataConfig("apiRefreshToken");

            Console.WriteLine("------AccountTest-----");
            AccountTest account = new AccountTest(apiToken);

            string testPhone = CommonUtils.GetDataConfig("testPhone");
            Console.WriteLine(account.testCaptchaSms(testPhone));

            string defaultPhone = CommonUtils.GetDataConfig("defaultPhone");
            string defaultSms = CommonUtils.GetDataConfig("defaultSms");
            Console.WriteLine(account.testUserSignIn(defaultPhone, defaultSms));

            string userRefreshToken = CommonUtils.GetDataConfig("userRefreshToken");
            Console.WriteLine(account.testRefreshToken(userRefreshToken));

            string userToken = CommonUtils.GetDataConfig("userToken");
            Console.WriteLine(account.testUserSignOut(userToken));

            account.testUserSignIn(defaultPhone, defaultSms);   //重新登录
            userToken = CommonUtils.GetDataConfig("userToken");

            Console.WriteLine(account.testUserInfo(userToken));

            string businessId = CommonUtils.GetDataConfig("businessId");
            Console.WriteLine(account.testAddSubAccount(userToken, businessId));

            Console.WriteLine("------OrderTest-----");
            OrderTest order = new OrderTest(apiToken, userToken);

            string orderId = CommonUtils.GetDataConfig("orderId");
            Console.WriteLine(order.testGetOneOrder(orderId));

            int state = (int)Utils.StateType.CHUPIAOZHONG;
            string lastId = CommonUtils.GetDataConfig("order_Id");
            int pageSize = 30;
            int orderType = (int)Utils.OrderType.FLIGHT;
            Console.WriteLine(order.testGetOrderList(state.ToString(), lastId, pageSize.ToString(), orderType.ToString()));
            Console.WriteLine(order.testDeleteOneOrder(orderId));

            Console.WriteLine("------PaymentTest-----");
            PaymentTest payment = new PaymentTest(apiToken, userToken);
            int paymentType = (int)Utils.PaymentType.ALIPAY;
            Console.WriteLine(payment.testOrderPay(orderId, paymentType));
            Console.WriteLine(payment.testOrderPayQR(orderId, paymentType));

            Console.WriteLine("------WalletTest-----");
            WalletTest Wallet = new WalletTest(apiToken, userToken);
            Console.WriteLine(Wallet.testGetBalance());

            string amount = "1";
            string payType = Utils.PayType.ALI_PAY.ToString();
            string mode = Utils.PaymentMode.APP.ToString();
            Console.WriteLine(Wallet.testWalletRecharge(amount, payType, mode));
            Console.ReadKey();

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
            FLIGHT = 0,   //机票
            FLIGHT_RETURN = 1,//机票返程
            TRAIN = 2,  //火车
            HOTEL = 3,   //酒店
            TAXI_ORDER = 4,
            PHONE_CHARGE = 5, //充话费
            PAIR = 6, //恋爱运势
            FORTUN = 7,   //月运势
            EXPRESS = 8, //闪送
            MOVIE = 9   //电影
        }

        public enum PaymentType
        {
            ALIPAY = 1,
            WECHATPAY = 2,
            WALLETPAY = 3
        }

        public enum PayType
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
