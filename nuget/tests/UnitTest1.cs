using Xunit;
using Xunit.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using NuGetNN;

namespace Tests
{
    public class UnitTests
    {
        [Fact]
        public async Task Test()
        {
            Image<Rgb24> image1 = Image.Load<Rgb24>("face1.png");
            Image<Rgb24> image2 = Image.Load<Rgb24>("fear.jpg");
            Image<Rgb24> image3 = Image.Load<Rgb24>("depp.jpg");

            var emotionNN = new EmotionNN();

            var emotions1 = await emotionNN.EmotionFerplusAsync(image1, CancellationToken.None);
            var emotions2 = await emotionNN.EmotionFerplusAsync(image2, CancellationToken.None);
            var emotions3 = await emotionNN.EmotionFerplusAsync(image3, CancellationToken.None);

            emotions1 = emotions1.OrderByDescending(e => e.Value).ToDictionary(item => item.Key, item => item.Value);
            emotions2 = emotions2.OrderByDescending(e => e.Value).ToDictionary(item => item.Key, item => item.Value);
            emotions3 = emotions3.OrderByDescending(e => e.Value).ToDictionary(item => item.Key, item => item.Value);

            Assert.Equal("happiness", emotions1.First().Key);
            Assert.Equal("anger", emotions2.First().Key);
            Assert.Equal("neutral", emotions3.First().Key);
        }
    }
}