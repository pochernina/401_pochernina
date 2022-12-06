using NuGetNN;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows;

namespace wpf_with_db
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
            else return;

            data.MaxProgress += image_paths.Count;
            data.CancelEnabled = true;
            data.ClearEnabled = false;

            using (var db = new Database())
            {
                var emotionNN = new EmotionNN();
                foreach (var path in image_paths)
                {
                    Image<Rgb24> img = Image.Load<Rgb24>(path);
                    byte[] img_bytes = System.IO.File.ReadAllBytes(path);
                    int hash = new BigInteger(img_bytes).GetHashCode();
                    var q = db.Items.Where(i => i.Hash == hash)
                                    .Where(i => Enumerable.SequenceEqual(i.Image, img_bytes))
                                    .FirstOrDefault();
                    if (q == null)
                    {
                        try
                        {
                            token = cts.Token;

                            var emotions = await emotionNN.EmotionFerplusAsync(img, token);
                            emotions = emotions.OrderByDescending(e => e.Value).ToDictionary(item => item.Key, item => item.Value);

                            data.Results.Add(new Result { Path = path, Emotions = emotions });
                            pb.Value++;
                            data.ProgressText = pb.Value.ToString();

                            string emotions_str = string.Join("; ", emotions
                                                        .Select(e => string
                                                        .Format("{0}: {1}", e.Key, e.Value)));
                            ImageItem item = new ImageItem
                            {
                                Path = path,
                                Hash = hash,
                                Image = img_bytes,
                                Emotions = emotions_str
                            };
                            db.Add(item);
                            db.SaveChanges();
                        }
                        catch (Exception) { MessageBox.Show("Calculations have been canceled"); break; }
                    }
                    else --data.MaxProgress;
                }
            }
            data.CancelEnabled = false;
            data.ClearEnabled = true;
            KeyChanged(sender, e);
        }

        private void GetImagesFromDb(object sender, RoutedEventArgs e)
        {
            using (var db = new Database())
            {
                foreach (var item in db.Items)
                {
                    Dictionary<string, float> emotions_dict = item.Emotions
                                                                  .Split("; ")
                                                                  .Select(s => s.Split(": "))
                                                                  .ToDictionary(s => s[0], s => float.Parse(s[1]));
                    data.Results.Add(new Result { Path = item.Path, Emotions = emotions_dict });
                }
            }
            KeyChanged(sender, e);
            data.UploadEnabled = false;
            data.ClearEnabled= true;
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

        private void DeleteImage(object sender, RoutedEventArgs e)
        {
            if (lb.SelectedIndex == -1) return;
            var item = data.Results[lb.SelectedIndex];
            using (var db = new Database()) 
            {
                db.Items.Remove(db.Items.Where(i => i.Path == item.Path).First());
                db.SaveChanges();
            }
            data.Results.Remove(item);
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
            data.UploadEnabled = true;
        }
    }
}
