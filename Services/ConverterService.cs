using FFMpegCore;
using System.IO;
using System.Threading.Tasks;

namespace StellarisMusicAdder.Services;

public class ConverterService
{
    public async Task ConvertToOggAsync(string inputFile, string outputFolder)
    {
        string outputFileName = Path.GetFileNameWithoutExtension(inputFile) + ".ogg";
        string outputFile = Path.Combine(outputFolder, outputFileName);

        await FFMpegArguments
            .FromFileInput(inputFile)
            .OutputToFile(outputFile, overwrite: true, options => options
                .WithAudioCodec("libvorbis") // Specify the ogg vorbis codec
                .WithCustomArgument("-q:a 5")) // Use this instead of WithAudioQuality
            .ProcessAsynchronously();
    }
}