//using CorrosionModels;
using Microsoft.Win32;
using SharpDX.Collections;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace PACMAN
{
    /// <summary>
    /// Interaction logic for SingleAnalysis.xaml
    /// </summary>
    public partial class SingleAnalysis : Window
    {
        public int locationValue = 0;
        public string locationString = "";
        public bool canceled = true;
        public string selectedFileName { get; set; }

        private Dictionary<string, int> locationValues = new Dictionary<string, int>();

        public SingleAnalysis(List<t_Location> locations)
        {
            InitializeComponent();
            locationValues.Add("This Location", Convert.ToInt32(ConfigurationManager.AppSettings["Location"]));
            foreach (var location in locations)
            {
                locationValues.Add(location.Name, location.Id);
                locationSelect.Items.Add(location.Name);
            }
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "CSV Files (.csv)|*.csv|Text Files (.txt)|*.txt";
            openFileDialog1.Title = "Select an Input File";
            if (openFileDialog1.ShowDialog() == true)
            {
                selectedFileName = openFileDialog1.FileName;
                SelectedFileText.Text = selectedFileName;
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFileName == "" || selectedFileName == null) {
                MessageBoxResult result = MessageBox.Show("Please select a file", "Input Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else{
                string selectedValue = locationSelect.SelectedItem as string;
                canceled = false;
                if (selectedValue == null || selectedValue == "")
                {
                    MessageBoxResult result = MessageBox.Show("Please select a location", "Input Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    locationValue = locationValues[selectedValue];
                    locationString = selectedValue;                   
                    Close();
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            canceled = true;
            Close();
        }
    }
}
