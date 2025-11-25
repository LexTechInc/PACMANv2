using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Interaction logic for AddLocation.xaml
    /// </summary>
    public partial class AddLocation : Window
    {
        public bool canceled = true;

        public bool isGridValid = false;

        public List<PollutionModel> gridResults = new List<PollutionModel>();

        public string LocationName = "";

        public AddLocation()
        {
            InitializeComponent();

            List<GridPollutionModel> pollutionModels = new List<GridPollutionModel>();

            GridPollutionModel pmJan = new GridPollutionModel();
            pmJan.MonthName = "January";
            pmJan.Month = 1;
            pollutionModels.Add(pmJan);

            GridPollutionModel pmFeb = new GridPollutionModel();
            pmFeb.MonthName = "February";
            pmFeb.Month = 2;
            pollutionModels.Add(pmFeb);

            GridPollutionModel pmMar = new GridPollutionModel();
            pmMar.MonthName = "March";
            pmMar.Month = 3;
            pollutionModels.Add(pmMar);

            GridPollutionModel pmApr = new GridPollutionModel();
            pmApr.MonthName = "April";
            pmApr.Month = 4;
            pollutionModels.Add(pmApr);

            GridPollutionModel pmMay = new GridPollutionModel();
            pmMay.MonthName = "May";
            pmMay.Month = 5;
            pollutionModels.Add(pmMay);

            GridPollutionModel pmJun = new GridPollutionModel();
            pmJun.MonthName = "June";
            pmJun.Month = 6;
            pollutionModels.Add(pmJun);

            GridPollutionModel pmJul = new GridPollutionModel();
            pmJul.MonthName = "July";
            pmJul.Month = 7;
            pollutionModels.Add(pmJul);

            GridPollutionModel pmAug = new GridPollutionModel();
            pmAug.MonthName = "August";
            pmAug.Month = 8;
            pollutionModels.Add(pmAug);

            GridPollutionModel pmSep = new GridPollutionModel();
            pmSep.MonthName = "September";
            pmSep.Month = 9;
            pollutionModels.Add(pmSep);

            GridPollutionModel pmOct = new GridPollutionModel();
            pmOct.MonthName = "October";
            pmOct.Month = 10;
            pollutionModels.Add(pmOct);

            GridPollutionModel pmNov = new GridPollutionModel();
            pmNov.MonthName = "November";
            pmNov.Month = 11;
            pollutionModels.Add(pmNov);

            GridPollutionModel pmDec = new GridPollutionModel();
            pmDec.MonthName = "December";
            pmDec.Month = 12;
            pollutionModels.Add(pmDec);

            this.newLocationGrid.ItemsSource = pollutionModels;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            List<GridPollutionModel> gridList = new List<GridPollutionModel>((newLocationGrid.ItemsSource as IList).OfType<GridPollutionModel>());
            bool isValid = true;
            ErrorLabel.Visibility = Visibility.Hidden;

            foreach (GridPollutionModel item in gridList)
            {
                if (item.Deposit == null || item.Sea == null || item.AmmNit == null || item.AmmSul == null || item.RainDep == null || item.RainConc == null || item.NaCl == null
                    || item.MgCl2 == null || item.Na2S04 == null || item.CaC12 == null || item.KCI == null || item.MgSO4 == null || item.K2SO4 == null || item.CaSO4 == null
                    || item.total == null || item.HSMass == null || item.HNMass == null)
                {
                    isValid = false;
                    ErrorLabel.Content = "No value in data grid can be blank";
                    ErrorLabel.Visibility = Visibility.Visible;
                    break;
                }
            }

            if (isValid)
            {
                if(newLocationName.Text == null || String.IsNullOrEmpty(newLocationName.Text))
                {
                    isValid = false;
                    ErrorLabel.Content = "New Location Name cannot be blank";
                    ErrorLabel.Visibility = Visibility.Visible;
                }
            }

            if (isValid)
            {
                
                foreach (GridPollutionModel item in gridList)
                {
                    PollutionModel model = new PollutionModel();
                    model.Deposit = (double)item.Deposit;
                    model.Sea = (double)item.Sea;
                    model.AmmNit = (double)item.AmmNit;
                    model.AmmSul = (double)item.AmmSul;
                    model.RainDep = (double)item.RainDep;
                    model.RainConc = (double)item.RainConc;
                    model.NaCl = (double)item.NaCl;
                    model.MgCl2 = (double)item.MgCl2;
                    model.Na2S04 = (double)item.Na2S04;
                    model.CaC12 = (double)item.CaC12;
                    model.KCI = (double)item.KCI;
                    model.MgSO4 = (double)item.MgSO4;
                    model.K2SO4 = (double)item.K2SO4;
                    model.CaSO4 = (double)item.CaSO4;
                    model.total = (double)item.total;
                    model.HSMass = item.HSMass;
                    model.HNMass = item.HNMass;
                    model.Month = item.Month;

                    gridResults.Add(model);
                }

                LocationName = newLocationName.Text;
                canceled = false;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            canceled = true;
            Close();
        }

    }
}
