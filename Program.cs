using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ChatApiTest
{
    class ChatApiTest
    {
        static void Main(string[] args)
        {
            ChatApiTest test = new ChatApiTest();
            test.testChatApiFrom("hello!!");
            test.testChatApiJson("hello!!");
            test.testApiChatGeoFrom("113.979399", "22.544891");
            test.testApiChatGeoJson("113.979399", "22.544891");
            test.testApiSpeechChatStream("../../test.amr");
            test.testApiSpeechChatData("../../test.amr");
            Console.ReadKey();
        }

        /*  闲聊服务  */
        public void testChatApiFrom(string msg)
        {

            JObject param = RequestUtils.GetSystemParmas();
            param.Add("msg", msg);

            string url = Utils.GetFromConfig("host") + "/api/chat";
            Console.WriteLine(RequestUtils.HttpPostForm(url, param));
        }

        public void testChatApiJson(string msg)
        {

            JObject param = RequestUtils.GetSystemParmas();
            param.Add("msg", msg);

            string url = Utils.GetFromConfig("host") + "/api/chat";
            Console.WriteLine(RequestUtils.HttpPostJson(url, param));
        }

        /*  上报GPS  */
        public void testApiChatGeoFrom(string lng, string lat)
        {
            JObject param = RequestUtils.GetSystemParmas();
            param.Add("geo[lng]", lng);
            param.Add("geo[lat]", lat);

            string url = Utils.GetFromConfig("host") + "/api/chat/geo";
            Console.WriteLine(RequestUtils.HttpPostForm(url, param));
        }

        public void testApiChatGeoJson(string lng, string lat)
        {
            JObject param = RequestUtils.GetSystemParmas();
            param.Add("geo[lng]", lng);
            param.Add("geo[lat]", lat);

            string url = Utils.GetFromConfig("host") + "/api/chat/geo";
            Console.WriteLine(RequestUtils.HttpPostJson(url, param));
        }

        /*   语音交互   */
        public void testApiSpeechChatStream(string file)
        {
            string fileExten = Path.GetExtension(file).Replace(".", "");
            if (fileExten != "amr" && fileExten != "wav" && fileExten != "pcm")
                return;
            JObject param = RequestUtils.GetSystemParmas();
            param.Add("codec", fileExten);
            param.Add("rate", "8000");
            string postStr = "?";
            foreach (var item in param)
            {
                if (postStr != "?")
                    postStr += "&";
                postStr += item.Key + "=" + item.Value;
            }
            string url = Utils.GetFromConfig("host") + "/api/speech/chat" + postStr;
            Console.WriteLine(RequestUtils.HttpPostStream(url, file));
        }

        public void testApiSpeechChatData(string file)
        {
            string fileExten = Path.GetExtension(file).Replace(".", "");
            if (fileExten != "amr" && fileExten != "wav" && fileExten != "pcm")
                return;
            JObject param = RequestUtils.GetSystemParmas();
            param.Add("codec", fileExten);
            param.Add("rate", "8000");
            string postStr = "?";
            foreach (var item in param)
            {
                if (postStr != "?")
                    postStr += "&";
                postStr += item.Key + "=" + item.Value;
            }
            string url = Utils.GetFromConfig("host") + "/api/speech/chat" + postStr;
            Console.WriteLine(RequestUtils.HttpPostFormData(url, file));
        }

    }

    class RequestUtils
    {
        public static JObject GetSystemParmas()
        {
            string appkey = Utils.GetFromConfig("appkey");
            string appsecret = Utils.GetFromConfig("appsecret");
            string nickname = Utils.GetFromConfig("nickname");
            string uid = Utils.GetMacAddress();
            string timestamp = Utils.GetTimestamp(DateTime.UtcNow);
            JObject param = new JObject();
            param.Add("appkey", appkey);
            param.Add("nickname", nickname);
            param.Add("uid", uid);
            param.Add("timestamp", timestamp);
            param.Add("verify", Utils.GetMD5(appsecret + uid + timestamp));
            return param;
        }
        public static string HttpPostForm(string url, JObject param)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            string postStr = "";
            foreach (var item in param)
            {
                if (postStr != "")
                    postStr += "&";
                postStr += item.Key + "=" + item.Value;
            }
            byte[] data = Encoding.UTF8.GetBytes(postStr.ToString());
            request.ContentLength = data.Length;
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream repStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(repStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            repStream.Close();

            JObject result = JObject.Parse(retString);
            return result.ToString();
        }
        public static string HttpPostJson(string url, JObject param)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] data = Encoding.UTF8.GetBytes(param.ToString());
            request.ContentLength = data.Length;
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream repStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(repStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            repStream.Close();

            JObject result = JObject.Parse(retString);
            return result.ToString();
        }
        public static string HttpPostFormData(string url, string filename)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            string boundary = "----WebKitFormBoundary7MA4YWxkTrZu0gW";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            byte[] beginboundary = Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
            byte[] endboundary = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");
            string end = "\r\n";
            byte[] headerbytes = Encoding.UTF8.GetBytes(end);
            string filemsg = "Content-Disposition: form-data; name=\"speech\"; filename=\"" + Path.GetFileName(filename) + "\"\r\n" +
                             "Content-Type: application/octet-stream\r\n\r\n";
            byte[] filemsgs = Encoding.UTF8.GetBytes(filemsg);
            FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            byte[] filedata = new byte[file.Length];
            file.Read(filedata, 0, Convert.ToInt32(file.Length));
            file.Close();
            Stream stream = new MemoryStream();
            stream.Write(beginboundary, 0, beginboundary.Length);
            stream.Write(filemsgs, 0, filemsgs.Length);
            stream.Write(filedata, 0, filedata.Length);
            stream.Write(headerbytes, 0, headerbytes.Length);
            stream.Write(endboundary, 0, endboundary.Length);

            byte[] datas = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(datas, 0, datas.Length);
            request.ContentLength = datas.Length;
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(datas, 0, datas.Length);
            reqStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream repStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(repStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            repStream.Close();

            JObject result = JObject.Parse(retString);
            return result.ToString();
        }

        public static string HttpPostStream(string url, string filename)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/octet-stream";

            FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[file.Length];
            file.Read(data, 0, Convert.ToInt32(file.Length));
            file.Close();

            Stream reqStream = request.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream repStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(repStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            repStream.Close();

            JObject result = JObject.Parse(retString);
            return result.ToString();
        }

    }

    class Utils
    {
        public static string GetMacAddress()
        {
            string strMac = string.Empty;
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        strMac = mo["MacAddress"].ToString();
                    }
                }
                moc = null;
                mc = null;

            }
            catch
            {
                return "unknown";
            }
            while (strMac != strMac.Replace(":", ","))
            {
                strMac = strMac.Replace(":", ",");
            }
            return strMac;
        }

        public static string GetTimestamp(DateTime now)
        {
            TimeSpan time = now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(time.TotalMilliseconds).ToString();
        }


        public static string GetMD5(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] cipher = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            string ciphertext = "";
            foreach (byte s in cipher)
            {
                ciphertext += s.ToString("x2");
            }
            return ciphertext;
        }

        public static string GetFromConfig(string key)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return config.AppSettings.Settings[key].Value;
        }

    }
}
