using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using StellarisMusicAdder.Services;

namespace StellarisMusicAdder.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly ConverterService _converterService = new();
    private readonly StellarisCommitService _commitService = new(); // Add the new service
    private string _outputFolder = string.Empty;
    private string _stellarisMusicPath = string.Empty; // Add property for Stellaris path
    private string _statusText = "Ready";

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> MusicFiles { get; } = new();

    public string OutputFolder
    {
        get => _outputFolder;
        set
        {
            _outputFolder = value;
            OnPropertyChanged(nameof(OutputFolder));
        }
    }

    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            OnPropertyChanged(nameof(StatusText));
        }
    }

    // --- New Property for Stellaris Music Path ---
    public string StellarisMusicPath
    {
        get => _stellarisMusicPath;
        set
        {
            _stellarisMusicPath = value;
            OnPropertyChanged(nameof(StellarisMusicPath));
        }
    }

    // --- Commands ---
    public ICommand AddFilesCommand { get; }
    public ICommand BrowseOutputCommand { get; }
    public ICommand ConvertAllCommand { get; }
    public ICommand ClearListCommand { get; }
    public ICommand BrowseStellarisPathCommand { get; } // New Command
    public ICommand CommitToStellarisCommand { get; }   // New Command

    public MainViewModel()
    {
        // Use a default output folder for convenience
        OutputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StellarisConvertedMusic");

        // Set a default suggestion for the Stellaris path
        string steamPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Steam\steamapps\common\Stellaris\music");
        if (Directory.Exists(steamPath))
        {
            StellarisMusicPath = steamPath;
        }

        // Initialize commands
        AddFilesCommand = new RelayCommand(_ => SelectFiles());
        BrowseOutputCommand = new RelayCommand(_ => SelectOutputFolder());
        ConvertAllCommand = new RelayCommand(async _ => await ConvertAllFilesAsync(), _ => CanConvert());
        ClearListCommand = new RelayCommand(_ => MusicFiles.Clear());

        // --- Initialize New Commands ---
        BrowseStellarisPathCommand = new RelayCommand(_ => SelectStellarisMusicFolder());
        CommitToStellarisCommand = new RelayCommand(async _ => await CommitFilesAsync(), _ => CanCommit());
    }

    private void SelectFiles()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Music Files",
            Filter = "All Supported Files|*.mp3;*.wav;*.flac;*.m4a;*.mp4|All Files|*.*",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() == true)
        {
            foreach (var file in openFileDialog.FileNames)
            {
                if (!MusicFiles.Contains(file))
                {
                    MusicFiles.Add(file);
                }
            }
        }
    }

    private void SelectOutputFolder()
    {
        // This is a common trick to use the modern "File Open" dialog for selecting folders.
        var dialog = new OpenFileDialog
        {
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true,
            // Set a dummy filename
            FileName = "Folder Selection"
        };

        if (dialog.ShowDialog() == true)
        {
            // Get the directory path from the dummy filename
            string? folderPath = Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                OutputFolder = folderPath;
            }
        }
    }

    private async Task ConvertAllFilesAsync()
    {
        if (!CanConvert()) return;

        Directory.CreateDirectory(OutputFolder);

        StatusText = "Converting...";
        var totalFiles = MusicFiles.Count;
        var processedCount = 0;

        // Create a list of all the conversion tasks that will run in parallel
        var conversionTasks = MusicFiles.Select(async file =>
        {
            try
            {
                await _converterService.ConvertToOggAsync(file, OutputFolder);

                // This is a thread-safe way to increment the counter from multiple tasks
                int currentProgress = Interlocked.Increment(ref processedCount);

                // Update the UI from the background thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusText = $"Converting... {currentProgress}/{totalFiles}";
                });
            }
            catch (Exception ex)
            {
                // If one file fails, show an error but let others continue
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Failed to convert {Path.GetFileName(file)}.\nError: {ex.Message}", "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }).ToList();

        // Now, wait for ALL of the started tasks to complete
        await Task.WhenAll(conversionTasks);

        // This code will only run after every conversion is finished
        StatusText = $"Conversion complete! {processedCount} of {totalFiles} files processed.";
        MessageBox.Show($"Successfully processed {processedCount} files.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private bool CanConvert()
    {
        return MusicFiles.Count > 0 && !string.IsNullOrWhiteSpace(OutputFolder);
    }

    // --- New Methods ---
    private void SelectStellarisMusicFolder()
    {
        var dialog = new OpenFileDialog
        {
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Folder Selection"
        };

        if (dialog.ShowDialog() == true)
        {
            string? folderPath = Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                StellarisMusicPath = folderPath;
            }
        }
    }

    private async Task CommitFilesAsync()
    {
        if (!CanCommit()) return;

        StatusText = "Committing files to Stellaris...";
        var (songCount, error) = await _commitService.GenerateStellarisFilesAsync(OutputFolder, StellarisMusicPath);

        if (!string.IsNullOrEmpty(error))
        {
            StatusText = "Commit failed.";
            MessageBox.Show(error, "Commit Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            StatusText = $"Successfully committed {songCount} songs!";
            MessageBox.Show($"Successfully generated song assets for {songCount} unique tracks in:\n{StellarisMusicPath}", "Commit Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private bool CanCommit()
    {
        // Can only commit if the Stellaris path is set and there are .ogg files in the output folder.
        return !string.IsNullOrWhiteSpace(StellarisMusicPath)
            && Directory.Exists(OutputFolder)
            && Directory.EnumerateFiles(OutputFolder, "*.ogg").Any();
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}