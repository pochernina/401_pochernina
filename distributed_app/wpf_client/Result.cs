using System.Collections.Generic;
using System.ComponentModel;
using System.Printing.IndexedProperties;

namespace wpf_client
{
    public class Result : INotifyPropertyChanged
    {
        private byte[] image;
        public byte[] Image
        {
            get => image;
            set
            {
                image = value;
                OnPropertyChanged("Image");
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

