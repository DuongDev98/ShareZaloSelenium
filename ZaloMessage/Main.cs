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
        bool runChuongTrinh = false;
        ChromeDriver chromeDriver = null;
        Actions actions = null;
        public Main()
        {
            InitializeComponent();
            btnMoZalo.Click += BtnOpen_Click;
            btnChiaSe.Click += BtnChiaSe_Click;
            btnDung.Click += BtnDung_Click;
            btnFile.Click += BtnFile_Click;
            FormClosing += Main_FormClosing;
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
                while (timKiemDuoiLenTren && runChuongTrinh);

                if (sb.ToString().Length > 0)
                {
                    if (File.Exists(filePath)) File.Delete(filePath);
                    File.AppendAllText(filePath, sb.ToString());
                }
            }
            catch (Exception ex)
            {
                ShowWarning(ex.Message);
            }
        }

        private void BtnDung_Click(object sender, EventArgs e)
        {
            runChuongTrinh = false;
        }

        private void BtnChiaSe_Click(object sender, EventArgs e)
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
                    ShowWarning("Bạn chưa cấu hình dữ liệu");
                    runChuongTrinh = false;
                    return;
                }

                while (runChuongTrinh)
                {
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
                                }

                                bool hasNextMessage = false;
                                IWebElement lastMessage = null;
                                //tìm tin nhắn để chia sẻ
                                do
                                {
                                    runSleep(1000);
                                    var lstMessage = chromeDriver.FindElements(By.ClassName(ClassJs.CHAT_MESSAGE));

                                    //chia sẻ tin nhắn
                                    foreach (var messageItem in lstMessage)
                                    {
                                        try
                                        {
                                            //hover
                                            runSleep(1000);
                                            actions.MoveToElement(messageItem).Perform();
                                            //tìm biểu tượng chia sẻ

                                            runSleep(1000);
                                            IWebElement btnShare = chromeDriver.FindElement(By.CssSelector("[data-id='" + ClassJs.BTN_SHARE + "']"));
                                            if (btnShare != null)
                                            {
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
                                                bool shared = false;
                                                foreach (IWebElement itemUserShare in lstUserShare)
                                                {
                                                    if (itemUserShare.Text.Trim().ToLower() == temp)
                                                    {
                                                        shared = true;
                                                        actions.Click(itemUserShare).Perform();
                                                        break;
                                                    }
                                                }

                                                runSleep(1000);
                                                IWebElement btnTemp = null;
                                                shared = false;
                                                if (shared)
                                                {
                                                    btnTemp = chromeDriver.FindElement(By.CssSelector("[data-id='" + ClassJs.BTN_CONFIRM_SHARE + "']"));
                                                }
                                                else
                                                {
                                                    btnTemp = chromeDriver.FindElement(By.CssSelector("[data-id='" + ClassJs.BTN_CANCEL_SHARE + "']"));
                                                }
                                                actions.Click(btnTemp).Perform();
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
            }
            catch (Exception ex)
            {
                ShowWarning(ex.Message);
            }
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
                            return "dương";
                        }
                        else if (key.EndsWith("hangsale.txt"))
                        {
                            return "dương";
                        }
                        else if (key.EndsWith("quanao.txt"))
                        {
                            return "dương";
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

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                //service
                ChromeDriverService cService = ChromeDriverService.CreateDefaultService();
                cService.HideCommandPromptWindow = true;
                //options
                ChromeOptions options = new ChromeOptions();
                options.AddArguments(@"--user-data-dir=C:\Users\PC.DESKTOP-LHETLS5\AppData\Local\Google\Chrome\User Data");
                options.AddArguments(@"--profile-directory=Default");
                options.AddArgument("--no-sandbox");

                chromeDriver = new ChromeDriver(cService, options);
                actions = new Actions(chromeDriver);
                chromeDriver.Manage().Window.Maximize();
                chromeDriver.Navigate().GoToUrl("https://chat.zalo.me/");
            }
            catch (Exception ex)
            {
                ShowWarning("Vui lòng đóng hết trình duyệt chrome");
            }
        }

        void ShowWarning(string text)
        {
            MessageBox.Show(text, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
