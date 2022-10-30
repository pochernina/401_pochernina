using NuGetNN;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace wpf
{
    public partial class MainWindow : Window
    {
        public Data data { get; set; }
        CancellationTokenSource cts;
        CancellationToken token;

        public MainWindow()
        {
            InitializeComponent();
            data = new Data();
            cts = new CancellationTokenSource();
            DataContext = this;
        }

        private async void OpenDialogAndPredict(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            List<string> image_paths = new();
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (dialog.ShowDialog() == true)
                foreach (var path in System.IO.Directory.GetFiles(dialog.SelectedPath))
                    image_paths.Add(path);

            data.MaxProgress += image_paths.Count;
            data.Enabled = true;
            var emotionNN = new EmotionNN();
            foreach (var path in image_paths)
            {
                Image<Rgb24> img = Image.Load<Rgb24>(path);
                try
                {
                    token = cts.Token;
                    var emotions = await emotionNN.EmotionFerplusAsync(img, token);
                    emotions = emotions.OrderByDescending(e => e.Value).ToDictionary(item => item.Key, item => item.Value);
                    data.Results.Add(new Result { Path = path, Emotions = emotions });
                    pb.Value++;
                    data.ProgressText = pb.Value.ToString();
                }
                catch (Exception) { MessageBox.Show("Calculations have been canceled"); break; }
            }
            data.Enabled = false;
            KeyChanged(sender, e);
        }

        private void KeyChanged(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Result> sorted_results = new();
            List<Tuple<string, float>> l = new();
            foreach (var r in data.Results)
                l.Add((r.Path, r.Emotions[data.SelectedKey]).ToTuple());
            foreach (var x in l.OrderByDescending(i => i.Item2))
            {
                sorted_results.Add(new Result
                {
                    Path = x.Item1,
                    Emotions = data.Results.Where(r => r.Path == x.Item1).ToList().First().Emotions
                });
            }
            data.Results.Clear();
            foreach (var r in sorted_results)
                data.Results.Add(r);
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            data.Results.Clear();
            data.MaxProgress = 0;
            data.ProgressText = "";
        }
    }
}
