using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace SORScrapHouse
{
    public partial class Form1 : Form
    {
        static IWebDriver driver;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            try
            {
                var driver = new ChromeDriver();
                string url = textBox1.Text;
                driver.Navigate().GoToUrl(url);

                IWebElement productListsWeb = driver.FindElement(By.Id("product-lists-web"));
                IList<IWebElement> divElements = productListsWeb.FindElements(By.ClassName("js__card"));

                // Thu thập các giá trị href
                List<string> hrefs = new List<string>();
                foreach (IWebElement div in divElements)
                {
                    IWebElement anchor = div.FindElement(By.TagName("a"));
                    string href = anchor.GetAttribute("href");
                    if (!string.IsNullOrEmpty(href))
                    {
                        hrefs.Add(href);
                    }
                }

                string filePath = "data.txt";

                // Ghi các giá trị href vào file, thêm dữ liệu mới vào cuối file nếu file đã tồn tại
                using (StreamWriter writer = new StreamWriter(filePath, append: true))
                {
                    foreach (string href in hrefs)
                    {
                        writer.WriteLine(href);
                    }
                }
                driver?.Quit();
                MessageBox.Show("Đã xong");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ex: {ex}");
            }
        }
        static void OnProcessExit(object sender, EventArgs e)
        {
            // Đảm bảo trình điều khiển Selenium được đóng khi ứng dụng thoát
            driver?.Quit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            try
            {
                ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
                chromeDriverService.HideCommandPromptWindow = true;
                ChromeOptions options = new ChromeOptions();
                options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
                string a = "";
                // Đường dẫn tới file
                string filePath = "data.txt";

                // Đọc tất cả các dòng từ file
                List<string> allLines = new List<string>();
                if (File.Exists(filePath))
                {
                    allLines = File.ReadAllLines(filePath).ToList();
                }
                else
                {
                    Console.WriteLine($"File '{filePath}' không tồn tại.");
                    return;
                }

                int soluong = (int)numericUpDown1.Value;
                var firstFiveHrefs = allLines.Take(soluong).ToList();

                string[] myarray = { "Diện tích", "Mặt tiền", "Đường vào", "Số tầng", "Số phòng ngủ", "Số toilet", "Mức giá" };

                // Lần lượt điều hướng tới từng link trong 5 đường link đầu tiên
                foreach (var link in firstFiveHrefs)
                {
                    var driver = new ChromeDriver(chromeDriverService,options);
                    driver.Navigate().GoToUrl(link);

                    IDictionary<string, string> dataValues = new Dictionary<string, string>()
                    {
                        {"Diện tích", ""},
                        { "Mức giá", ""},
                        { "Mặt tiền", ""},
                        { "Đường vào", ""},
                        { "Số tầng", ""},
                        { "Số phòng ngủ", ""},
                        { "Số toilet",""},
                        { "zipcode", textBox2.Text}
                    };               

                    IList<IWebElement> specItems = driver.FindElements(By.ClassName("re__pr-specs-content-item"));

                    foreach (IWebElement item in specItems)
                    {
                        IWebElement titleElement = item.FindElement(By.ClassName("re__pr-specs-content-item-title"));
                        string titleText = titleElement.Text;
                        if (myarray.Any(s => s.Equals(titleText, StringComparison.OrdinalIgnoreCase)))
                        {
                            // Lấy giá trị của thẻ có class 're__pr-specs-content-item-value'
                            IWebElement valueElement = item.FindElement(By.ClassName("re__pr-specs-content-item-value"));
                            string valueText = valueElement.Text;
                            valueText = valueText.Replace(",", ".");
                            // Lọc ra chỉ phần số, loại bỏ ký tự chữ
                            string numberOnly = Regex.Replace(valueText, @"[^0-9.]", "");
                            dataValues[titleText] = numberOnly;

                            //dataValues.Add(valueText);
                            a += titleText + ":" + numberOnly + "\n";
                        }
                    }

                    string filePathcsv = "train.csv";

                    // Ghi các giá trị vào file CSV, thêm dữ liệu mới vào cuối file nếu file đã tồn tại
                    using (StreamWriter writer = new StreamWriter(filePathcsv, append: true))
                    {
                        if (dataValues.Count > 0)
                        {
                            List<string> row = new List<string>();
                            foreach (var kvp in dataValues)
                            {
                                row.Add($"{kvp.Value}");
                            }

                            // Ghi toàn bộ danh sách vào một dòng trong file CSV
                            writer.WriteLine(string.Join(",", row));
                        }
                    }
                    driver?.Quit();
                }
                MessageBox.Show("Đã xong");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ex: {ex}");
            }
        }
    }
}
