using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using Newtonsoft.Json;
using ShippingAuto.Model;
using ShippingRequest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShippingAuto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            this.ExportBtn.IsEnabled = false;
            this.lblMessage.Content = "";
        }

        private string filePath;


        public ObservableCollection<OrderHistory> GetObservable(List<OrderHistory> orderHistory)
        {
            var result = new ObservableCollection<OrderHistory>();
            foreach (var item in orderHistory)
            {
                result.Add(item);
            }
            return result;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                // Open document
                string filename = openFileDialog.FileName;
                txtFileName.Content = filename;

                filePath = filename;

                //load Dataed

                //Read Data
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = args => args.Header.ToLower(),
                };
                using (StreamReader reader = new StreamReader(path: filePath, Encoding.GetEncoding("GB2312")))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<OrderHistoryMap>();
                        var records = csv.GetRecords<OrderHistory>();

                        //Display Data
                        this.orderGrid.ItemsSource = records.ToList();
                    }
                }
            }
        }
        
        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            var data = this.orderGrid.ItemsSource as List<OrderHistory>;

            using (var writer = new StreamWriter(path: this.filePath.Replace(".csv", "") + "_proceessed.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
               
                csv.WriteRecords(data);
            }


            List<ShippingForm> forms = new List<ShippingForm>();
            using (var writer = new StreamWriter(path:this.filePath.Replace(".csv","")+ "_Filled.csv",false, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var item in data)
                {
                    forms.Add(new ShippingForm(item));
                }
                csv.WriteRecords(forms);
            }

            this.lblMessage.Content = "Export Success";
        }

        
        private void ExecuetBtn_Click(object sender, RoutedEventArgs e)
        {  
            this.ExportBtn.IsEnabled = false;
      


            var data = this.orderGrid.ItemsSource as List<OrderHistory>;

            int total = data.Count;

            int count = 0;


            this.lblMessage.Content = string.Format("正在处理{0}/{1}", count,  total);

            foreach (var item in data)
            {
                if (item.OrderPlatform.Trim() == "淘宝")
                {
                    if (!string.IsNullOrEmpty(item.OrderShipmentExpressName)) { continue; }
                    var bizNumber = item.BizNumber.Replace("\t", "");
                    var bizPrefix = "0P";
                    var start = bizNumber.IndexOf(bizPrefix) + bizPrefix.Length;
                    string orderId = bizNumber.Substring(start);
                    var url = ApiUrl(orderId);

                    var encode = Encoding.GetEncoding("GB2312");
                    RequestHelper tool = new RequestHelper();

                    var responseString = tool.HttpGet(url, encode, new Dictionary<string, string>() {
                       {
                            "cookie", this.txtCookie.Text
                        }
                    });

                    try
                    {
                        var result = JsonConvert.DeserializeObject<ExpressResult>(responseString);
                        item.OrderShipmentExpressName = result.ExpressName;
                        item.OrderShipmentExpressId = result.ExpressId;
                        item.ShippingStaus = result.GetShippingStatus();

                        count++;
                        this.lblMessage.Content = string.Format("正在处理{0}/{1}", count, total);
                    }
                    catch (Exception ex)
                    {
                        item.Remark = "出错了!!" + bizNumber;
                    }
                }
            }

            this.lblMessage.Content = "处理完毕";
            
            this.ExecuetBtn.IsEnabled = true;
            this.ExportBtn.IsEnabled = true;
        }

    

        public string ApiUrl(string orderId)
        {
            return $"{this.txtExpressUrl.Text}{orderId}";
        }
    }
}
