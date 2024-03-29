﻿using System.Collections.Generic;
using System.ComponentModel;

namespace wpf
{
    public class Result : INotifyPropertyChanged
    {
        private string path;
        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged("Path");
            }
        }

        public Dictionary<string, float> Emotions { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

