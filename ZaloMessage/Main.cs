using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZaloMessage
{
    public partial class Main : Form
    {
        Timer tmrProgressBar;
        bool runChuongTrinh = false;
        ChromeDriver chromeDriver = null;
        Actions actions = null;
        public Main()
        {
            InitializeComponent();
            //TopMost = true;

            tmrProgressBar = new Timer();
            tmrProgressBar.Interval = 100;
            tmrProgressBar.Tick += TmrProgressBar_Tick;

            progressBar.Minimum = 0;
            progressBar.Maximum = 200;

            btnMoZalo.Click += BtnMoZalo_Click;
            btnChiaSe.Click += BtnChiaSe_Click;
            btnFile.Click += BtnFile_Click;
            FormClosing += Main_FormClosing;

            SetShareUi(false);
        }

        private void TmrProgressBar_Tick(object sender, EventArgs e)
        {
            tmrProgressBar.Enabled = false;
            if (progressBar.Value == progressBar.Maximum)
            {
                progressBar.Value = progressBar.Minimum;
            }
            else
            {
                progressBar.PerformStep();
            }
            tmrProgressBar.Enabled = true;
        }

        private void BtnFile_Click(object sender, EventArgs e)
        {
            try
            {
                string filePath = Directory.GetCurrentDirectory() + @"/nhom/danhsachnguoidung.txt";
                StringBuilder sb = new StringBuilder();

                //scroll to end
                ScrollToEnd();

                bool timKiemDuoiLenTren = false;
                IWebElement lastItemSearch = null;
                do
                {
                    IReadOnlyCollection<IWebElement> lstUser = chromeDriver.FindElements(By.ClassName(ClassJs.ITEM_MESSAGE));
                    for (int i = lstUser.Count - 1; i >= 0; i--)
                    {
                        IWebElement element = lstUser.ElementAt(i);
                        string text = element.FindElement(By.ClassName(ClassJs.TEN_NGUOI_DUNG)).Text;
                        sb.AppendLine(text);
                    }

                    if (lastItemSearch == null || !lstUser.ElementAt(0).Equals(lastItemSearch))
                    {
                        timKiemDuoiLenTren = true;
                        //Sctroll lên trên
                        lastItemSearch = lstUser.ElementAt(0);
                        actions.ScrollToElement(lastItemSearch).Perform();
                    }
                    else
                    {
                        timKiemDuoiLenTren = false;
                    }
                }
                while (timKiemDuoiLenTren);

                if (sb.ToString().Length > 0)
                {
                    if (File.Exists(filePath)) File.Delete(filePath);
                    File.AppendAllText(filePath, sb.ToString());
                }

                ShowNotification("Xuất thành công");
            }
            catch (Exception ex)
            {
                ShowNotification(ex.Message);
            }
        }

        private void BtnDung_Click(object sender, EventArgs e)
        {
            runChuongTrinh = false;
        }

        private void BtnChiaSe_Click(object sender, EventArgs e)
        {
            if (chromeDriver == null)
            {
                ShowNotification("Bạn chưa khởi tại chrome");
                return;
            }

            SetShareUi(true);

            Task task = new Task(() => {
                try
                {
                    runChuongTrinh = true;

                    Dictionary<string, List<string>> dicFiles = new Dictionary<string, List<string>>();
                    //Lấy danh sách nhóm cần chia sẻ
                    string[] fileNhoms = Directory.GetFiles(Directory.GetCurrentDirectory() + @"/nhom");
                    foreach (string filePath in fileNhoms)
                    {
                        if (!filePath.Contains("quanao.txt") && !filePath.Contains("giaydep.txt") && !filePath.Contains("hangsale.txt")) continue;

                        string noiDungFile = File.ReadAllText(filePath);
                        string[] lines = noiDungFile.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (string item in lines)
                        {
                            if (!dicFiles.ContainsKey(filePath)) dicFiles.Add(filePath, new List<string>() { item });
                            else dicFiles[filePath].Add(item);
                        }
                    }

                    if (dicFiles.Count == 0)
                    {
                        ShowNotification("Bạn chưa cấu hình dữ liệu");
                        runChuongTrinh = false;
                    }

                    while (runChuongTrinh)
                    {
                        ShowNotification("Đang tìm kiếm tin nhắn mới");

                        //scroll to end
                        ScrollToEnd();

                        bool timKiemDuoiLenTren = false;
                        IWebElement lastItemSearch = null;
                        do
                        {
                            //Tìm kiếm tin nhắn mới trên màn hình, khi hết tin nhắn mới thì dừng lại
                            IReadOnlyCollection<IWebElement> lstUser = chromeDriver.FindElements(By.ClassName(ClassJs.ITEM_MESSAGE));
                            for (int i = lstUser.Count - 1; i >= 0; i--)
                            {
                                IWebElement element = lstUser.ElementAt(i);
                                string text = element.FindElement(By.ClassName(ClassJs.TEN_NGUOI_DUNG)).Text;

                                if (text == "TinGoc")
                                {
                                }

                                //Kiểm tra có tin nhắn mới không
                                var lstTemp = element.FindElements(By.ClassName(ClassJs.TIN_NHAN_MOI));
                                if (lstTemp.Count == 0)
                                {
                                    if (i == 0) break;
                                    else continue;
                                }

                                //kiểm tra có phải nhóm đang cần không?
                                if (KiemTraNhom(dicFiles, text))
                                {
                                    ShowNotification("Đang chia sẻ: " + text);

                                    //click vào để chia sẻ
                                    actions.Click(element).Perform();

                                    IWebElement TinNhanMoiPopup = null;
                                    for (int j = 0; j < 20; j++)
                                    {
                                        try
                                        {
                                            runSleep(300);
                                            //Click vào tin nhắn mới
                                            TinNhanMoiPopup = chromeDriver.FindElement(By.ClassName(ClassJs.CHAT_NOTIFY));
                                            if (TinNhanMoiPopup != null)
                                            {
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            TinNhanMoiPopup = null;
                                        }
                                    }

                                    if (TinNhanMoiPopup != null)
                                    {
                                        actions.Click(TinNhanMoiPopup).Perform();
                                        runSleep(5000);
                                    }

                                    bool hasNextMessage = false;
                                    IWebElement lastMessage = null;
                                    //tìm tin nhắn để chia sẻ
                                    do
                                    {
                                        try
                                        {
                                            runSleep(1000);
                                            var lstMessage = chromeDriver.FindElements(By.ClassName(ClassJs.CHAT_MESSAGE));

                                            //chia sẻ tin nhắn
                                            //bool shared = false;
                                            foreach (var messageItem in lstMessage)
                                            {
                                                try
                                                {
                                                    if (!runChuongTrinh) break;
                                                    lastMessage = messageItem;
                                                    //hover
                                                    runSleep(1000);
                                                    actions.ScrollToElement(messageItem).Perform();

                                                    runSleep(1000);
                                                    actions.MoveToElement(messageItem.FindElement(By.ClassName("chat-message__actionholder"))).Perform();
                                                    //tìm biểu tượng chia sẻ

                                                    runSleep(2000);
                                                    IWebElement btnShare = chromeDriver.FindElement(By.CssSelector("[data-id='" + ClassJs.BTN_SHARE + "']"));
                                                    if (btnShare != null)
                                                    {
                                                        //shared = true;

                                                        //click share
                                                        actions.Click(btnShare).Perform();

                                                        //chọn tên chia sẻ
                                                        string temp = LayTenNhomChiaSeDen(text, dicFiles);

                                                        runSleep(1000);
                                                        IWebElement txtSearchUser = chromeDriver.FindElement(By.CssSelector("[data-id='" + ClassJs.TXT_USER_SHARE + "']"));
                                                        txtSearchUser.SendKeys(temp);

                                                        runSleep(2000);
                                                        var lstUserShare = chromeDriver.FindElements(By.ClassName(ClassJs.ITEM_USER_SHARE));

                                                        //thấy thì share, không thấy thì hủy
                                                        bool clickShare = false;
                                                        foreach (IWebElement itemUserShare in lstUserShare)
                                                        {
                                                            if (itemUserShare.Text.Trim().ToLower() == temp.ToLower())
                                                            {
                                                                clickShare = true;
                                                                actions.Click(itemUserShare).Perform();
                                                                break;
                                                            }
                                                        }

                                                        runSleep(1000);
                                                        IWebElement btnTemp = null;
                                                        if (clickShare)
                                                        {
                                                            btnTemp = chromeDriver.FindElement(By.CssSelector("[data-id='" + ClassJs.BTN_CONFIRM_SHARE + "']"));
                                                        }
                                                        else
                                                        {
                                                            btnTemp = chromeDriver.FindElement(By.CssSelector("[data-id='" + ClassJs.BTN_CANCEL_SHARE + "']"));
                                                        }
                                                        actions.Click(btnTemp).Perform();

                                                        runSleep(3000);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                            }

                                            //duyệt tin nhắn tiếp theo
                                            if (lastMessage != null || !lstMessage.ElementAt(lstMessage.Count - 1).Equals(lastMessage))
                                            {
                                                hasNextMessage = true;
                                                actions.ScrollToElement(lastMessage).Perform();
                                            }
                                            else
                                            {
                                                hasNextMessage = false;
                                            }
                                        }
                                        catch (Exception ex1)
                                        {
                                            hasNextMessage = false;
                                        }
                                    }
                                    while (hasNextMessage && runChuongTrinh);
                                }
                            }

                            if (lastItemSearch == null || !lstUser.ElementAt(0).Equals(lastItemSearch))
                            {
                                timKiemDuoiLenTren = true;
                                //Sctroll lên trên
                                lastItemSearch = lstUser.ElementAt(0);
                                actions.ScrollToElement(lastItemSearch).Perform();
                            }
                            else
                            {
                                timKiemDuoiLenTren = false;
                            }
                        }
                        while (timKiemDuoiLenTren && runChuongTrinh);

                        runSleep(10000);
                    }

                    ShowNotification("");
                    Invoke(new MethodInvoker(() => {
                        SetShareUi(false);
                    }));
                }
                catch (Exception ex)
                {
                    ShowNotification(ex.Message);
                }
            });

            task.Start();
        }

        private void SetShareUi(bool run)
        {
            tmrProgressBar.Enabled = run;
            progressBar.Value = 0;
            if (run) lblThongBao.Text = "";

            btnMoZalo.Enabled = chromeDriver == null;
            btnChiaSe.Enabled = !run && !btnMoZalo.Enabled;
            btnFile.Enabled = !run && !btnMoZalo.Enabled;
        }

        private string LayTenNhomChiaSeDen(string text, Dictionary<string, List<string>> dicFiles)
        {
            foreach (string key in dicFiles.Keys)
            {
                foreach (string value in dicFiles[key])
                {
                    if (value.Trim().ToLower() == text.Trim().ToLower())
                    {
                        if (key.EndsWith("giaydep.txt"))
                        {
                            return "ShareDuong";
                        }
                        else if (key.EndsWith("hangsale.txt"))
                        {
                            return "ShareDuong2";
                        }
                        else if (key.EndsWith("quanao.txt"))
                        {
                            return "ShareDuong3";
                        }
                    }
                }
            }
            return "";
        }

        private bool KiemTraNhom(Dictionary<string, List<string>> dicFiles, string text)
        {
            foreach (string key in dicFiles.Keys)
            {
                foreach (string item in dicFiles[key])
                {
                    if (item.Trim().ToLower() == text.Trim().ToLower())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ScrollToEnd()
        {
            try
            {
                bool scroll = false;
                IWebElement lastItem = null;
                do
                {
                    var lstMsg = chromeDriver.FindElements(By.ClassName(ClassJs.ITEM_MESSAGE));
                    IWebElement temp = lstMsg[lstMsg.Count - 1];
                    if (lastItem == null || !temp.Equals(lastItem))
                    {
                        scroll = true;
                        lastItem = temp;
                        //scroll to end
                        actions.ScrollToElement(lastItem).Perform();
                    }
                    else
                    {
                        scroll = false;
                    }
                }
                while (scroll);
            }
            catch (Exception ex)
            {
                ShowNotification(ex.Message);
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (chromeDriver != null)
                {
                    chromeDriver.Quit();
                }
            }
            catch (Exception ex)
            { }
        }

        private void BtnMoZalo_Click(object sender, EventArgs e)
        {
            try
            {
                ShowNotification("Đang mở chrome");

                //service
                ChromeDriverService cService = ChromeDriverService.CreateDefaultService();
                cService.HideCommandPromptWindow = true;
                //options
                ChromeOptions options = new ChromeOptions();
                options.AddArguments(@"--user-data-dir="+ Environment.ExpandEnvironmentVariables("%localappdata%") + @"\Google\Chrome\User Data");
                //options.AddArguments(@"--profile-directory=Default");
                options.AddArguments(@"--profile-directory=Profile 3");

                chromeDriver = new ChromeDriver(cService, options);
                actions = new Actions(chromeDriver);
                chromeDriver.Manage().Window.Maximize();
                chromeDriver.Navigate().GoToUrl("https://chat.zalo.me/");
                chromeDriver.ExecuteScript("document.body.style.zoom='80 %'");
                ShowNotification("");

                SetShareUi(false);
            }
            catch (Exception ex)
            {
                string text = ex.Message;
                if (text.Contains("--user-data-dir"))
                {
                    text = "Bạn phải đóng tất cả các trình duyệt chrome để chạy chương trình này";
                }
                ShowNotification(text);
            }
        }

        void ShowNotification(string text)
        {
            Invoke(new MethodInvoker(() =>
            {
                lblThongBao.Text = text;
            }));
        }

        void runSleep(int value)
        {
            System.Threading.Thread.Sleep(value);
        }
        static public void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }
    }
}
