using CShapeDemo.account_center.accountTest;
using CShapeDemo.account_center.tokenTest;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CShapeDemo.account_center
{
    class CommonUtils
    {
        public static string GetQueryStringCipher(JObject query)
        {
            if (query != null)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in query)
                {
                    if (stringBuilder.Length > 0)
                        stringBuilder.Append("&");
                    stringBuilder.Append(item.Key + "=" + item.Value);
                }
                string queryString = "?q=" + HttpUtility.UrlEncode(EncodeAES(stringBuilder.ToString()));    //get请求才需要encode
                return queryString;
            }
            return null;
        }

        public static string GetBodyStringCipher(JObject query)
        {
            if (query != null)
            {
                string bodyString = query.ToString();
                return EncodeAES(bodyString);
            }
            return null;
        }

        public static JObject GetCommonSystem()
        {
            JObject allTokens = GetAllTokens();
            if (allTokens == null)
            {
                Console.WriteLine("获取token失败！");
                return null;
            }
            JObject param = new JObject();
            param.Add("Access-Token", allTokens.Value<string>("apiToken"));
            param.Add("Authorization", "Bearer " + allTokens.Value<string>("userToken"));
            param.Add("Timestamp", GetTimestamp());
            return param;
        }


        public static string GetApiToken()
        {
            TokenTest tokenTest = new TokenTest();
            JObject tokens = tokenTest.testGetToken();
            if (tokens == null) return null;
            string apiToken = tokens.Value<string>("accessToken");
            return apiToken;

        }

        public static JObject GetAllTokens()
        {
            AccountTest test = new AccountTest();
            JObject result = test.testUserSignIn();
            try
            {
                string userToken = result.Value<JObject>("payload").Value<string>("accessToken");
                string apiToken = result.Value<string>("token");
                JObject jObject = new JObject();
                jObject.Add("apiToken", apiToken);
                jObject.Add("userToken", userToken);
                return jObject;
            }
            catch
            {
                Console.WriteLine("获取token失败");
                return null;
            }
        }

        public static string GetUserRefreshToken()
        {
            AccountTest test = new AccountTest();
            JObject result = test.testUserSignIn();
            try
            {
                JObject payload = result.Value<JObject>("payload");
                string refreshToken = payload.Value<string>("refreshToken");
                return refreshToken;
            }
            catch
            {
                Console.WriteLine("获取refreshToken失败");
                return null;
            }
        }

        /* AES加密 */
        public static string EncodeAES(string text)
        {
            if (text == null || text.Length == 0) return text;

            string key = GetFromConfig("aesKey");
            string iv = GetFromConfig("aesIv");
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 256;   //key->base64 32位
            rijndaelCipher.BlockSize = 128; //iv->base64 16位
            byte[] pwdBytes = Convert.FromBase64String(key);
            byte[] keyBytes = new byte[32];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
                len = keyBytes.Length;
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = Convert.FromBase64String(iv);
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(text);
            byte[] cipherBytes = transform.TransformFinalBlock(plainText, 0, plainText.Length);
            return Convert.ToBase64String(cipherBytes);

        }

        /* AES解密 */
        public static string DecodeAES(string text)
        {
            if (text == null || text.Length == 0) return text;

            string key = GetFromConfig("aesKey");
            string iv = GetFromConfig("aesIv");
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 256;   //key->base64 32位
            rijndaelCipher.BlockSize = 128; //iv->base64 16位
            byte[] pwdBytes = Convert.FromBase64String(key);
            byte[] keyBytes = new byte[32];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
                len = keyBytes.Length;
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = Convert.FromBase64String(iv);
            ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
            byte[] encryptedData = Convert.FromBase64String(text);
            byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return Encoding.UTF8.GetString(plainText);
        }

        public static string GetFromConfig(string key)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return config.AppSettings.Settings[key].Value;
        }

        public static string GetTimestamp()
        {
            TimeSpan time = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(time.TotalMilliseconds).ToString();
        }
    }
}
