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
    /// Interaction logic for TimespanAnalysis.xaml
    /// </summary>
    /// 

    public partial class TimespanAnalysis : Window
    {
        public bool canceled = true;

        public bool entireRange = false;
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public int location { get; set; }

        public string locationString { get; set; }

        private Dictionary<string, int> locationValues = new Dictionary<string, int>();

        public TimespanAnalysis(List <t_Location> locations)
        {
            InitializeComponent();
            locationValues.Add("This Location", Convert.ToInt32(ConfigurationManager.AppSettings["Location"]));
            locationSelect.Items.Add("This Location");
            locationSelect.SelectedValue = "This Location";
            foreach (var location in locations)
            {
                locationValues.Add(location.Name, location.Id);
                locationSelect.Items.Add(location.Name);
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            if (entireRange)
            {

            }
            else {
                if (StartDate.SelectedDate != null)
                {
                    start = (DateTime)StartDate.SelectedDate;
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Please select a start date", "Input Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    valid = false;
                }
                if (valid == true)
                {
                    if (EndDate.SelectedDate != null)
                    {
                        end = (DateTime)EndDate.SelectedDate;
                        //end.AddDays(1);
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show("Please select an end date", "Input Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        valid = false;
                    }
                }
            }
            if (valid == true)
            {
                string selectedValue = locationSelect.SelectedItem as string;
                if (selectedValue == null || selectedValue == "")
                {
                    MessageBoxResult result = MessageBox.Show("Please select a location", "Input Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    valid = false;
                }
                else
                {
                    location = locationValues[selectedValue];
                    locationString = selectedValue;
                }
            }
            if (valid == true)
            {
                canceled = false;
                Close();
            }              
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            canceled = true;
            Close();
        }

        private void EntireRangeCB_Checked(object sender, RoutedEventArgs e)
        {
            entireRange = !entireRange;
            StartDate.IsEnabled = !StartDate.IsEnabled;
            EndDate.IsEnabled = !EndDate.IsEnabled;
        }
    }
}
