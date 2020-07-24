using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void  Button_ClickAsync(object sender, RoutedEventArgs e)
        {

           
            

            btn.IsEnabled = true;
            //千峰视频页地址
            var url = urlText.Text;


            if (!string.IsNullOrWhiteSpace(url)) 
            {

                //await DownloadFile(url, "D:/" +Path.GetFileName(url));
                var reData = await Http(url, "get", "");

                var dirList = await GetDirList(reData);


                foreach (var items in dirList)
                {
                    string dirPath = Path.Combine("D:/qianfeng/", items.Value);
                    if (!Directory.Exists(dirPath))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
                        directoryInfo.Create();
                    }

                    var videoUrl = "http://" + new Uri(url).Host + items.Key;

                    var videoData = await Http(videoUrl, "get", "");

                    var downloadUrlList = await GetUrl(videoData);



                    foreach (var item in downloadUrlList)
                    {

                        //该行取消注释后，可下载

                        //await DownloadFile(item, "D:/" + items.Value + Path.GetFileName(item));

                    }
                }

            }
            else
            {
                MessageBox.Show("请输入Url");
            }
            





        }



        public async Task<Dictionary<string, string>> GetDirList(string data)
        {

            var text = "<li class=\"fl\">(.|\n)*?</a>";
            var reg = new Regex(text);
            var result = reg.Matches(data);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (Match items in result)
            {

                var regHref = "href=\"(.|\n)*?\"";

                var urlreg = new Regex(regHref);
                var urlResult = urlreg.Matches(items.Value);

                var hrefItem = urlResult[0];

                var href = hrefItem.Value.Replace("href=", "").Trim('"');

                var regTitle = "title=\"(.|\n)*?\"";

                var titleReg = new Regex(regTitle);
                var titleResult = titleReg.Matches(items.Value);

                var titleItem = titleResult[0];

                var title = titleItem.Value.Replace("title=", "").Trim('"');

                dic.Add(href, title);
            }


            return dic;

        }





        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<string>> GetUrl(string data) 
        {

            var text = "<li class=\"clearfix\">(.|\n)*?</ul>";
            var reg = new Regex(text);
            var result = reg.Matches(data);
            List<string> list = new List<string>();
            foreach (Match items in result)
            {

                var regText = "data-url=\"(.|\n)*?\"";
                var urlreg = new Regex(regText);
                var urlResult = urlreg.Matches(items.Value);
                foreach(Match item in urlResult) 
                {

                    var urlText = item.Value.Replace("data-url=", "").Trim('"');

                    list.Add(urlText);
                    
                }

                
            }


            return list;

        }






        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="serverFilePath"></param>
        /// <param name="targetPath"></param>
        public async Task  DownloadFile(string serverFilePath, string targetPath)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverFilePath);
            
            WebResponse respone = request.GetResponse();
            var size = respone.ContentLength;
            long taotal = 0;
            Stream netStream = respone.GetResponseStream();
            using (Stream fileStream = new FileStream(targetPath, FileMode.Create))
            {
                byte[] read = new byte[1024];
                int realReadLen = netStream.Read(read, 0, read.Length);
                while (realReadLen > 0)
                {
                    taotal += realReadLen;
                    var jindu1 = taotal / size * 100;
                    bar.Value = jindu1;
                    jindu.Content = jindu1+"%";
                    fileStream.Write(read, 0, realReadLen);
                    realReadLen = await netStream.ReadAsync(read, 0, read.Length);
                }
                netStream.Close();
                fileStream.Close();
            }
        }




        /// <summary>
        /// 发送http请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> Http(string url, string method, string data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = string.IsNullOrEmpty(method) ? "GET" : method;
            request.ContentType = "application/json;charset=utf-8";
            if (!string.IsNullOrEmpty(data))
            {
                Stream RequestStream = await request.GetRequestStreamAsync();
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                await RequestStream.WriteAsync(bytes, 0, bytes.Length);
                RequestStream.Close();
            }
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            Stream ResponseStream = response.GetResponseStream();
            StreamReader StreamReader = new StreamReader(ResponseStream, Encoding.GetEncoding("utf-8"));
            string re = await StreamReader.ReadToEndAsync();
            StreamReader.Close();
            ResponseStream.Close();
            return re;
        }

    }
}
