using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;

namespace ShippingRequest
{
	public class RequestHelper
	{
        public string HttpGet(string url, Encoding encode = null, Dictionary<string, string> dicHeader = null)
        {
            string retStr = string.Empty;
            encode = encode ?? Encoding.UTF8;
            try
            {
                HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(UrlEncode(url, encode));
                hwr.Method = "GET";
                hwr.ContentType = "application/x-www-form-urlencoded";
                hwr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.67 Safari/537.36";
                hwr.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                hwr.KeepAlive = false;
                hwr.Headers.Add("Accept-Encoding", "gzip, deflate");
                //hwr.Headers.Add("Accept-Encoding", "gzip, deflate");
                hwr.AutomaticDecompression = DecompressionMethods.GZip;

                if (dicHeader != null)
                {
                    foreach (var kv in dicHeader)
                    {
                        switch (kv.Key)
                        {
                            //部分请求头不能像这样hwr.Headers[kv.Key] = kv.Value，需要的自行写
                            case "ContentType": hwr.ContentType = kv.Value; break;
                            case "Accept": hwr.Accept = kv.Value; break;
                            case "UserAgent": hwr.UserAgent = kv.Value; break;
                            case "Referer": hwr.Referer = UrlEncode(kv.Value, encode); break;
                            default: hwr.Headers[kv.Key] = kv.Value; break;
                        }
                    }
                }

                //下载页面
                HttpWebResponse httpresponse = (HttpWebResponse)hwr.GetResponse();
                Stream stream = httpresponse.GetResponseStream();
                var isGzip = httpresponse.ContentEncoding.ToLower().Trim() == "gzip";
                if (isGzip)
                {
                    retStr = GzipDecompress(stream);
                }
                else
                {
                    retStr = StreamToString(stream, encode, isGzip);
                }
                httpresponse.Close();
                hwr.Abort();//重要，不然会占用TCP链接资源
            }
            catch (WebException ex)
            {
                //日志自行实现
                //ex.Status;  
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    //Console.WriteLine("HTTP Status Code: " + (int)response.StatusCode);
                    //string errResopnse = StreamToString(response.GetResponseStream(), encode, response.ContentEncoding.ToLower().Trim() == "gzip");
                }
            }
            catch (Exception ex)
            {
                //日志自行实现
            }
            finally
            {
                GC.Collect();
            }

            return retStr;
        }

        /// <summary>
        /// 发起Post请求，并接收响应体为字符串
        /// </summary>
        /// <param name="url">网址</param>
        /// <param name="postBody">请求体，目前只能传字符串、byte[]、Stream</param>
        /// <param name="encode">编码格式</param>
        /// <param name="dicHeader">请求头</param>
        /// <returns></returns>
        public string HttpPost(string url, object postBody, Encoding encode = null, Dictionary<string, string> dicHeader = null)
        {
            string retStr = string.Empty;
            encode = encode ?? Encoding.UTF8;
            try
            {
                HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(UrlEncode(url, encode));
                hwr.Method = "POST";
                hwr.ContentType = "application/x-www-form-urlencoded";
                hwr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.67 Safari/537.36";
                hwr.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                hwr.KeepAlive = false;
                hwr.Headers.Add("Accept-Encoding", "gzip, deflate");
                if (dicHeader != null)
                {
                    foreach (var kv in dicHeader)
                    {
                        switch (kv.Key)
                        {
                            //部分请求头不能像这样hwr.Headers[kv.Key] = kv.Value，需要的自行写
                            case "ContentType": hwr.ContentType = kv.Value; break;
                            case "Accept": hwr.Accept = kv.Value; break;
                            case "UserAgent": hwr.UserAgent = kv.Value; break;
                            case "Referer": hwr.Referer = UrlEncode(kv.Value, encode); break;
                            default: hwr.Headers[kv.Key] = kv.Value; break;
                        }
                    }
                }

                Stream postDataStream = hwr.GetRequestStream();
                //处理请求体为byte[]
                byte[] postBytes = null;
                if (postBody != null)
                {
                    var tp = postBody.GetType();
                    if (tp == typeof(string))
                    {
                        encode.GetBytes(postBody + "");
                    }
                    else if (tp == typeof(byte[]))
                    {
                        postBytes = postBody as byte[];
                    }
                    else if (postBody is Stream)
                    {
                        using (Stream sm = postBody as Stream)
                        {
                            postBytes = new byte[sm.Length];
                            sm.Position = 0;
                            sm.Read(postBytes, 0, Convert.ToInt32(sm.Length));
                        }
                    }
                    else
                    {
                        throw new Exception("不支持的请求体类型");
                    }
                }

                //发送请求体
                if (postBytes != null)
                {
                    hwr.ContentLength = postBytes.Length;
                    postDataStream.Write(postBytes, 0, postBytes.Length);
                }

                //下载页面
                HttpWebResponse httpresponse = (HttpWebResponse)hwr.GetResponse();

                retStr = StreamToString(httpresponse.GetResponseStream(), encode, httpresponse.ContentEncoding.ToLower().Trim() == "gzip");
                httpresponse.Close();
                hwr.Abort();//重要，不然会占用TCP链接资源
            }
            catch (WebException ex)
            {
                //日志自行实现
                //ex.Status;  
                var response = ex.Response as System.Net.HttpWebResponse;
                if (response != null)
                {
                    //Console.WriteLine("HTTP Status Code: " + (int)response.StatusCode);
                    //string errResopnse = StreamToString(response.GetResponseStream(), encode, response.ContentEncoding.ToLower().Trim() == "gzip");
                }
            }
            catch (Exception ex)
            {
                //日志自行实现
            }
            finally
            {
                GC.Collect();
            }

            return retStr;
        }

        /// <summary>
        /// 读取流为字符串
        /// </summary>
        /// <param name="ream">注意，该流会被释放</param>
        /// <param name="encode"></param>
        /// <param name="isGzip"></param>
        /// <returns></returns>
        public string StreamToString(Stream ream, Encoding encode, bool isGzip = false)
        {
            string retStr = null;
            if (ream != null)
            {
                List<byte> body = new List<byte>();
                int b = -1;
                byte[] bts = new byte[102400];
                while ((b = ream.Read(bts, 0, bts.Length)) > 0) body.AddRange(bts.Take(b));
                ream.Close(); ream.Dispose();
                ream = new MemoryStream(body.ToArray());
                if (isGzip)
                {
                    ream = new GZipStream(ream, CompressionMode.Decompress);
                }
                StreamReader sr = new StreamReader(ream, encode);
                retStr = sr.ReadToEnd();
                sr.Close(); sr.Dispose(); ream.Close(); ream.Close();
            }
            return retStr;
        }


        public string GzipDecompress(Stream responseStream)
        {
            MemoryStream msTemp = new MemoryStream();
            GZipStream gzs = new GZipStream(responseStream, CompressionMode.Decompress);
            byte[] buf = new byte[1024];
            int len;
            while ((len = gzs.Read(buf, 0, buf.Length)) > 0)
            {
                msTemp.Write(buf, 0, len);
            }
            msTemp.Position = 0;
            responseStream = msTemp;

            StreamReader sr = new StreamReader(responseStream);

            string retToEnd = sr.ReadToEnd();

            return retToEnd;
        }

        /// <summary>
        /// 仅对字符大于255的进行编码
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public string UrlEncode(string url, Encoding encode = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return url;
            encode = encode ?? Encoding.UTF8;
            StringBuilder sb = new StringBuilder();
            foreach (var c in url)
            {
                if (c <= 255) sb.Append(c);
                else sb.Append(System.Web.HttpUtility.UrlEncode("" + c, encode));
            }
            return sb.ToString();
        }
    }
}


