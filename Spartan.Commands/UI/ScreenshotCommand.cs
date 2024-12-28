using SkiaSharp;
using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.UI;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.UI;

namespace Spartan.Commands.UI;

public class ScreenshotCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var screenshotRequest = (ScreenshotCommandRequestModel)request;

        var screenshotPath = Path.Combine(screenshotRequest.LocalDestinationDirectory,
            $"Screenshot {DateTime.Now:yyyy-MM-dd HH-mm-ss}.png");

        return new ScreenshotCommandResponseModel
        {
            Command = screenshotRequest.Command,
            LocalDestinationPath = screenshotPath,
            FileContents = CaptureScreenshotAsPngChunks()!,
            Output =
                $"Saved screenshot to {screenshotPath} (local)."
        };
    }

    private static List<byte[]>? CaptureScreenshotAsPngChunks(int chunkSize = int.MaxValue)
    {
        try
        {
            // Get the screen dimensions (this is platform-dependent)
            const int screenWidth = 1920; // Replace with dynamic screen width
            const int screenHeight = 1080; // Replace with dynamic screen height

            using var bitmap = new SKBitmap(screenWidth, screenHeight);
            using var canvas = new SKCanvas(bitmap);

            // Capture the screen (platform-specific implementation required here)
            // Placeholder: Fill canvas with a solid color
            canvas.Clear(SKColors.LightBlue);

            // Encode the bitmap as a PNG in memory
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            // Chunk the data into a list of byte arrays
            var pngBytes = data.ToArray();
            return ChunkByteArray(pngBytes, chunkSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error capturing screenshot: {ex.Message}");
            return null;
        }
    }

    private static List<byte[]>? ChunkByteArray(byte[] source, int chunkSize)
    {
        var result = new List<byte[]>();
        var totalBytes = source.Length;
        var offset = 0;

        while (offset < totalBytes)
        {
            var currentChunkSize = Math.Min(chunkSize, totalBytes - offset);
            var chunk = new byte[currentChunkSize];
            Array.Copy(source, offset, chunk, 0, currentChunkSize);
            result.Add(chunk);
            offset += currentChunkSize;
        }

        return result;
    }
}