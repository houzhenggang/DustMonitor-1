using System;
using System.Collections.Generic;
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
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace DustMonitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Dictionary<string, string> queueDic = new Dictionary<string, string>();

        //public static readonly object lockobj = new object();
        //public static readonly object locksend = new object();
        public MainWindow()
        {
            //queueDic.Add("2013-10-31T22:22:22","123.2");
            InitializeComponent();
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            this.DeviceID.Text = "5714";
            this.SensorID.Text = "8827";
            this.APIKey.Text = "e29724be2907878edcfd99416bb438c7";
            this.COMSelect.Items.Add("com1");
            this.COMSelect.Items.Add("com2");
            this.COMSelect.Items.Add("com3");
            this.COMSelect.Items.Add("com4");
            this.COMSelect.Items.Add("com5");
            this.COMSelect.SelectedItem = "com5";

        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //ComOperation com = new ComOperation("com5");
            //ThreadPool.QueueUserWorkItem(new WaitCallback(ReadPort), new object());
            //ThreadPool.QueueUserWorkItem(new WaitCallback(PostToYeelink), new object());
            //MessageBox.Show(DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss"));
            System.Threading.Timer timer = new Timer(new TimerCallback(PostToYeelink), null, 10000, 10000);
            Thread read = new Thread(new ParameterizedThreadStart(ReadPort));
           // Thread post = new Thread(new ParameterizedThreadStart(PostToYeelink));
            read.Start(new object());
           // post.Start(new object());
            this.StartButton.IsEnabled = false;
        }

        public void ReadPort(object parameters)
        {

            try
            {
                string result = string.Empty;
                System.IO.Ports.SerialPort currentPort = new System.IO.Ports.SerialPort("com2");

                byte[] buffer = new byte[128];
                Regex reg = new Regex(@"{""density"":""[-]?\d{1,4}(\.\d{0,3})?""}", RegexOptions.Compiled);
                Regex regFloat = new Regex(@"[-]?\d{1,4}(\.\d{0,3})?");
                currentPort.Parity = System.IO.Ports.Parity.None;
                currentPort.BaudRate = 9600;
                currentPort.DataBits = 8;
                currentPort.StopBits = System.IO.Ports.StopBits.One;
                currentPort.Open();

                if (!currentPort.IsOpen)
                {
                    MessageBox.Show("请打开" + "com5" + "端口");
                }
                while (true)
                {
                    //lock (queueDic)
                    //{
                        currentPort.Read(buffer, 0, buffer.Length);
                        if (buffer.Length > 64)
                        {
                            result = System.Text.ASCIIEncoding.Default.GetString(buffer);
                            MatchCollection matchs = reg.Matches(result);
                            foreach (Match match in matchs)
                            {
                                queueDic.Add(DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss") + System.Guid.NewGuid().ToString(), regFloat.Match(match.Value).Value);
                            }
                            //Monitor.Pulse(queueDic);
                            //Monitor.Wait(queueDic);
                            //MessageBox.Show(queueDic.Last().Value);

                        }
                    //}

                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }
        public void PostToYeelink(object parameters)
        {


            while (true)
            {
                if (queueDic.Count > 0)
                {
                    string apiKey = "e29724be2907878edcfd99416bb438c7";
                    string deviceID = "5714";
                    string sensorID = "8827";
                    string timeStamp, currentValue, json, status = string.Empty;
                    byte[] buffer=new byte[128];
                    lock (queueDic)
                    {
                        try
                        {
                            timeStamp = queueDic.Last().Key.Substring(0, 19);
                            currentValue = queueDic.Last().Value;
                            json = @"{""timestamp"":""" + timeStamp + @""",""value"":" + currentValue + "}";
                            buffer = ASCIIEncoding.ASCII.GetBytes(json);

                        }
                        catch (Exception ex)
                        {

                        }
                        finally
                        {
                            //Monitor.Pulse(queueDic);
                            //Monitor.Wait(queueDic);
                        }
                    }
                    //Monitor.Pulse(queueDic);
                    //Monitor.Wait(queueDic);


                    GC.Collect();
                    HttpWebRequest request = WebRequest.Create("http://api.yeelink.net/v1.0/device/" + deviceID + "/sensor/" + sensorID + "/datapoints") as HttpWebRequest;
                    request.Proxy = null;
                    request.Method = "POST";
                    request.Host = "api.yeelink.net";
                    request.ContentLength = buffer.Length;
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Headers.Add("U-ApiKey", apiKey);
                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.101 Safari/537.36";
                    request.Accept = "*/*";
                    request.KeepAlive = false;
                    request.Timeout = 20000;
                    Stream requestStrm = request.GetRequestStream();
                    requestStrm.Write(buffer, 0, buffer.Length);
                    requestStrm.Close();

                    queueDic.Clear();

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    status = response.StatusCode.ToString();

                    if (request != null) request.Abort();
                    response.Close();

                    MessageBox.Show(status);
                }
            }
        }

        private void APIKey_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            DustMonitor.About aboutWindow = new About();
            //aboutWindow.ShowsNavigationUI = true;
            //aboutWindow
            //aboutWindow.NavigationService

        }
    }
    //public class ComOperation {
    //    public string com
    //    {
    //        get;
    //        set;
    //    }
    //    public ComOperation(string com) {
    //        this.com = com;
    //    }
    //    public string ReadProt() {
    //        string result = string.Empty;
    //        System.IO.Ports.SerialPort currentPort = new System.IO.Ports.SerialPort(this.com);
    //        byte[] buffer=new byte[128];
    //        Regex reg = new Regex(@"{""density"":""[-]?\d{1,4}\.""");
    //        currentPort.Parity = System.IO.Ports.Parity.None;
    //        currentPort.BaudRate = 9600;
    //        currentPort.DataBits = 8;
    //        currentPort.StopBits = System.IO.Ports.StopBits.One;
    //        currentPort.Open();
    //        if (!currentPort.IsOpen)
    //        {
    //            MessageBox.Show("请打开"+this.com+"端口");
    //        }
    //        while (true)
    //        {
    //            currentPort.Read(buffer, 0, buffer.Length);
    //            if (buffer.Length>90)
    //            {
    //                result = System.Text.ASCIIEncoding.Default.GetString(buffer);


    //            }
    //            //MessageBox.Show(System.Text.ASCIIEncoding.Default.GetString(buffer));
    //        }
    //        return result;
    //    }
    //}
}
