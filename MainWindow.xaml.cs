using System.Windows;
using StellarisMusicAdder.ViewModels;

namespace StellarisMusicAdder;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Note: We're dropping a string array of file paths
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Access the ViewModel and add the files
            if (DataContext is MainViewModel viewModel)
            {
                foreach (var file in files)
                {
                    if (!viewModel.MusicFiles.Contains(file))
                    {
                        viewModel.MusicFiles.Add(file);
                    }
                }
            }
        }
    }
}