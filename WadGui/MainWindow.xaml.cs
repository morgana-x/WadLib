using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using WadLib;
namespace WadGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Wad currentWad = null;
        private List<string> selectedItems = new List<string>();
        bool Processing = false;

        public MainWindow()
        {
            InitializeComponent();
            Textbox.TextChanged += TextChanged;
            listBox.SelectionMode = SelectionMode.Multiple;
            this.KeyDown += keyDownInput;
        }
        private void keyDownInput(object sender,KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.A)) 
                listBox.SelectAll();
        }

        private void RefreshList(string search)
        {
            if (currentWad == null) return;
 
            List<WadFile> foundEntrys = new();
            foreach (var entry in currentWad.FileEntries)
            {
                if (search == string.Empty)
                {
                    foundEntrys.Add(entry);
                    continue;
                }
                if (entry.FileName.StartsWith(search))
                {
                    foundEntrys.Add(entry);
                    continue;
                }
                else if(entry.FileName.EndsWith(search))
                {
                    foundEntrys.Add(entry);
                    continue;
                }
                else if (entry.FileName.Contains(search))
                {
                    foundEntrys.Add(entry);
                    continue;
                }
                
            }
            listBox.Items.Clear();

            foreach(var  entry in foundEntrys)
                listBox.Items.Add(entry.FileName);
            
        }

        private void Button_Click_Extract_Selected(object sender, RoutedEventArgs e)
        {
            if (currentWad == null || Processing) return;

            OpenFolderDialog openFileDialog = new OpenFolderDialog();
            openFileDialog.Title = "Select the Directory to extact to";
            openFileDialog.ShowDialog();

            ProgressLabel.Background = Brushes.Yellow;
            ProgressLabel.Content = "Extracting " + selectedItems.Count + " items...";

            Processing = true;
            foreach (var item in selectedItems)
                currentWad.ExtractFile(item, openFileDialog.FolderName);
            Processing = false;

            ProgressLabel.Background = Brushes.Green;
            ProgressLabel.Content = "Succesfully extracted " + selectedItems.Count + " items...";
        }
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentWad != null)
                RefreshList(Textbox.Text);
        }
        private void Button_Click_Extract_All(object sender, RoutedEventArgs e)
        {
            if (currentWad == null || Processing) return;
            OpenFolderDialog openFileDialog = new OpenFolderDialog() { Title = "Select the Directory to extact ALL files to" };
            openFileDialog.ShowDialog();

            ProgressLabel.Background = Brushes.Yellow;
            ProgressLabel.Content = "Extracting " + currentWad.FileEntries.Count + " items...";

            Processing = true;
            currentWad.ExtractAllFiles(openFileDialog.FolderName);
            Processing = false;

            ProgressLabel.Background = Brushes.Green;
            ProgressLabel.Content = "Succesfully Extracted " + currentWad.FileEntries.Count + " items...";
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedItems.Clear();
            foreach(var itm in listBox.SelectedItems) selectedItems.Add(itm.ToString());
        }
        private void Button_Click_Open_Wad(object sender, RoutedEventArgs e)
        {
            if (Processing) return;

            OpenFileDialog openFileDialog = new OpenFileDialog() { 
                Multiselect=false, CheckFileExists=true,
                DefaultExt=".wad", Title = "Select the WAD file"
            };
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName == string.Empty) return;
            if (!Wad.IsWad(openFileDialog.FileName)) return;

            if (currentWad != null)
            {
                currentWad.Dispose();
                currentWad = null;
            }
            
            currentWad = new Wad(openFileDialog.FileName);
            OpenWadButton.Content = System.IO.Path.GetFileName(openFileDialog.FileName);
            RefreshList(string.Empty);
        }
    }
}