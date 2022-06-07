using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfNaverMovieFindet.models;

namespace WpfNaverMovieFindet
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        bool IsFavorite = false;    // 네이버API로 검색한건지, 즐겨찾기 DB에서 온 것인지 확인할 값
        // IsFavorite == True -> DB에서 온 값 / IsFavorite == false -> 네이버API

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 검색창 입력 후 Enter키로 검색
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearchName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                btnSearch_Click(sender, e);
            }
        }

        /// <summary>
        /// 검색버튼 클릭 이벤트 핸들러
        /// 네이버 OpenAPI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            stsResult.Content = string.Empty;
            if (string.IsNullOrEmpty(txtSearchName.Text))
            {
                stsResult.Content = "검색할 영화명을 입력, 검색버튼을 눌러주세요.";
                //MessageBox.Show("검색할 영화명을 입력, 검색버튼을 눌러주세요.");
                commons.ShowMessageAsync("검색", "검색할 영화명을 입력, 검색버튼을 눌러주세요.");
                return;
            }

            // 검색시작
            //commons.ShowMessageAsync("결과", $"{txtSearchName.Text}");
            try
            {
                SearchNameOpenApi(txtSearchName.Text);
                commons.ShowMessageAsync("검색", "영화검색 완료!!");
                IsFavorite = false;     // API로 검색했으므로
            }
            catch (System.Exception ex)
            {
                commons.ShowMessageAsync("예외", $"예외발생 : {ex}");
            }
        }

        /// <summary>
        /// 네이버 영화 검색 메서드
        /// </summary>
        /// <param name="searchName"></param>
        private void SearchNameOpenApi(string searchName)
        {
            string clientID = "8_pPTDTU00XtqkabqGKD";
            string clientSecret = "Z4NOukDvm5";
            string openApiUri = $"https://openapi.naver.com/v1/search/movie?start=1&display=30&query={searchName}";
            string result = string.Empty;   // 빈 값 초기화
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

            var parsedJson = JObject.Parse(result);     // string to json

            int total = Convert.ToInt32(parsedJson["total"]);   // 전체 검색결과 수
            int display = Convert.ToInt32(parsedJson["display"]);

            stsResult.Content = $"{total} 중 {display} 호출 성공!";

            // 데이터그리드에 검색결과 할당
            var items = parsedJson["items"];
            var json_array = (JArray)items;

            List<MovieItem> movieItems = new List<MovieItem>();

            foreach (var item in json_array)
            {
                MovieItem movie = new MovieItem(
                    Regex.Replace(item["title"].ToString(), @"<(.|\n)*?>", string.Empty),
                    item["link"].ToString(),
                    item["image"].ToString(),
                    item["subtitle"].ToString(),
                    item["pubDate"].ToString(),
                    item["director"].ToString().Replace("|", ", "),
                    item["actor"].ToString().Replace("|", ", "),
                    item["userRating"].ToString());
                movieItems.Add(movie);
            }

            this.DataContext = movieItems;
        }

        /// <summary>
        /// 즐겨찾기 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddWatchList_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (grdResult.SelectedItems.Count == 0)
            {
                commons.ShowMessageAsync("오류", "즐겨찾기에 추가할 영화를 선택하세요(복수선택 가능)");
                return;
            }

            if (IsFavorite == true)
            {
                commons.ShowMessageAsync("오류", "이미 즐겨찾기한 영화 입니다!");
                return;
            }

            List<TblFavoriteMovies> list = new List<TblFavoriteMovies>();   //FavoriteMovieItem(x)
            foreach (MovieItem item in grdResult.SelectedItems)
            {
                TblFavoriteMovies temp = new TblFavoriteMovies()
                {
                    Title = item.Title,
                    Link = item.Link,
                    Image = item.Image,
                    SubTitle = item.SubTitle,
                    PubDate = item.PubDate,
                    Director = item.Director,
                    Actor = item.Actor,
                    UserRating = item.UserRating,
                    RegDate = DateTime.Now
                };

                list.Add(temp);
            }

            // EF 테이블 데이터 입력(INSERT)
            try
            {
                using (var ctx = new OpenApiLabEntities())
                {
                    foreach (var item in list)
                    {
                        ctx.Set<TblFavoriteMovies>().Add(item);     // INSERT문 대체
                    }
                    ctx.SaveChanges();
                }

                commons.ShowMessageAsync("저장", "즐겨찾기 추가 성공!!");
            }
            catch (Exception ex)
            {
                commons.ShowMessageAsync("예외", $"예외발생 : {ex}");
            }
        }

        /// <summary>
        /// 즐겨찾기 목록 보기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewWatchList_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DataContext = null;
            txtSearchName.Text = String.Empty;

            List<TblFavoriteMovies> list = new List<TblFavoriteMovies>();
            try
            {
                using (var ctx = new OpenApiLabEntities())
                {
                    list = ctx.TblFavoriteMovies.ToList();
                }

                this.DataContext = list;
                stsResult.Content = $"즐겨찾기 {list.Count}개 조회";
                commons.ShowMessageAsync("즐겨찾기", "즐겨찾기 조회 완료!");
                IsFavorite = true;     // DB에서 가져 왔으므로
            }
            catch (Exception ex)
            {
                commons.ShowMessageAsync("예외", $"예외발생 : {ex}");
                IsFavorite = false;
            }

        }

        /// <summary>
        /// 즐겨찾기 목록 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelWatchList_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsFavorite == false)
            {
                commons.ShowMessageAsync("오류", "즐겨찾기 내용이 아니면 삭제할 수 없습니다.");
                return;
            }
            if (grdResult.SelectedItems.Count == 0)
            {
                commons.ShowMessageAsync("오류", "삭제할 영화를 선택하세요.");
            }

            foreach (TblFavoriteMovies item in grdResult.SelectedItems)
            {
                using (var ctx = new OpenApiLabEntities())
                {
                    var delItem = ctx.TblFavoriteMovies.Find(item.idx);     // PK
                    ctx.Entry(delItem).State = System.Data.EntityState.Deleted;     // 객체상태를 삭제상태 변경
                    ctx.SaveChanges();      // commit
                }
            }

            btnViewWatchList_Click(sender, e);      // 즐겨찾기보기 버튼클릭 이벤트 실행
        }

        /// <summary>
        /// 유튜브 예고편 동영상보기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWatchTrailer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (grdResult.SelectedItems.Count == 0)
            {
                commons.ShowMessageAsync("유튜브 영화", "영화를 선택하세요");
                return;
            }
            if (grdResult.SelectedItems.Count > 1)
            {
                commons.ShowMessageAsync("유튜브 영화", "영화를 하나만 하세요");
                return;
            }

            string movieName = ""; // string.Empty
            if (IsFavorite == true)     // 네이버 API에서 온 값이면
            {
                movieName = (grdResult.SelectedItem as TblFavoriteMovies).Title;
            }
            else     // 즐겨찾기 DB 값이면
            {
                movieName = (grdResult.SelectedItem as MovieItem).Title;    // 한글 영화제목
            }
            
            var trailerWindow = new TrailerWindow(movieName);   // 영화제목 받는 생성자 변경!
            trailerWindow.Owner = this;     // MainWindow
            trailerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            trailerWindow.ShowDialog();     // 모달창
        }

        /// <summary>
        /// 선택한 영화 포스터 보이기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdResult_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            if (grdResult.SelectedItem is MovieItem)
            {
                var movie = grdResult.SelectedItem as MovieItem;
                if (string.IsNullOrEmpty(movie.Image))
                {
                    imgPoster.Source = new BitmapImage(new Uri("/resource/No_Picture.jpg", UriKind.RelativeOrAbsolute));
                }
                else     
                {
                    imgPoster.Source = new BitmapImage(new Uri(movie.Image, UriKind.RelativeOrAbsolute));
                }
            }

            if (grdResult.SelectedItem is TblFavoriteMovies)
            {
                var movie = grdResult.SelectedItem as TblFavoriteMovies;
                if (string.IsNullOrEmpty(movie.Image))
                {
                    imgPoster.Source = new BitmapImage(new Uri("/resource/No_Picture.jpg", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgPoster.Source = new BitmapImage(new Uri(movie.Image, UriKind.RelativeOrAbsolute));
                }
            }
        }

        /// <summary>
        /// 네이버 영화 웹브라우저 열기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNaverMovie_Click(object sender, RoutedEventArgs e)
        {
            if (grdResult.SelectedItems.Count == 0)
            {
                commons.ShowMessageAsync("네이버 영화", "영화를 선택하세요");
                return;
            }
            if (grdResult.SelectedItems.Count > 1)
            {
                commons.ShowMessageAsync("네이버 영화", "영화를 하나만 하세요");
                return;
            }

            string linkUrl = String.Empty;

            if (IsFavorite == true)
            {
                linkUrl = (grdResult.SelectedItem as TblFavoriteMovies).Link;
            }
            else
            {
                linkUrl = (grdResult.SelectedItem as MovieItem).Link;
            }
                
            Process.Start(linkUrl);
        }
    }
}
