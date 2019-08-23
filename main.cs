using ChatApiTest.account_center.paymentTest;
using ChatApiTest.account_center.walletTest;
using CShapeDemo.account_center.accountTest;
using CShapeDemo.account_center.orderTest;
using CShapeDemo.account_center.tokenTest;
using System;

namespace ChatApiTest
{
    class main
    {
        static void Main(string[] args)
        {
            ChatApiTest chatApi = new ChatApiTest();
            Console.WriteLine("------ChatTest-----");
            chatApi.testChatApiFrom("hello!!");
            chatApi.testChatApiJson("hello!!");
            chatApi.testApiChatGeoFrom("113.979399", "22.544891");
            chatApi.testApiChatGeoJson("113.979399", "22.544891");
            chatApi.testApiSpeechChatStream("../../chat-api/test.amr");
            chatApi.testApiSpeechChatData("../../chat-api/test.amr");

            Console.WriteLine("------TokenTest-----");
            TokenTest token = new TokenTest();
            Console.WriteLine(token.testGetToken());
            Console.WriteLine(token.testRefreshToken());
            Console.WriteLine(token.testDeleteToken());

            Console.WriteLine("------AccountTest-----");
            AccountTest account = new AccountTest();
            Console.WriteLine(account.testCaptchaSms());
            Console.WriteLine(account.testUserSignIn());
            Console.WriteLine(account.testRefreshToken());
            Console.WriteLine(account.testUserSignOut());
            Console.WriteLine(account.testUserInfo());
            Console.WriteLine(account.testAddSubAccount());

            Console.WriteLine("------OrderTest-----");
            OrderTest order = new OrderTest();
            Console.WriteLine(order.testGetOneOrder());
            Console.WriteLine(order.testGetOrderList());
            Console.WriteLine(order.testDeleteOneOrder());

            Console.WriteLine("------PaymentTest-----");
            PaymentTest payment = new PaymentTest();
            Console.WriteLine(payment.testOrderPay());
            Console.WriteLine(payment.testOrderPayQR());

            Console.WriteLine("------WalletTest-----");
            WalletTest Wallet = new WalletTest();
            Console.WriteLine(Wallet.testGetBalance());
            Console.WriteLine(Wallet.testWalletRecharge());
            Console.ReadKey();

        }
    }
}
