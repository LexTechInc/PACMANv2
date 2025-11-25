using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PACMAN
{
    public class CorrosionData
    {
        public string fileTitle { get; set; }
        public List<double> Temperature { get; set; }
        public int LargestPitValue { get; set; }
        public List<double> RelativeHumidity { get; set; }
        public List<double> Precipitation { get; set; }
        public List<double> TimeofWetness { get; set; }
        public List<double> SurfacePollutants { get; set; }
        public List<double> Droplets { get; set; }
        public List<DateTime> Y { get; set; }
        public List<DateTime> PrecipitationDates { get; set; }
        public List<DateTime> TOWDates { get; set; }
        public List<DateTime> PollutantDates { get; set; }

        public List<int> DropletsY { get; set; }

        public List<double> PitDiameter { get; set; }

        public List<double> PitCount { get; set; }

        public List<double> SteelWeightLoss { get; set; }

        public List<double> SteelLossTime { get; set; }

        public int[] cutoffValues { get; set; }

        public int[] cutoffValuesDroplets { get; set; }

        public string[] BaseNames { get; set; }

        public string AircraftTail { get; set; }
        public CorrosionDataPoint pointObject { get; set; }
        public IEnumerable<CorrosionDataPoint> pointObjectList { get; set; }
        public List<CorrosionDataPoint> TemperaturePoints { get; set; }
        public List<CorrosionDataPoint> WetnessPoints { get; set; }
        public List<CorrosionDataPoint> DropletPoints { get; set; }
        public List<CorrosionDataPoint> HumidityPoints { get; set; }
        public List<CorrosionDataPoint> PrecipitationPoints { get; set; }
        public List<CorrosionDataPoint> PollutantPoints { get; set; }
        public List<CorrosionDataPoint> PitPoints { get; set; }
        public List<CorrosionDataPoint> SteelLossPoints { get; set; }

        public List<CorrosionDataPoint> Corr1Values { get; set; }

        public List<CorrosionDataPoint> Corr2Values { get; set; }

        public CorrosionData()
        {
            PitDiameter = new List<double>();
            PitCount = new List<double>();
            SteelWeightLoss = new List<double>();
            SteelLossTime = new List<double>();
            pointObjectList = new List<CorrosionDataPoint>();
            PrecipitationDates = new List<DateTime>();
            TOWDates = new List<DateTime>();
            PollutantDates = new List<DateTime>();
            SurfacePollutants = new List<double>();
            Droplets = new List<double>();
            DropletsY = new List<int>();
            Precipitation = new List<double>();
            TimeofWetness = new List<double>();
            Temperature = new List<double>();
            RelativeHumidity = new List<double>();
            Y = new List<DateTime>();
        }

        public List<CorrosionDataPoint> getTemperaturePoints()
        {
            List<CorrosionDataPoint> cdpList = new List<CorrosionDataPoint>();
            int i = 0;
            int cutOffIndex = 0;
            using (var e1 = Temperature.GetEnumerator())
            using (var e2 = Y.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    CorrosionDataPoint cdp = new CorrosionDataPoint();
                    cdp.Value = e1.Current;
                    cdp.Y = i + 1;
                    cdp.Date = e2.Current;
                    cdp.Name = "Temperature";
                    if (i < cutoffValues[cutOffIndex])
                    {
                        cdp.Group = BaseNames[cutOffIndex];
                    }
                    else if (cutOffIndex < cutoffValues.Length - 1)
                    {
                        cutOffIndex++;
                        cdp.Group = BaseNames[cutOffIndex];
                    }
                    cdpList.Add(cdp);
                    i++;
                }
            }
            cdpList.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
            return cdpList;
        }

        public List<CorrosionDataPoint> getRelativeHumidityPoints()
        {
            List<CorrosionDataPoint> cdpList = new List<CorrosionDataPoint>();
            int i = 0;
            int cutOffIndex = 0;
            using (var e1 = RelativeHumidity.GetEnumerator())
            using (var e2 = Y.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    CorrosionDataPoint cdp = new CorrosionDataPoint();
                    cdp.Value = e1.Current;
                    cdp.Y = i + 1;
                    cdp.Date = e2.Current;
                    cdp.Name = "Relative Humidity";
                    if (i < cutoffValues[cutOffIndex])
                    {
                        cdp.Group = BaseNames[cutOffIndex];
                    }
                    else if (cutOffIndex < cutoffValues.Length - 1)
                    {
                        cutOffIndex++;
                        cdp.Group = BaseNames[cutOffIndex];
                    }
                    cdpList.Add(cdp);
                    i++;
                }
            }
            cdpList.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
            return cdpList;
        }



        public List<CorrosionDataPoint> getPrecipitationPoints()
        {
            List<CorrosionDataPoint> cdpList = new List<CorrosionDataPoint>();
            int i = 0;
            int cutOffIndex = 0;
            using (var e1 = Precipitation.GetEnumerator())
            using (var e2 = PrecipitationDates.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    CorrosionDataPoint cdp = new CorrosionDataPoint();
                    cdp.Value = e1.Current;
                    cdp.Y = i + 1;
                    cdp.Date = e2.Current;
                    cdp.Name = "Precipitation";
                    if (i < cutoffValues[cutOffIndex])
                    {
                        cdp.Group = BaseNames[cutOffIndex];
                    }
                    else if (cutOffIndex < cutoffValues.Length - 1)
                    {
                        cutOffIndex++;
                        cdp.Group = BaseNames[cutOffIndex];
                    }
                    cdpList.Add(cdp);
                    i++;
                }
            }
            cdpList.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
            return cdpList;
        }


        public List<CorrosionDataPoint> getTimeofWetnessPoints()
        {
            List<CorrosionDataPoint> cdpList = new List<CorrosionDataPoint>();
            int i = 0;
            int cutOffIndex = 0;
            using (var e1 = TimeofWetness.GetEnumerator())
            using (var e2 = TOWDates.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    CorrosionDataPoint cdp = new CorrosionDataPoint();
                    cdp.Value = e1.Current;
                    cdp.Y = i + 1;
                    cdp.Date = e2.Current;
                    cdp.Name = "Time of Wetness";
                    if (i < cutoffValues[cutOffIndex])
                    {
                        cdp.Group = BaseNames[cutOffIndex];
                    }
                    else if (cutOffIndex < cutoffValues.Length - 1)
                    {
                        cutOffIndex++;
                        cdp.Group = BaseNames[cutOffIndex];
                    }
                    cdpList.Add(cdp);
                    i++;
                }
            }
            cdpList.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
            return cdpList;
        }

        public List<CorrosionDataPoint> getSurfacePollutantsPoints()
        {
            List<CorrosionDataPoint> cdpList = new List<CorrosionDataPoint>();
            int i = 0;
            int cutOffIndex = 0;
            using (var e1 = SurfacePollutants.GetEnumerator())
            using (var e2 = PollutantDates.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    CorrosionDataPoint cdp = new CorrosionDataPoint();
                    cdp.Value = e1.Current;
                    cdp.Y = i + 1;
                    cdp.Date = e2.Current;
                    cdp.Name = "Surface Pollutants";
                    if (i < cutoffValues[cutOffIndex])
                    {
                        cdp.Group = BaseNames[cutOffIndex];
                    }
                    else if (cutOffIndex < cutoffValues.Length - 1)
                    {
                        cutOffIndex++;
                        cdp.Group = BaseNames[cutOffIndex];
                    }
                    cdpList.Add(cdp);
                    i++;
                }
            }
            cdpList.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
            return cdpList;
        }

        public List<CorrosionDataPoint> getDropletsPoints(bool addZeroPoint)
        {
            List<CorrosionDataPoint> cdpList = new List<CorrosionDataPoint>();

            int i = 0;
            int dropletIndex = 0;

            if (addZeroPoint)
            {
                CorrosionDataPoint cdpZeroPoint = new CorrosionDataPoint();
                cdpZeroPoint.Value = 0;
                cdpZeroPoint.Y = 0;
                cdpZeroPoint.Name = "Droplets";
                cdpList.Add(cdpZeroPoint);
            }

            using (var e1 = Droplets.GetEnumerator())
            using (var e2 = DropletsY.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    CorrosionDataPoint cdp = new CorrosionDataPoint();
                    cdp.Value = e1.Current;
                    cdp.Y = e2.Current;
                    cdp.Name = "Droplets";
                    if ((i <= cutoffValuesDroplets[dropletIndex] && cutoffValuesDroplets[dropletIndex] != 0) || (cutoffValuesDroplets[dropletIndex] == 0 && cutoffValuesDroplets.Length == 1) || cutoffValuesDroplets.Length - 1 == dropletIndex)
                    {
                        cdp.Group = BaseNames[dropletIndex];
                    }
                    else if (cutoffValuesDroplets[dropletIndex] == 0 && cutoffValuesDroplets.Length > dropletIndex)
                    {
                        while (cutoffValuesDroplets[dropletIndex] == 0 && cutoffValuesDroplets.Length - 1 > dropletIndex)
                        {
                            dropletIndex++;
                        }
                        cdp.Group = BaseNames[dropletIndex];
                    }
                    else if (dropletIndex < cutoffValuesDroplets.Length - 1)
                    {
                        dropletIndex++;
                        cdp.Group = BaseNames[dropletIndex];
                    }
                    cdpList.Add(cdp);
                    i++;
                }
            }
            return cdpList;
        }

        public List<CorrosionDataPoint> getPitPoints()
        {
            List<CorrosionDataPoint> cdpList = new List<CorrosionDataPoint>();
            using (var e1 = PitDiameter.GetEnumerator())
            using (var e2 = PitCount.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    CorrosionDataPoint cdp = new CorrosionDataPoint();
                    cdp.Value = e1.Current;
                    cdp.Series = e2.Current;
                    cdp.Name = "Pit Distribution";
                    cdpList.Add(cdp);
                }
            }
            return cdpList;
        }

        public List<CorrosionDataPoint> getSteelLossPoints()
        {
            List<CorrosionDataPoint> cdpList = new List<CorrosionDataPoint>();
            using (var e1 = SteelWeightLoss.GetEnumerator())
            using (var e2 = SteelLossTime.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    CorrosionDataPoint cdp = new CorrosionDataPoint();
                    cdp.Value = e1.Current;
                    cdp.Series = e2.Current;
                    cdp.Name = "Steel Weight Loss";
                    cdpList.Add(cdp);
                }
            }
            return cdpList;
        }

        public class BaseTimestamp
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string BaseName { get; set; }

            public BaseTimestamp()
            {

            }
        }
    }
}
