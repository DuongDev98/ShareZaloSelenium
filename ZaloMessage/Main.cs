using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            tmrProgressBar = new Timer();
            tmrProgressBar.Interval = 100;
            tmrProgressBar.Tick += TmrProgressBar_Tick;

            progressBar.Minimum = 0;
            progressBar.Maximum = 200;

            btnMoZalo.Click += BtnMoZalo_Click;
            btnChiaSe.Click += BtnChiaSe_Click;
            btnFile.Click += BtnFile_Click;
            btnDung.Click += BtnDung_Click1;
            FormClosing += Main_FormClosing;

            SetShareUi(false);
        }

        private void BtnDung_Click1(object sender, EventArgs e)
        {
            runChuongTrinh = false;
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

                //scroll to end
                ScrollToEnd();

                List<string> userTextCollection = new List<string>();
                bool timKiemDuoiLenTren = false;
                IWebElement lastUser = null;
                do
                {
                    IReadOnlyCollection<IWebElement> userCollection = chromeDriver.FindElements(By.ClassName(ClassJs.ITEM_MESSAGE));
                    for (int i = userCollection.Count - 1; i >= 0; i--)
                    {
                        IWebElement element = userCollection.ElementAt(i);
                        string text = element.FindElement(By.ClassName(ClassJs.TEN_NGUOI_DUNG)).Text;
                        if (!userTextCollection.Contains(text)) userTextCollection.Add(text);
                    }

                    if (lastUser == null || !userCollection.ElementAt(0).Equals(lastUser))
                    {
                        timKiemDuoiLenTren = true;
                        //Sctroll lên trên
                        lastUser = userCollection.ElementAt(0);
                        actions.ScrollToElement(lastUser).Perform();
                    }
                    else
                    {
                        timKiemDuoiLenTren = false;
                    }
                }
                while (timKiemDuoiLenTren);

                StringBuilder sb = new StringBuilder();
                foreach (string item in userTextCollection)
                {
                    sb.AppendLine(item);
                }
                if (sb.ToString().Length > 0)
                {
                    if (File.Exists(filePath)) File.Delete(filePath);
                    File.AppendAllText(filePath, sb.ToString());
                }

                ShowNotification("Xuất thành công");
            }
            catch (Exception ex)
            {
                ShowNotification(ex.Message + ": " + ex.StackTrace);
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

            Task task = new Task(() =>
            {
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
                        IWebElement lastUser = null;
                        do
                        {
                            bool shared = false;
                            //Tìm kiếm tin nhắn mới trên màn hình, khi hết tin nhắn mới thì dừng lại
                            IReadOnlyCollection<IWebElement> userCollection = chromeDriver.FindElements(By.ClassName(ClassJs.ITEM_MESSAGE));
                            for (int i = userCollection.Count - 1; i >= 0; i--)
                            {
                                shared = false;
                                IWebElement element = userCollection.ElementAt(i);
                                string text = element.FindElement(By.ClassName(ClassJs.TEN_NGUOI_DUNG)).Text;

                                //Kiểm tra có tin nhắn mới không
                                var lstTemp = element.FindElements(By.ClassName(ClassJs.TIN_NHAN_MOI));
                                if (lstTemp.Count == 0)
                                {
                                    if (i == 0) break;
                                    else continue;
                                }

                                //Kiểm tra thời gian có hợp lệ không?
                                string timeText = element.FindElement(By.ClassName(ClassJs.ITEM_TIME)).Text.Trim().ToLower();
                                if (timeText.EndsWith("phút"))
                                {
                                    timeText = timeText.Replace("phút", "").Trim();
                                    int phut = int.Parse(timeText);
                                    if (phut < 20) continue;
                                }
                                else if (timeText.Contains("vài giây")) continue;

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
                                        runSleep(20000);
                                    }

                                    //lấy tin nhắn mới đầu tiên
                                    runSleep(2000);
                                    IWebElement firtNewMessage = null;
                                    var messageCollection = chromeDriver.FindElements(By.ClassName(ClassJs.CHAT_MESSAGE));
                                    for (int dem = 0; dem < messageCollection.Count; dem++)
                                    {
                                        if (firtNewMessage == null)
                                        {
                                            //tin nhắn mới 1 ảnh, 1 ảnh 1 tin nhắn
                                            firtNewMessage = messageCollection.ElementAt(dem).FindElements(By.ClassName(ClassJs.HIGHLIGHT1)).Count > 0 ? messageCollection.ElementAt(dem) : null;
                                        }

                                        if (firtNewMessage == null)
                                        {
                                            //1 tin nhắn + nhiều ảnh
                                            firtNewMessage = messageCollection.ElementAt(dem).FindElements(By.ClassName(ClassJs.HIGHLIGHT2)).Count > 0 ? messageCollection.ElementAt(dem) : null;
                                        }

                                        if (firtNewMessage != null) break;
                                    }

                                    if (firtNewMessage != null)
                                    {
                                        string dataFirtNewMessage = GetBaseData(firtNewMessage);

                                        //thấy nút share đầu tiên thì share luôn
                                        ClickShareMessage(text, dicFiles);

                                        IWebElement lastNewMessage = null;
                                        //duyệt đến cuối tin nhắn
                                        bool end = false;
                                        do
                                        {
                                            chromeDriver.ExecuteScript(@"var vContainer = document.getElementById('messageViewContainer');
                                            var viewSc = vContainer.getElementsByTagName('div')[0];
                                            viewSc.scroll(0, viewSc.scrollHeight);");
                                            runSleep(2000);

                                            messageCollection = chromeDriver.FindElements(By.ClassName(ClassJs.CHAT_MESSAGE));
                                            if (lastNewMessage != null && messageCollection.ElementAt(messageCollection.Count - 1).Equals(lastNewMessage))
                                            {
                                                end = true;
                                            }

                                            lastNewMessage = messageCollection.ElementAt(messageCollection.Count - 1);
                                        }
                                        while (!end);

                                        firtNewMessage = TimTinNhanMoiDauTien(dataFirtNewMessage, messageCollection);

                                        if (firtNewMessage != null)
                                        {
                                            try
                                            {
                                                foreach (var item in messageCollection)
                                                {
                                                    if (shared)
                                                    {
                                                        //hover
                                                        runSleep(1000);
                                                        actions.ScrollToElement(item).Perform();

                                                        runSleep(1000);
                                                        actions.MoveToElement(item.FindElement(By.ClassName(ClassJs.MESSAGE_ACTION_HOLDER))).Perform();

                                                        ClickShareMessage(text, dicFiles);
                                                    }

                                                    if (item.Equals(firtNewMessage))
                                                    {
                                                        shared = true;
                                                    }
                                                }
                                            }
                                            catch (Exception exHolder)
                                            {
                                            }
                                        }
                                    }
                                }
                                if (shared) break;
                            }

                            if (!shared)
                            {
                                if (lastUser == null || !userCollection.ElementAt(0).Equals(lastUser))
                                {
                                    timKiemDuoiLenTren = true;
                                    //Sctroll lên trên
                                    lastUser = userCollection.ElementAt(0);
                                    actions.ScrollToElement(lastUser).Perform();
                                }
                                else
                                {
                                    timKiemDuoiLenTren = false;
                                }
                            }
                            else
                            {
                                timKiemDuoiLenTren = true;
                            }
                        }
                        while (timKiemDuoiLenTren && runChuongTrinh);

                        if (runChuongTrinh) runSleep(10000);
                    }

                    ShowNotification("");
                    Invoke(new MethodInvoker(() =>
                    {
                        SetShareUi(false);
                    }));
                }
                catch (Exception ex)
                {
                    ShowNotification(ex.Message + ": " + ex.StackTrace);
                    File.AppendAllText(Directory.GetCurrentDirectory()+@"/log.txt", "--" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + Environment.NewLine + ex.StackTrace, Encoding.UTF8);
                    Invoke(new MethodInvoker(() =>
                    {
                        SetShareUi(false);
                    }));
                }
            });

            task.Start();
        }

        private void ClickShareMessage(string text, Dictionary<string, List<string>> dicFiles)
        {
            runSleep(2000);
            IWebElement btnShare = null;

            try
            {
                btnShare = chromeDriver.FindElement(By.CssSelector("[data-id='" + ClassJs.BTN_SHARE + "']"));
            }
            catch (Exception exShare1)
            {
                try
                {
                    btnShare = chromeDriver.FindElement(By.CssSelector("[data-id='" + ClassJs.BTN_LAST_SHARE + "']"));
                }
                catch (Exception exShare2)
                {
                    btnShare = null;
                }
            }

            if (btnShare != null)
            {
                //click share
                actions.Click(btnShare).Perform();

                //chọn tên chia sẻ
                string temp = LayTenNhomChiaSeDen(text, dicFiles);

                runSleep(500);
                IWebElement txtSearchUser = chromeDriver.FindElement(By.CssSelector("[data-id='" + ClassJs.TXT_USER_SHARE + "']"));
                txtSearchUser.SendKeys(temp);

                runSleep(500);
                var lstUserShare = chromeDriver.FindElements(By.ClassName(ClassJs.ITEM_USER_SHARE));

                //thấy thì share, không thấy thì hủy
                bool clickShare = false;
                foreach (IWebElement itemUserShare in lstUserShare)
                {
                    if (itemUserShare.Text.Trim().ToLower().Contains(temp.ToLower()))
                    {
                        clickShare = true;
                        actions.Click(itemUserShare).Perform();
                        break;
                    }
                }

                runSleep(500);
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
                runSleep(2000);
            }
        }

        private IWebElement TimTinNhanMoiDauTien(string dataFirtNewMessage, ReadOnlyCollection<IWebElement> messageCollection)
        {
            foreach (var item in messageCollection)
            {
                if (dataFirtNewMessage == GetBaseData(item))
                {
                    return item;
                }
            }
            return null;
        }

        private string GetBaseData(IWebElement element)
        {
            StringBuilder data = new StringBuilder();
            var temp = element.FindElements(By.TagName("img"));
            if (temp.Count > 0)
            {
                foreach (var img in temp)
                {
                    data.AppendLine(img.GetAttribute("src"));
                }
            }
            data.AppendLine(element.Text.Trim());
            return data.ToString().Trim();
        }

        private void SetShareUi(bool run)
        {
            tmrProgressBar.Enabled = run;
            progressBar.Value = 0;
            if (run) lblThongBao.Text = "";

            btnMoZalo.Enabled = chromeDriver == null;
            btnChiaSe.Enabled = !run && !btnMoZalo.Enabled;
            btnDung.Enabled = run;
            btnFile.Enabled = !run && !btnMoZalo.Enabled;
        }

        private string LayTenNhomChiaSeDen(string text, Dictionary<string, List<string>> dicFiles)
        {
            // sửa danh sách nhóm chia sẻ đến
            foreach (string key in dicFiles.Keys)
            {
                foreach (string value in dicFiles[key])
                {
                    if (value.Trim().ToLower() == text.Trim().ToLower())
                    {
                        if (key.EndsWith("giaydep.txt"))
                        {
                            return "Giày Mũ, Phụ Kiện, Đồ Lót Thảo Phương";
                            //return "shareduong";
                        }
                        else if (key.EndsWith("hangsale.txt"))
                        {
                            return "Hàng Sale Thảo Phương";
                            //return "shareduong2";
                        }
                        else if (key.EndsWith("quanao.txt"))
                        {
                            return "Quần Áo, Váy Set Thảo Phương";
                            // return "shareduong3";
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
                ShowNotification(ex.Message + ": " + ex.StackTrace);
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
                options.AddArguments("--user-data-dir=" + Environment.ExpandEnvironmentVariables("%localappdata%") + @"\Google\Chrome\User Data");
                options.AddArguments("--profile-directory=Default");

                chromeDriver = new ChromeDriver(cService, options);
                actions = new Actions(chromeDriver);
                chromeDriver.Manage().Window.Maximize();
                chromeDriver.Navigate().GoToUrl("https://chat.zalo.me/");
                //chromeDriver.ExecuteScript("document.body.style.zoom='80 %'");
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
