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
        public static ObservableCollection<VideoViewModel> getVideo(State state)
        {
            if (!_videos.ContainsKey(state))
            {
                _videos.Add(state, new ObservableCollection<VideoViewModel>());
            }
            return _videos[state];
        }
        public static void setVideo(State state, ObservableCollection<VideoViewModel> video)
        {
            if (!_videos.ContainsKey(state))
            {
                _videos.Add(state, new ObservableCollection<VideoViewModel>());
            }
            _videos[state] = video;
        }
    }
}
