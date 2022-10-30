using Xunit;
using Xunit.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

// dotnet test --logger:"console;verbosity=detailed"

using NuGetNN;

namespace Tests
{
    public class UnitTests
    {
        private readonly ITestOutputHelper _output;
        public UnitTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Test()
        {
            Image<Rgb24> image1 = Image.Load<Rgb24>("face1.png");
            Image<Rgb24> image2 = Image.Load<Rgb24>("fear.jpg");
            Image<Rgb24> image3 = Image.Load<Rgb24>("depp.jpg");

            var emotionNN = new EmotionNN();

            var emotions1 = emotionNN.EmotionFerplusAsync(image1, CancellationToken.None);
            var emotions2 = emotionNN.EmotionFerplusAsync(image2, CancellationToken.None);
            var emotions3 = emotionNN.EmotionFerplusAsync(image3, CancellationToken.None);

            await emotions1;
            await emotions2;
            await emotions3;

            _output.WriteLine("Image 1");
            foreach (KeyValuePair<string, float> kvp in emotions1.Result)
                _output.WriteLine($"{kvp.Key}, {kvp.Value}");
            _output.WriteLine("\nImage 2");
            foreach (KeyValuePair<string, float> kvp in emotions2.Result)
                _output.WriteLine($"{kvp.Key}, {kvp.Value}");
            _output.WriteLine("\nImage 3");
            foreach (KeyValuePair<string, float> kvp in emotions3.Result)
                _output.WriteLine($"{kvp.Key}, {kvp.Value}");
        }
    }
}