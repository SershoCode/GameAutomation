using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace GameAutomation.Core;

public class ScreenScanner
{
    private readonly Dictionary<string, Bitmap> _objectImages;
    private readonly Tesseract _tessetactRu;
    private readonly Tesseract _tessetactEn;

    public ScreenScanner()
    {
        _objectImages = [];

        _tessetactRu = new Tesseract("./Screen/TessData", "rus", OcrEngineMode.TesseractOnly);
        _tessetactEn = new Tesseract("./Screen/TessData", "eng", OcrEngineMode.TesseractOnly);
    }

    public Bitmap GetScreenshot(int left, int top, int right, int bottom)
    {
        var bitmap = new Bitmap(right - left, bottom - top, PixelFormat.Format24bppRgb);

        using var graphics = Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(left, top, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);

        return bitmap;
    }

    public async Task<ObjectPosition> FindObjectAsync<TColor>(string imageName,
                                             int left,
                                             int top,
                                             int right,
                                             int bottom,
                                             int acceptableInaccuracyPercentage = 1,
                                             ScreenHighlighter highlighterForSearch = null)
                                             where TColor : struct, IColor
    {
        if (_objectImages == null)
            throw new FileNotFoundException("Пожалуйста, сначала загрузите изображения объектов через метод LoadObjectImagesForScans");

        HighlightedRectangle rectangle = null;

        if (highlighterForSearch != null)
            rectangle = await highlighterForSearch.AddRectangleAsync(left, top, right, bottom, 1, ColorType.Gray, FigureLineType.Dashed);

        if (acceptableInaccuracyPercentage > 100)
            acceptableInaccuracyPercentage = 100;

        using var objectImage = _objectImages[imageName]
            .ToImage<TColor, byte>();

        using var screenshotImage = GetScreenshot(left, top, right, bottom)
            .ToImage<TColor, byte>();

        using var outImage = new Mat();

        CvInvoke.MatchTemplate(screenshotImage, objectImage, outImage, Emgu.CV.CvEnum.TemplateMatchingType.SqdiffNormed);

        var minThd = acceptableInaccuracyPercentage * 0.01;

        var minVal = 0.0;
        var maxVal = 0.0;

        var minLoc = new Point();
        var maxLoc = new Point();

        CvInvoke.MinMaxLoc(outImage, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

        if (highlighterForSearch != null)
            highlighterForSearch.RemoveRectangle(rectangle);

        if (minThd > minVal)
        {
            var objectWidth = objectImage.Size.Width;
            var objectHeigth = objectImage.Size.Height;

            var leftLoc = minLoc.X;
            var topLoc = minLoc.Y;
            var rightLoc = minLoc.X + objectWidth;
            var bottomLoc = minLoc.Y + objectHeigth;

            return new ObjectPosition(leftLoc + (objectWidth / 2), topLoc + (objectHeigth / 2), leftLoc, topLoc, rightLoc, bottomLoc, objectWidth, objectHeigth);
        }

        return null;
    }

    public async Task<ObjectPosition> WaitObjectAsync<TColor>(string imageName,
                                                              int left,
                                                              int top,
                                                              int right,
                                                              int bottom,
                                                              TimeSpan timeForWait,
                                                              TimeSpan attemptsPeriodic,
                                                              int acceptableInaccuracyPercentage = 1,
                                                              ScreenHighlighter higlighterForSearchAttemps = null) where TColor : struct, IColor
    {
        var currentTime = DateTime.UtcNow;

        var endTime = currentTime.Add(timeForWait);

        var lastCheckTime = currentTime;

        while (lastCheckTime < endTime)
        {
            var objectPosition = await FindObjectAsync<TColor>(imageName, left, top, right, bottom, acceptableInaccuracyPercentage, higlighterForSearchAttemps);

            if (objectPosition != null)
                return objectPosition;

            lastCheckTime = DateTime.UtcNow;

            await Task.Delay(attemptsPeriodic);
        }

        return null;
    }

    public AverageColor GetAverageColorOfArea<TColor>(int left, int top, int right, int bottom) where TColor : struct, IColor
    {
        using var screenshot = GetScreenshot(left, top, right, bottom)
             .ToImage<TColor, byte>();

        var averageColorOfArea = screenshot.GetAverage();

        return new AverageColor((int)averageColorOfArea.MCvScalar.V0, (int)averageColorOfArea.MCvScalar.V1, (int)averageColorOfArea.MCvScalar.V2);
    }

    public  async Task<AverageColor> WaitAverageColorOfAreaAsync<TColor>(int left, int top, int right, int bottom, string sumString, TimeSpan timeForWait, TimeSpan attemptsPeriodic) where TColor : struct, IColor
    {
        var currentTime = DateTime.UtcNow;

        var endTime = currentTime.Add(timeForWait);

        var lastCheckTime = currentTime;

        while (lastCheckTime < endTime)
        {
            var averageColor = GetAverageColorOfArea<TColor>(left, top, right, bottom);

            if (averageColor.SumString == sumString)
                return averageColor;

            lastCheckTime = DateTime.UtcNow;

            await Task.Delay(attemptsPeriodic);
        }

        return null;
    }


    public ColorCode GetMainColorOfArea(int left, int top, int right, int bottom)
    {
        using var screenshot = GetScreenshot(left, top, right, bottom);

        var averageColor = GetAverageColorOfArea<Rgb>(left, top, right, bottom);

        return averageColor.PriorityColor;
    }

    public byte[] GetHashOfArea(int left, int top, int right, int bottom)
    {
        using var screnshot = GetScreenshot(left, top, right, bottom);

        byte[] bytes = null;

        using var memoryStream = new MemoryStream();

        screnshot.Save(memoryStream, ImageFormat.Bmp);

        bytes = memoryStream.ToArray();

        return MD5.HashData(bytes);
    }

    public string RecognizeText<TColor>(RecognizeLanguage language, int left, int top, int right, int bottom) where TColor : struct, IColor
    {
        var tesseract = language switch
        {
            RecognizeLanguage.Ru => _tessetactRu,
            RecognizeLanguage.En => _tessetactEn,
            _ => throw new NotImplementedException(),
        };

        using var textImage = GetScreenshot(left, top, right, bottom)
          .ToImage<TColor, byte>();

        tesseract.SetImage(textImage);

        tesseract.Recognize();

        return tesseract.GetUTF8Text();
    }

    public void LoadObjectImages(string imagesDirectory)
    {
        if (!Directory.Exists(imagesDirectory))
            Directory.CreateDirectory(imagesDirectory);

        var files = Directory.GetFiles(imagesDirectory);

        if (files.Length == 0)
            throw new FileNotFoundException($"В папке {imagesDirectory} нет ни одного изображения");

        foreach (var file in files)
        {
            var filneme = file.Replace($"{imagesDirectory}\\", "");
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), file);
            var bitmap = new Bitmap(filePath);

            _objectImages.Add(filneme, bitmap);
        }
    }
}
