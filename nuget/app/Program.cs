using NuGetNN;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

Image<Rgb24> image = Image.Load<Rgb24>("face1.png");
System.Collections.Generic.IEnumerable<(string Emotion, float Prob)> emotions = EmotionNN.EmotionFerplus(image);
foreach (var i in emotions)
    Console.WriteLine($"{i.Emotion}: {i.Prob}");