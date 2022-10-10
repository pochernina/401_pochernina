using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NuGetNN
{
    public class EmotionNN
    {
        private InferenceSession session;
        public EmotionNN()
        {
            using var modelStream = typeof(EmotionNN).Assembly.GetManifestResourceStream("package.emotion-ferplus-7.onnx");
            using var memoryStream = new MemoryStream();
            modelStream!.CopyTo(memoryStream);
            session = new InferenceSession(memoryStream.ToArray());
        }

        ~EmotionNN()
        {
            session.Dispose();
        }

        private string[] keys = { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };

        public async Task<Dictionary<string, float>> EmotionFerplusAsync(Image<Rgb24> image, CancellationToken token)
        {
            return await Task<Dictionary<string, float>>.Factory.StartNew(() =>
            {
                image.Mutate(ctx => ctx.Resize(new Size(64, 64)));
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", GrayscaleImageToTensor(image)) };
                token.ThrowIfCancellationRequested();
                float[] softmax_var;
                lock (this.session)
                {
                    using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
                    softmax_var = results.First(v => v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray();
                }
                var emotions = Softmax(softmax_var);
                return keys.Zip(emotions).ToDictionary(item => item.First, item => item.Second);
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private DenseTensor<float> GrayscaleImageToTensor(Image<Rgb24> img)
        {
            var w = img.Width;
            var h = img.Height;
            var t = new DenseTensor<float>(new[] { 1, 1, h, w });

            img.ProcessPixelRows(pa =>
            {
                for (int y = 0; y < h; y++)
                {
                    Span<Rgb24> pixelSpan = pa.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        t[0, 0, y, x] = pixelSpan[x].R;
                    }
                }
            });

            return t;
        }
        private float[] Softmax(float[] z)
        {
            var exps = z.Select(x => Math.Exp(x)).ToArray();
            var sum = exps.Sum();
            return exps.Select(x => (float)(x / sum)).ToArray();
        }
    }
}