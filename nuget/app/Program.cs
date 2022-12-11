using NuGetNN;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

var watch = new System.Diagnostics.Stopwatch();
watch.Start();

Image<Rgb24> image1 = Image.Load<Rgb24>("../../images/face1.png");
Image<Rgb24> image2 = Image.Load<Rgb24>("../../images/fear.jpg");
Image<Rgb24> image3 = Image.Load<Rgb24>("../../images/cat_face.jpg");
Image<Rgb24> image4 = Image.Load<Rgb24>("../../images/depp.jpg");

var emotionNN = new EmotionNN();

var emotions1 = emotionNN.EmotionFerplusAsync(image1, CancellationToken.None);
var emotions2 = emotionNN.EmotionFerplusAsync(image2, CancellationToken.None);
var emotions3 = emotionNN.EmotionFerplusAsync(image3, CancellationToken.None);
var emotions4 = emotionNN.EmotionFerplusAsync(image4, CancellationToken.None);

await emotions1;
await emotions2;
await emotions3;
await emotions4;

Console.WriteLine("--------------\nImage 1\n--------------");
foreach (KeyValuePair<string, float> kvp in emotions1.Result)
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
Console.WriteLine("--------------\nImage 2 (fear)\n--------------");
foreach (KeyValuePair<string, float> kvp in emotions2.Result)
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
Console.WriteLine("--------------\nImage 3 (cat)\n--------------");
foreach (KeyValuePair<string, float> kvp in emotions3.Result)
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
Console.WriteLine("--------------\nImage 4 (depp)\n--------------");
foreach (KeyValuePair<string, float> kvp in emotions4.Result)
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");

watch.Stop();
Console.WriteLine($"\n\nExecution time: {watch.ElapsedMilliseconds} ms");
