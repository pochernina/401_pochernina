using Microsoft.AspNetCore.Mvc;
using NuGetNN;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace server.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        [HttpPost]
        public async Task<string> Predict([FromBody]string image, CancellationToken token)
        {
            var emotionNN = new EmotionNN();
            string result = "";
            using (var db = new Database())
            {
                byte[] img_bytes = Convert.FromBase64String(image);
                Image<Rgb24> img = Image.Load<Rgb24>(img_bytes);
                int hash = new BigInteger(img_bytes).GetHashCode();
                var q = db.Items.Where(i => i.Hash == hash)
                                .Where(i => Enumerable.SequenceEqual(i.Image, img_bytes))
                                .FirstOrDefault();
                if (q == null)
                {
                    try
                    {
                        var emotions = await emotionNN.EmotionFerplusAsync(img, token);

                        emotions = emotions.OrderByDescending(e => e.Value).ToDictionary(item => item.Key, item => item.Value);
                        result = string.Join("; ", emotions.Select(e => string
                                                           .Format("{0}: {1}", e.Key, e.Value)));

                        ImageItem item = new ImageItem
                        {
                            Hash = hash,
                            Image = img_bytes,
                            Emotions = result
                        };
                        db.Add(item);
                        db.SaveChanges();
                    }
                    catch (Exception) { return "error"; }
                }
            }
            return result;
        }

        [HttpGet("images_db")]
        public async Task<List<string>> GetImagesFromDb()
        {
            var images = new List<string>();
            using (var db = new Database())
            {
                foreach (var item in db.Items)
                {
                    images.Add(Convert.ToBase64String(item.Image));
                }
            }
            return images;
        }

        [HttpGet("results_db")]
        public async Task<List<string>> GetResultsFromDb()
        {
            var results = new List<string>();
            using (var db = new Database())
            {
                foreach (var item in db.Items)
                {
                    results.Add(item.Emotions);
                }
            }
            return results;
        }

        [HttpGet]
        public async Task<Dictionary<string, string>> GetBestImages(string emotion)
        {
            var results = new Dictionary<string, string>();
            using (var db = new Database())
            {
                foreach (var item in db.Items)
                {
                    if (item.Emotions.Split("; ").First().Split(": ").First() == emotion)
                        results.Add(Convert.ToBase64String(item.Image), item.Emotions);
                }
            }
            return results;
        }

        [HttpDelete]
        public async void DeleteImagesFromDb()
        {
            using (var db = new Database())
            {
                foreach (var item in db.Items)
                    db.Items.Remove(item);

                db.SaveChanges();
            }
        }
    }
}