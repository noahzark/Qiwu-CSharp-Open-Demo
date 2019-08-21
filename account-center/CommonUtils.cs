using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatApiTest.account_center
{
    class CommonUtils
    {
        public static string DecodeAES(string text)
        {
            if (text == null) return text;

            string key = Utils.GetFromConfig("aesKey");
            string iv = Utils.GetFromConfig("aesIv");
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 256;   //key->base64 32位
            rijndaelCipher.BlockSize = 128; //iv->base64 16位
            byte[] encryptedData = Convert.FromBase64String(text);
            byte[] pwdBytes = Convert.FromBase64String(key);
            byte[] keyBytes = new byte[32];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
                len = keyBytes.Length;
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = Convert.FromBase64String(iv);
            ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
            byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return Encoding.UTF8.GetString(plainText);
        }

        public static string GetQueryString(JObject param)
        {
            StringBuilder stringBuilder = new StringBuilder("?");
            foreach (var item in param)
            {
                if (stringBuilder.Length > 1)
                    stringBuilder.Append("&");
                stringBuilder.Append(item.Key + "=" + item.Value);
            }
            return stringBuilder.ToString();
        }

        public static string GetFromConfig(string key)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return config.AppSettings.Settings[key].Value;
        }

        public static string GetTimestamp(DateTime now)
        {
            TimeSpan time = now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(time.TotalMilliseconds).ToString();
        }
    }
}
