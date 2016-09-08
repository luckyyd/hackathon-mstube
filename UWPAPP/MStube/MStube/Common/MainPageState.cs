using MStube.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MStube.Common
{
    public static class MainPageState
    {
        public enum State
        {
            None,
            Recommend,
            Topic,
            Latest,
            Search
        }
        static int MAX_COUNT = 50;
        static State _currentState = State.None;
        static string _currentText = "";
        static Dictionary<State, ObservableCollection<VideoViewModel>> _videos = new Dictionary<State, ObservableCollection<VideoViewModel>>();
        public static State currentState
        {
            get { return _currentState; }
            set { _currentState = value; }
        }

        public static string currentText
        {
            get { return _currentText; }
            set { _currentText = value; }
        }

        #region Convert Utils
        private static ObservableCollection<T> Convert<T>(IEnumerable<T> original)
        {
            return new ObservableCollection<T>(original);
        }
        private static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            var result = new ObservableCollection<T>();
            foreach (var item in source)
                result.Add(item);
            return result;
        }
        #endregion

        public static ObservableCollection<VideoViewModel> getVideo(State state)
        {
            if (!_videos.ContainsKey(state))
            {
                _videos.Add(state, new ObservableCollection<VideoViewModel>());
            }
            return Convert(_videos[state].Take(MAX_COUNT));
        }

        public static void setVideo(State state, ObservableCollection<VideoViewModel> video)
        {
            if (!_videos.ContainsKey(state))
            {
                _videos.Add(state, new ObservableCollection<VideoViewModel>());
            }
            if (state.Equals(State.Recommend))
            {
                _videos[state] = video.Union(_videos[state]).ToObservableCollection();
            }
            else {
                _videos[state] = video;
            }
        }
    }
}
