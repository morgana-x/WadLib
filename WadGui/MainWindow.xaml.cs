using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using WadLib;
namespace WadGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            {
                listBox.SelectAll();
            }
        }
        private Wad currentWad = null;
        private List<string> selectedItems = new List<string>();
        bool Processing = false;
        private void RefreshList(string search)
        {
            if (currentWad == null)
            {
                return;
            }
            List<WadFileEntry> foundEntrys = new();
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
            //listBox.MaxHeight = foundEntrys.Count * 30;
            foreach(var  entry in foundEntrys)
            {
                listBox.Items.Add(entry.FileName);
            }
            
        }

        private void Button_Click_Extract_Selected(object sender, RoutedEventArgs e)
        {
            if (currentWad == null || Processing)
            {
                return;
            }
            OpenFolderDialog openFileDialog = new OpenFolderDialog();
            openFileDialog.Title = "Select the Directory to extact to";
            openFileDialog.ShowDialog();
            string selectedOutFolder = openFileDialog.FolderName;
            Processing = true;
            ProgressLabel.Background = Brushes.Yellow; //.SetCurrentValue(BackgroundProperty,// "#ebe834");
            ProgressLabel.Content = "Extracting " + selectedItems.Count + " items...";
            //ProgressLabel.Background.SetValue(BackgroundProperty, 2);
            foreach (var item in selectedItems)
            {
                currentWad.ExtractFile(item, selectedOutFolder);
            }
            Processing = false;
            ProgressLabel.Background = Brushes.Green;
            ProgressLabel.Content = "Succesfully extracted " + selectedItems.Count + " items...";
        }
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentWad == null)
            {
                return;
            }
            
            RefreshList(Textbox.Text);
        }
        private void Button_Click_Extract_All(object sender, RoutedEventArgs e)
        {
            if (currentWad == null || Processing)
            {
                return;
            }
            OpenFolderDialog openFileDialog = new OpenFolderDialog();
            openFileDialog.Title = "Select the Directory to extact ALL files to";
            openFileDialog.ShowDialog();
            string selectedOutFolder = openFileDialog.FolderName;
            ProgressLabel.Background = Brushes.Yellow;
            ProgressLabel.Content = "Extracting " + currentWad.FileEntries.Count + " items...";
            Processing = true;
            currentWad.ExtractAllFiles(selectedOutFolder);
            Processing = false;
            ProgressLabel.Background = Brushes.Green;
            ProgressLabel.Content = "Succesfully Extracted " + currentWad.FileEntries.Count + " items...";
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedItems.Clear();
            foreach (var item in listBox.SelectedItems)
            {
                selectedItems.Add(item.ToString());
            }
            //selectedItems = (List<string>)listBox.SelectedItems.Cast<string>();
        }
        private void Button_Click_Open_Wad(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Select the WAD file";
            openFileDialog.DefaultExt = ".wad";
            openFileDialog.CheckFileExists = true;
            openFileDialog.ShowDialog();
            if (currentWad != null)
            {
                currentWad.Dispose();
                currentWad = null;
            }
            
            currentWad = new Wad(openFileDialog.FileName);
            OpenWadButton.Content = System.IO.Path.GetFileName(openFileDialog.FileName);
            RefreshList(string.Empty);
        }

        private void Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentWad == null)
            {
                return;
            }
            
        }
    }
}