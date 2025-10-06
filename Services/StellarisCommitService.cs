using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisMusicAdder.Services;

public class StellarisCommitService
{
    public async Task<(int songCount, string error)> GenerateStellarisFilesAsync(string oggSourceFolder, string stellarisMusicFolder)
    {
        try
        {
            // Find all .ogg files in the source folder
            if (!Directory.Exists(oggSourceFolder))
            {
                return (0, "The specified output folder does not exist.");
            }

            var oggFiles = Directory.GetFiles(oggSourceFolder, "*.ogg");

            if (oggFiles.Length == 0)
            {
                return (0, "No .ogg files found in the output folder to commit.");
            }

            // Use a HashSet to automatically get a unique list of song names without the extension
            var uniqueSongNames = oggFiles.Select(Path.GetFileNameWithoutExtension).ToHashSet();

            // Define the paths for the output files
            string assetFilePath = Path.Combine(stellarisMusicFolder, "songs.asset");
            string txtFilePath = Path.Combine(stellarisMusicFolder, "songs.txt");

            // --- Build the songs.asset file content ---
            var assetContent = new StringBuilder();
            foreach (var songName in uniqueSongNames)
            {
                assetContent.AppendLine("music = {");
                assetContent.AppendLine($"\tname = \"{songName}\"");
                assetContent.AppendLine($"\tfile = \"{songName}.ogg\"");
                assetContent.AppendLine("\tvolume = 0.80");
                assetContent.AppendLine("}");
            }

            // --- Build the songs.txt file content ---
            var txtContent = new StringBuilder();
            foreach (var songName in uniqueSongNames)
            {
                txtContent.AppendLine("song = {");
                txtContent.AppendLine($"\tname = \"{songName}\"");
                txtContent.AppendLine("}");
            }

            // Write both files asynchronously, overwriting if they exist
            await File.WriteAllTextAsync(assetFilePath, assetContent.ToString());
            await File.WriteAllTextAsync(txtFilePath, txtContent.ToString());

            return (uniqueSongNames.Count, string.Empty);
        }
        catch (System.Exception ex)
        {
            // Return a meaningful error message
            return (0, $"An error occurred: {ex.Message}");
        }
    }
}