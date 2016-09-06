using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MStube.Common;
using MStube.Items;
using MStube.ViewModels;
using MStube.Utils;
using Newtonsoft.Json;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Microsoft.HockeyApp;
using Windows.Foundation;
using System.ComponentModel;
using System.Collections.ObjectModel;
using VideoLibrary;
using System.Linq;
using Windows.Storage;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MStube
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<VideoViewModel> _videoList = new ObservableCollection<VideoViewModel>();
        public ObservableCollection<VideoViewModel> currentVideoList
        {
            get { return _videoList; }
            set
            {
                if (value != _videoList)
                {
                    _videoList = value;
                    VideoBriefList.ItemsSource = _videoList;
                    NotifyPropertyChanged();
                }
            }
        }
        private List<TopicViewModel> topicList = new List<TopicViewModel>();

        private DeviceInfo device = DeviceInfo.Instance;
        private long user_id = 0;
        public MainPage()
        {
            this.InitializeComponent();
            TopicList.Visibility = Visibility.Collapsed;
            if (MainPageState.currentState.Equals(MainPageState.State.None))
            {
                MainPageState.currentState = MainPageState.State.Recommend;
                this.InitializeValues();
            }
        }

        public async void InitializeValues(bool refresh = true)
        {
            SetText("");
            if (refresh == true)
            {
                LoadingProgressRing.IsActive = true;
                user_id = await GetUserId.GetUserIdFromServer(this.device);
                VideoBriefList.Visibility = Visibility.Collapsed;
                TopicList.Visibility = Visibility.Collapsed;
                List<VideoDetailItem> newVideoListCandidates = await Utils.GetRecommend.GetRecommendFromServer(user_id);
                List<VideoViewModel> videoView = GenerateVideoViewModel.GenerateVideoViewFromVideoDetail(newVideoListCandidates);
                MainPageState.currentState = MainPageState.State.Recommend;
                MainPageState.setVideo(MainPageState.State.Recommend, new ObservableCollection<VideoViewModel>(videoView));
                currentVideoList = MainPageState.getVideo(MainPageState.State.Recommend);
            }
            if (TopicList.Visibility == Visibility.Collapsed)
            {
                LoadingProgressRing.IsActive = false;
                VideoBriefList.Visibility = Visibility.Visible;
            }
        }

        private void ItemClicked(object sender, ItemClickEventArgs e)
        {
            VideoViewModel clickedItem = e.ClickedItem as VideoViewModel;
            clickedItem.user_id = user_id;
            HockeyClient.Current.TrackEvent("Item Clicked: " + clickedItem.item_id.ToString());
            Task.Run(() => Utils.SendPreference.SendPreferenceToServer(clickedItem.user_id, clickedItem.item_id, 4));
            Frame rootFrame = Window.Current.Content as Frame;
            switch (clickedItem.Source)
            {
                case "channel9":
                    rootFrame.Navigate(typeof(VideoPage), e.ClickedItem);
                    break;
                case "youtube":
                    var youTube = YouTube.Default;
                    var video = youTube.GetVideo(clickedItem.Url);
                    clickedItem.VideoSourceUri = video.Uri;
                    clickedItem.Description = clickedItem.FullDescription;
                    rootFrame.Navigate(typeof(VideoPage), e.ClickedItem);
                    break;
                case "vimeo":
                    rootFrame.Navigate(typeof(WebPage), e.ClickedItem);
                    break;
                default:
                    rootFrame.Navigate(typeof(VideoPage), e.ClickedItem);
                    break;
            }
        }

        private async void TopicClicked(object sender, ItemClickEventArgs e)
        {
            TopicViewModel clickedItem = e.ClickedItem as TopicViewModel;
            VideoBriefList.Visibility = Visibility.Collapsed;
            TopicList.Visibility = Visibility.Collapsed;
            LoadingProgressRing.IsActive = true;
            SetText(clickedItem.topic);
            List<VideoDetailItem> searchresult = await Utils.SearchTopic.SearchTopicToServer(clickedItem.topic);
            if (searchresult.Count >= 1)
            {
                List<VideoViewModel> videoView = GenerateVideoViewModel.GenerateVideoViewFromVideoDetail(searchresult);
                MainPageState.currentState = MainPageState.State.Topic;
                MainPageState.setVideo(MainPageState.State.Topic, new ObservableCollection<VideoViewModel>(videoView));
                currentVideoList = MainPageState.getVideo(MainPageState.State.Topic);
            }
            LoadingProgressRing.IsActive = false;
            VideoBriefList.Visibility = Visibility.Visible;
        }
        private void SetText(string text)
        {
            autoSuggestBox.Text = text;
            MainPageState.currentText = text;
        }
        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            this.InitializeValues();
        }

        #region Menu items click
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            InitializeValues();
        }

        private async void SearchTopic_Click(object sender, RoutedEventArgs e)
        {
            SetText("");
            LoadingProgressRing.IsActive = false;
            List<TopicViewModel> topicList = await GetTopicList.GetTopicListFromServer();
            TopicList.ItemsSource = topicList;
            TopicList.Visibility = Visibility.Visible;
            VideoBriefList.Visibility = Visibility.Collapsed;
        }

        private async void Latest_Click(object sender, RoutedEventArgs e)
        {
            VideoBriefList.Visibility = Visibility.Collapsed;
            TopicList.Visibility = Visibility.Collapsed;
            LoadingProgressRing.IsActive = true;
            SetText("Latest");
            List<VideoDetailItem> searchresult = await Utils.GetLastest.GetLatestFromServer();
            if (searchresult.Count >= 1)
            {
                List<VideoViewModel> videoView = GenerateVideoViewModel.GenerateVideoViewFromVideoDetail(searchresult);
                MainPageState.currentState = MainPageState.State.Latest;
                MainPageState.setVideo(MainPageState.State.Latest, new ObservableCollection<VideoViewModel>(videoView));
                currentVideoList = MainPageState.getVideo(MainPageState.State.Latest);
            }
            if (TopicList.Visibility == Visibility.Collapsed)
            {
                LoadingProgressRing.IsActive = false;
                VideoBriefList.Visibility = Visibility.Visible;
            }
        }

        private async void SendFeedback_Click(object sender, RoutedEventArgs e)
        {
            var mailto = new Uri(@"mailto:t-yimwan@microsoft.com?subject=MStube%20Feedback&body=");
            await Windows.System.Launcher.LaunchUriAsync(mailto);
        }
        #endregion

        #region AutoSuggestBox
        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                //User selected an item, take an action on it here
            }
            else
            {
                LoadingProgressRing.IsActive = true;
                List<VideoDetailItem> searchresult = await SearchTitle.SearchTitleToServer(args.QueryText);
                LoadingProgressRing.IsActive = false;
                SetText(args.QueryText);
                if (searchresult.Count >= 1)
                {
                    List<VideoViewModel> videoView = GenerateVideoViewModel.GenerateVideoViewFromVideoDetail(searchresult);
                    MainPageState.currentState = MainPageState.State.Search;
                    MainPageState.setVideo(MainPageState.State.Search, new ObservableCollection<VideoViewModel>(videoView));
                    currentVideoList = MainPageState.getVideo(MainPageState.State.Search);
                    TopicList.Visibility = Visibility.Collapsed;
                    VideoBriefList.Visibility = Visibility.Visible;
                }
            }
        }

        private async void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var selectedItem = args.SelectedItem.ToString();
            sender.Text = selectedItem;
            List<VideoDetailItem> searchresult = await SearchTitle.SearchTitleToServer(sender.Text);
            LoadingProgressRing.IsActive = false;
            SetText(sender.Text);
            if (searchresult.Count >= 1)
            {
                List<VideoViewModel> videoView = GenerateVideoViewModel.GenerateVideoViewFromVideoDetail(searchresult);
                MainPageState.currentState = MainPageState.State.Search;
                MainPageState.setVideo(MainPageState.State.Search, new ObservableCollection<VideoViewModel>(videoView));
                currentVideoList = MainPageState.getVideo(MainPageState.State.Search);
                TopicList.Visibility = Visibility.Collapsed;
                VideoBriefList.Visibility = Visibility.Visible;
            }
        }

        private List<string> _listSuggestion = null;
        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // User selected an item from the suggestion list, take an action on it here.
                List<string> wordList = new List<string>();
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Typeahead.txt"));
                using (var inputStream = await file.OpenReadAsync())
                using (var classicStream = inputStream.AsStreamForRead())
                using (var streamReader = new StreamReader(classicStream))
                {
                    while (streamReader.Peek() >= 0)
                    {
                        wordList.Add(streamReader.ReadLine());
                    }
                }
                _listSuggestion = wordList.Where(x => x.StartsWith(sender.Text)).ToList();
                sender.ItemsSource = _listSuggestion;
            }
            else
            {
                // Use args.QueryText to determine what to do.
            }
        }
        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // PropertyChanged event triggering method.
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            LoadingProgressRing.IsActive = false;
            currentVideoList = MainPageState.getVideo(MainPageState.currentState);
            autoSuggestBox.Text = MainPageState.currentText;
        }
    }
}
