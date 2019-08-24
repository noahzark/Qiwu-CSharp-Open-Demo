using CShapeDemo.account_center.accountTest;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Net;
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
                string queryString = "?q=" + HttpUtility.UrlEncode(EncodeAES(stringBuilder.ToString()));    //需要encode
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

        public static JObject GetCommonSystem(string apiToken,string userToken)
        {
            JObject param = new JObject();
            param.Add("Access-Token", apiToken);
            param.Add("Authorization", "Bearer " + userToken);
            param.Add("Timestamp", GetTimestamp());
            return param;
        }

        /* AES加密 */
        public static string EncodeAES(string text)
        {
            if (text == null || text.Length == 0) return text;

            string key = GetDataConfig("aesKey");
            string iv = GetDataConfig("aesIv");
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

            string key = GetDataConfig("aesKey");
            string iv = GetDataConfig("aesIv");
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

        public static string GetAppConfig(string key)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return config.AppSettings.Settings[key].Value;
        }

        public static void WriteDataConfig(string key, string value)
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap() { ExeConfigFilename = "../../TestData.Config" };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, value);
            }
            else
            {
                config.AppSettings.Settings[key].Value = value;
            }
            config.Save();
        }

        public static void DeleteDataConfig(string key)
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap() { ExeConfigFilename = "../../TestData.Config" };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(key);
            config.Save();
        }

        public static string GetDataConfig(string key)
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap() { ExeConfigFilename = "../../TestData.Config" };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            try
            {
                return config.AppSettings.Settings[key].Value;
            }
            catch
            {
                Console.WriteLine("Data中" + key + "不存在");
                return null;
            }
        }

        public static string GetTimestamp()
        {
            TimeSpan time = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(time.TotalMilliseconds).ToString();
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
}
