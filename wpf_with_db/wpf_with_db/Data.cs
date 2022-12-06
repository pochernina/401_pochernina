using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace wpf_with_db
{
    public class Data : INotifyPropertyChanged
    {
        private static readonly List<string> keys = new() { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
        public static List<string> Keys { get => keys; }

        private string selectedKey = keys[0];
        public string SelectedKey
        {
            get => selectedKey;
            set
            {
                selectedKey = value;
                OnPropertyChanged("SelectedKey");
            }
        }

        private int maxProgress = 0;
        public int MaxProgress
        {
            get => maxProgress;
            set
            {
                maxProgress = value;
                OnPropertyChanged("MaxProgress");
            }
        }

        private string progressText = "";
        public string ProgressText
        {
            get => progressText;
            set
            {
                progressText = string.Format("{0}/{1}", value, MaxProgress);
                OnPropertyChanged("ProgressText");
            }
        }

        private bool clearEnabled = false;
        public bool ClearEnabled
        {
            get => clearEnabled;
            set
            {
                clearEnabled = value;
                OnPropertyChanged("ClearEnabled");
            }
        }

        private bool cancelEnabled = false;
        public bool CancelEnabled
        {
            get => cancelEnabled;
            set
            {
                cancelEnabled = value;
                OnPropertyChanged("CancelEnabled");
            }
        }

        private bool uploadEnabled = true;
        public bool UploadEnabled
        {
            get => uploadEnabled;
            set
            {
                uploadEnabled = value;
                OnPropertyChanged("UploadEnabled");
            }
        }

        public ObservableCollection<Result> Results { get; set; } = new ObservableCollection<Result>();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
