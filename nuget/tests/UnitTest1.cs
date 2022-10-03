using Xunit;
using Xunit.Abstractions;
using NuGetNN;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
// dotnet test --logger:"console;verbosity=detailed"
namespace NugetNN
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;
        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test1()
        {
            Image<Rgb24> image = Image.Load<Rgb24>("face.png");
            var emotions = EmotionNN.EmotionFerplus(image);
            _output.WriteLine($"{emotions.GetType()}");
            foreach (KeyValuePair<string, float> kvp in emotions)
                _output.WriteLine($"{kvp.Key}, {kvp.Value}");
        }
    }
}