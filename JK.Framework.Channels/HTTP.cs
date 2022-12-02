using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace JK.Framework.Channels
{
    public delegate void PostDelegate(int progress, ref bool cancel);//代理
    public class HttpClient
    {
        public static string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            Dictionary<string, long> macPlusSpeed = new Dictionary<string, long>();
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    System.Diagnostics.Debug.WriteLine("Found MAC Address: " + nic.GetPhysicalAddress() + " Type: " + nic.NetworkInterfaceType);

                    string tempMac = nic.GetPhysicalAddress().ToString();

                    if (!string.IsNullOrEmpty(tempMac) && tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                        macPlusSpeed.Add(tempMac, nic.Speed);
                }

                macAddress = macPlusSpeed.OrderByDescending(row => row.Value).ThenBy(row => row.Key).FirstOrDefault().Key;
            }
            catch { }

            System.Diagnostics.Debug.WriteLine("Fastest MAC address: " + macAddress);

            return macAddress;
        }

        /// <summary>
        /// 获取mac地址
        /// </summary>
        /// <returns></returns>
        public static string GetMacByNetworkInterface()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    return BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes());
                }
            }
            catch (Exception)
            {
            }
            return "00-00-00-00-00-00";
        }

        /// <summary>
        /// htttp协议post请求并获取返回字符串(异步)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static Task<string> HttpPostAsync(string url, string param = null)
        {
            return Task.Run<string>(() =>
            {
                return HttpPost(url, param);
            });
        }

        /// <summary>
        /// http协议get请求并获取返回字符串（异步）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static Task<string> HttpGetAsync(string url, string param = null)
        {
            return Task.Run<string>(() =>
            {
                return HttpGet(url, param);
            });
        }



        public static string HttpGet(string url, string param = null)
        {
            string result = string.Empty;
            try
            {
                string getUrl = url;
                if (!string.IsNullOrEmpty(param))
                    getUrl = string.Format("{0}?{1}", url, param);
                HttpWebRequest hwbRequest = (HttpWebRequest)WebRequest.Create(getUrl);
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }
                hwbRequest.Method = "GET";
                //获取返回值
                HttpWebResponse wbResponse = (HttpWebResponse)hwbRequest.GetResponse();
                using (Stream responseStream = wbResponse.GetResponseStream())
                {
                    using (StreamReader sread = new StreamReader(responseStream))
                    {
                        result = sread.ReadToEnd();
                    }
                }

            }
            catch (Exception ex)
            {
                return "";
            }
            return result;
        }

        /// <summary>
        /// htttp协议post请求并获取返回字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string HttpPost(string url, string param = null)
        {
            //param格式："key1=value1&key2=value2&..."
            string result = string.Empty;
            try
            {
                HttpWebRequest hwbRequest = (HttpWebRequest)WebRequest.Create(url);
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }
                hwbRequest.Method = "POST";
                hwbRequest.ContentType = "application/x-www-form-urlencoded";
                //请求参数不为空则写入
                if (param != null)
                {
                    hwbRequest.ContentLength = Encoding.UTF8.GetByteCount(param);
                    using (Stream requestStream = hwbRequest.GetRequestStream())
                    {
                        using (StreamWriter swrite = new StreamWriter(requestStream))
                        {
                            swrite.Write(param);
                        }
                    }
                }
                //获取返回值
                HttpWebResponse wbResponse = (HttpWebResponse)hwbRequest.GetResponse();
                using (Stream responseStream = wbResponse.GetResponseStream())
                {
                    using (StreamReader sread = new StreamReader(responseStream))
                    {
                        result = sread.ReadToEnd();
                    }
                }
            }
            catch (Exception ex) { throw new CustomException("网络请求失败！"); }
            return result;
        }//end HttpPost



        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;// Always accept
        }
    }



    class CustomException : ApplicationException
    {
        public string functionName { get; set; }
        private string exMode { get; set; }
        public CustomException() { }

        public CustomException(string message) : base(message) { }

        public CustomException(string message, Exception inner) : base(message, inner) { }

        public CustomException(string message, string functionName, string exMode) : base(message)
        {
            this.functionName = functionName;
            this.exMode = exMode;
        }

        public CustomException(string message, string functionName) : base(message)
        {
            this.functionName = functionName;
        }

        public CustomException(string message, string functionName, Exception inner) : base(message, inner)
        {
            this.functionName = functionName;
        }

        public override string Message
        {
            get
            {
                string extension = base.Message;
                if (!string.IsNullOrEmpty(functionName))
                {
                    extension = extension + functionName;
                }

                if (!string.IsNullOrEmpty(exMode))
                {
                    extension = extension + exMode;
                }
                return extension;
            }
        }
    }

    class NetworkException : CustomException
    {
        public NetworkException() { }
        public NetworkException(string message) : base(message) { }

        public NetworkException(string message, Exception inner) : base(message, inner) { }

        public NetworkException(string message, string functionName) : base(message, functionName, "[0x01]") { }
    }

    class InteractionException : CustomException
    {
        public InteractionException() { }
        public InteractionException(string message) : base(message) { }

        public InteractionException(string message, Exception inner) : base(message, inner) { }

        public InteractionException(string message, string functionName) : base(message, functionName, "[0x02]") { }
    }

    class FileException : CustomException
    {

        public FileException() { }
        public FileException(string message) : base(message) { }

        public FileException(string message, Exception inner) : base(message, inner) { }

        public FileException(string message, string functionName) : base(message, functionName, "[0x03]") { }


    }

}
