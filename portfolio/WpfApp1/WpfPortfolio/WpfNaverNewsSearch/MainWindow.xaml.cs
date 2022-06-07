using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using WpfNaverNewsSearch.helpers;

namespace WpfNaverNewsSearch
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 검색창 입력 후 Enter키로 검색
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtSearch.Text))
                    {
                        commons.ShowMessageAsync("검색", "검색할 뉴스를 입력하세요.");
                        return;
                    }
                    else
                    {
                        //commons.ShowMessageAsync("실행", "뉴스검색 실행!");
                        SearchNaverNews();
                        commons.ShowMessageAsync("검색", "뉴스 검색완료 !!");
                    }
                }
            }
            catch (Exception ex)
            {
                commons.ShowMessageAsync("예외", $"예외 발생 : {ex}");
            }
        }

        /// <summary>
        /// 네이버 뉴스 검색 메서드
        /// </summary>
        /// <param name="searchName"></param>
        private void SearchNaverNews()
        {
            string keyword = txtSearch.Text;
            string clientID = "8_pPTDTU00XtqkabqGKD";
            string clientSecret = "Z4NOukDvm5";
            string openApiUri = $"https://openapi.naver.com/v1/search/news?start={txtStartNum.Text}&display=10&query={keyword}";
            string result;

            WebRequest request = null;
            WebResponse response = null;
            Stream stream = null;
            StreamReader reader = null;

            // Naver OpenAPI 실제 요청
            try
            {
                request = WebRequest.Create(openApiUri);
                request.Headers.Add("X-Naver-Client-Id", clientID);     // 중요!
                request.Headers.Add("X-Naver-Client-Secret", clientSecret);     // 중요!!

                response = request.GetResponse();
                stream = response.GetResponseStream();
                reader = new StreamReader(stream);

                result = reader.ReadToEnd();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                reader.Close();
                stream.Close();
                response.Close();
            }

            //MessageBox.Show(result);
            var parsedJson = JObject.Parse(result);     // string to json

            int total = Convert.ToInt32(parsedJson["total"]);   // 전체 검색결과 수
            int display = Convert.ToInt32(parsedJson["display"]);   // 10

            var items = parsedJson["items"];
            var json_array = (JArray)items;

            List<NewsItem> newsItems = new List<NewsItem>();    // 데이터그리드 연동

            foreach (var item in json_array)
            {
                var temp = DateTime.Parse(item["pubDate"].ToString());
                NewsItem news = new NewsItem()
                {
                    Title = Regex.Replace( item["title"].ToString(), @"<(.|\n)*?>", string.Empty),
                    OriginalLink = item["originallink"].ToString(),
                    Link = item["link"].ToString(),
                    Description = Regex.Replace(item["description"].ToString(), @"<(.|\n)*?>", string.Empty),
                    PubDate = temp.ToString("yyyy-MM-dd HH:mm")
                };

                newsItems.Add(news);
            }

            this.DataContext = newsItems;
        }

        /// <summary>
        /// 선택한 뉴스 보가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgrResult_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (dgrResult.SelectedItem == null)
            {
                return;     // 두번째 검색부터 오류 제거
            }

            string link = (dgrResult.SelectedItem as NewsItem).Link;
            Process.Start(link);
        }
    }

    public class NewsItem
    {
        public string Title { get; set; }
        public string OriginalLink { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string PubDate { get; set; }
    }
}
