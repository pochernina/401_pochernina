using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Windows;
using Polly;
using Polly.Retry;

namespace wpf_client
{
    public partial class MainWindow : Window
    {
        public Data data { get; set; }
        CancellationTokenSource cts;
        readonly string  server_url;
        readonly AsyncRetryPolicy retryPolicy;

        public MainWindow()
        {
            InitializeComponent();
            data = new Data();
            cts = new CancellationTokenSource();
            DataContext = this;
            server_url = "https://localhost:5001/";
            var jitterer = new Random();
            retryPolicy = Policy
                          .Handle<HttpRequestException>()
                          .WaitAndRetryAsync(5,
                           retryAttempt => TimeSpan.FromMilliseconds(100));
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

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    foreach (var path in image_paths)
                    {
                        byte[] image = System.IO.File.ReadAllBytes(path);

                        var http = new HttpClient();
                        http.BaseAddress = new Uri(server_url);
                        http.DefaultRequestHeaders.Accept.Clear();
                        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = await HttpClientJsonExtensions.PostAsJsonAsync(http, "api/images", Convert.ToBase64String(image), cts.Token);
                        response.EnsureSuccessStatusCode();
                        string result = await response.Content.ReadFromJsonAsync<string>();

                        if (result == "") --data.MaxProgress;
                        else
                        {
                            Dictionary<string, float> emotions_dict = result.Split("; ")
                                                                            .Select(s => s.Split(": "))
                                                                            .ToDictionary(s => s[0], s => float.Parse(s[1]));
                            data.Results.Add(new Result { Image = image, Emotions = emotions_dict });
                            pb.Value++;
                            data.ProgressText = pb.Value.ToString();
                        }
                    }
                });
            }
            catch (Exception) { MessageBox.Show("Server is unavailable \n or\nCalculations have been cancelled"); }
            finally
            {
                data.CancelEnabled = false;
                data.ClearEnabled = true;
            }
        }

        private async void GetImagesFromDb(object sender, RoutedEventArgs e)
        {
            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    var http = new HttpClient();
                    http.BaseAddress = new Uri(server_url);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response_images = await http.GetAsync("api/images/images_db");
                    response_images.EnsureSuccessStatusCode();
                    List<string> images = await response_images.Content.ReadFromJsonAsync<List<string>>();
                    var response_results = await http.GetAsync("api/images/results_db");
                    response_results.EnsureSuccessStatusCode();
                    List<string> emotions = await response_results.Content.ReadFromJsonAsync<List<string>>();

                    var items = images.Zip(emotions, (i, e) => new
                    {
                        image = Convert.FromBase64String(i),
                        emotions = e
                    });
                    foreach (var item in items)
                    {
                        Dictionary<string, float> emotions_dict = item.emotions
                                                                      .Split("; ")
                                                                      .Select(s => s.Split(": "))
                                                                      .ToDictionary(s => s[0], s => float.Parse(s[1]));
                        data.Results.Add(new Result { Image = item.image, Emotions = emotions_dict });
                    }
                });
            }
            catch (Exception) { MessageBox.Show("Server is unavailable"); }
            finally
            {
                data.UploadEnabled = false;
                data.ClearEnabled = true;
            }
        }

        private async void KeyChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    ObservableCollection<Result> sorted_results = new();
                    var http = new HttpClient();
                    http.BaseAddress = new Uri(server_url);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await http.GetAsync($"api/images?emotion={data.SelectedKey}");
                    response.EnsureSuccessStatusCode();
                    Dictionary<string, string> results = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                    data.Results.Clear();
                    foreach (var r in results)
                    {
                        Dictionary<string, float> emotions_dict = r.Value.Split("; ")
                                                                         .Select(s => s.Split(": "))
                                                                         .ToDictionary(s => s[0], s => float.Parse(s[1]));
                        data.Results.Add(new Result
                        {
                            Image = Convert.FromBase64String(r.Key),
                            Emotions = emotions_dict
                        });
                    }
                });
            }
            catch (Exception) { MessageBox.Show("Server is unavailable"); }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }

        private async void Clear(object sender, RoutedEventArgs e)
        {
            data.Results.Clear();
            data.MaxProgress = 0;
            data.ProgressText = "";

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    var http = new HttpClient();
                    http.BaseAddress = new Uri(server_url);
                    var response = await http.DeleteAsync("api/images");
                    response.EnsureSuccessStatusCode();
                });
            }
            catch (Exception) { MessageBox.Show("Server is unavailable"); }
            finally { data.UploadEnabled = true; }
        }
    }
}
