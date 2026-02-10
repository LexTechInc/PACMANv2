using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PACMAN
{
    public class DataObjects
    {
    }

    public class sensorData
    {
        public int col0 { get; set; }
        public double col1 { get; set; }
        public double col2 { get; set; }
        public int col3 { get; set; }
        public double col4 { get; set; }
        public double col5 { get; set; }
        public DateTime col6 { get; set; }
        public int Location { get; set; }
    }

    public class combinedData
    {
        public int Id { get; set; } // Database ID
        public int col0 { get; set; } //Record Number
        public double col1 { get; set; } //TOW Sensor
        public double col2 { get; set; } //TOW Calc
        public double col3 { get; set; } //Surface Temperature
        public int col4 { get; set; } //Time Interval
        public double col5 { get; set; } //ATM_RH
        public double col6 { get; set; } //ATM_Temp
        public double col7 { get; set; } //Rain
        public DateTime col8 { get; set; } // Timestamp
        public int col9 { get; set; } //Month
        public int col10 { get; set; } //Sensor = 1, Meterological = 2
        public int Location { get; set; }

        public double corr1 { get; set; }

        public double corr2 { get; set; }

        public double corrTemp { get; set;}
    }

    public class enviroP
    {
        ///public int period { get; set; }
        public double col0 { get; set; }
        public double col1 { get; set; }
        public double col2 { get; set; }
        public double col3 { get; set; }
        public double col4 { get; set; }
        public double col5 { get; set; }
        public double col6 { get; set; }
        public double col7 { get; set; }
        public double col8 { get; set; }
        public double col9 { get; set; }
        public double col10 { get; set; }
        public double col11 { get; set; }
        public double col12 { get; set; }
        public double col13 { get; set; }
        public double col14 { get; set; }
        public double col15 { get; set; }
        public double col16 { get; set; }
        public double col17 { get; set; }
    }

    public class nDrops
    {
        public double col0 { get; set; }
        public double col1 { get; set; }
        public double col2 { get; set; }
        public double col3 { get; set; }
        public int col4 { get; set; }
    }

    public class PollutionModel
    {
        //[Required(ErrorMessage = "Deposit is required.")]
        public double Deposit { get; set; }
        //[Required(ErrorMessage = "Min Salts is required.")]
        public double Sea { get; set; }
        //[Required(ErrorMessage = "NH4NO3 is required.")]
        public double AmmNit { get; set; }
        //[Required(ErrorMessage = "(NH4)2S04 is required.")]
        public double AmmSul { get; set; }
        //[Required(ErrorMessage = "pH is required.")]
        public double RainDep { get; set; }
        //[Required(ErrorMessage = "Conc is required.")]
        public double RainConc { get; set; }
        //[Required(ErrorMessage = "NaCl is required.")]
        public double NaCl { get; set; }
        //[Required(ErrorMessage = "MgC12 is required.")]
        public double MgCl2 { get; set; }
        //[Required(ErrorMessage = "Na2S04 is required.")]
        public double Na2S04 { get; set; }
        //[Required(ErrorMessage = "CaC12 is required.")]
        public double CaC12 { get; set; }
        //[Required(ErrorMessage = "KCI is required.")]
        public double KCI { get; set; }
        //[Required(ErrorMessage = "MgSO4 is required.")]
        public double MgSO4 { get; set; }
        //[Required(ErrorMessage = "K2SO4 is required.")]
        public double K2SO4 { get; set; }
        //[Required(ErrorMessage = "CaSO4 is required.")]
        public double CaSO4 { get; set; }
        //[Required(ErrorMessage = "Total is required.")]
        public double total { get; set; }
        //[Required(ErrorMessage = "H2SO4 is required.")]
        public double? HSMass { get; set; }
        //[Required(ErrorMessage = "HNO3 is required.")]
        public double? HNMass { get; set; }

        public int Location { get; set; }

        public string MonthName { get; set; }
        public int Month { get; set; }
    }

    public class GridPollutionModel
    {
        //[Required(ErrorMessage = "Deposit is required.")]
        public double? Deposit { get; set; }
        //[Required(ErrorMessage = "Min Salts is required.")]
        public double? Sea { get; set; }
        //[Required(ErrorMessage = "NH4NO3 is required.")]
        public double? AmmNit { get; set; }
        //[Required(ErrorMessage = "(NH4)2S04 is required.")]
        public double? AmmSul { get; set; }
        //[Required(ErrorMessage = "pH is required.")]
        public double? RainDep { get; set; }
        //[Required(ErrorMessage = "Conc is required.")]
        public double? RainConc { get; set; }
        //[Required(ErrorMessage = "NaCl is required.")]
        public double? NaCl { get; set; }
        //[Required(ErrorMessage = "MgC12 is required.")]
        public double? MgCl2 { get; set; }
        //[Required(ErrorMessage = "Na2S04 is required.")]
        public double? Na2S04 { get; set; }
        //[Required(ErrorMessage = "CaC12 is required.")]
        public double? CaC12 { get; set; }
        //[Required(ErrorMessage = "KCI is required.")]
        public double? KCI { get; set; }
        //[Required(ErrorMessage = "MgSO4 is required.")]
        public double? MgSO4 { get; set; }
        //[Required(ErrorMessage = "K2SO4 is required.")]
        public double? K2SO4 { get; set; }
        //[Required(ErrorMessage = "CaSO4 is required.")]
        public double? CaSO4 { get; set; }
        //[Required(ErrorMessage = "Total is required.")]
        public double? total { get; set; }
        //[Required(ErrorMessage = "H2SO4 is required.")]
        public double? HSMass { get; set; }
        //[Required(ErrorMessage = "HNO3 is required.")]
        public double? HNMass { get; set; }

        public int Location { get; set; }

        public string MonthName { get; set; }
        public int Month { get; set; }
    }

    public class CorrosionDataPoint
    {
        public DateTime Date { get; set; }
        public string DateString
        {
            //get {
            //    DateTime universalTime = Date.ToUniversalTime();
            //    return universalTime.ToString("MM/dd/yyyy:HH:mm");//"yyyy-MM-dd HH:mm" 
            //    //Date.ToUniversalTime().ToString(); 
            //}
            get { return Date.ToShortDateString(); }
        }
        public int Y { get; set; }
        public double Value { get; set; }
        public string Name { get; set; }
        public double Series { get; set; }
        public string Group { get; set; }

        public CorrosionDataPoint()
        {

        }
    }

    public class SensorDataViewModel
    {
        public int RecordNumber { get; set; }
        public double DSTOTimeOfWetness { get; set; }
        public double ThermistorTemp { get; set; }
        public double TimeInterval { get; set; }
        public double Humidity { get; set; }
        public double Temperature { get; set; }
        public double Rain { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class CombinedDataViewModel
    {
        public int ID { get; set; }
        public int Record_Number { get; set; }
        public double? TOWSensor { get; set; }
        public double? TOWCalc { get; set; }
        public double SurfaceTemp { get; set; }
        public int TimeInterval { get; set; }
        public double ATM_RH { get; set; }
        public double ATM_Temp { get; set; }
        public double Rain { get; set; }
        public DateTime Timestamp { get; set; }
        public int Month { get; set; }
        public int Location { get; set; }
        public int DataSourceType { get; set; }

        public double corr1 { get; set; }

        public double corr2 { get; set; }

        public double corrTemp { get; set;}
    }

    public enum MetDataTypes
    {
        Sensor,
        Meteorological
    }

    public class AnalysisTask
    {
        public int Id { get; set; }

        public int CrystalSize { get; set; }

        public bool DetergentWash { get; set; }

        public MetDataTypes MetDataType { get; set; }

        public int Location { get; set; }

        public string LocationString { get; set; }

        public int WashFreq { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        public bool Processed { get; set; }
        public string TailNumber { get; set; }

        public AnalysisTask()
        {

        }

        public AnalysisTask(AnalysisTask at)
        {
            this.Id = at.Id;
            this.CrystalSize = at.CrystalSize;
            this.DetergentWash = Convert.ToBoolean(at.DetergentWash);
            this.EndDate = at.EndDate;
            this.Location = at.Location;
            this.MetDataType = (MetDataTypes)at.MetDataType;
            this.Processed = Convert.ToBoolean(at.Processed);
            this.StartDate = at.StartDate;
            this.WashFreq = at.WashFreq;
            this.TailNumber = at.TailNumber;
        }


    }

    public static class ConstData
    {
        public static double[,] naClConst = {  {-5.718130111694336,0.4279699921607971,-0.003710000077262521},
                                        {0.2300499975681305,-0.005659999791532755,3.35000004270114e-05},
                                        {-0.003250000067055225,7.970000297063962e-05,-4.569999987324991e-07}};

        public static double[,] polyConst = {  {14.10694026947021, -4.402440071105957, 0.6257799863815308,-0.0340300016105175},
                                        {-0.351610004901886, 0.1268099993467331, -0.01429000031203032, 0.0001004959995043464},
                                        {0.005880000069737434, -0.001509999972768128, -0.0001035899986163713, 4.790559978573583e-05},
                                        {-4.620719846570864e-05, 2.534829945943784e-06, 4.960530077369185e-06, -9.018340278998949e-07}};

        public static double[,] PollFactors =
        {
            { 75,  31,  92,  33,  83, 62, 81, 90, 97, 36, 26, 97 },
            {47.4013, 9.21222,   -2067.26, 7.6418, -243.986, -71.7793, -167.063, 1815.54, -8015.24, 15.9934, 21.2367, -8015.24},
            {-1.3607,  -0.217089,  64.8411, -0.162965, 8.08717, 3.14066, 4.78475, -58.9797, 244.777, -0.41478, -0.4003, 244.777},
            {0.0161713, 0.00314646, -0.674292, 0.0026448, -0.0855822, -0.0380665, -0.0409448, 0.641416, -2.48871, 0.00583, 0.00429, -2.48871},
            {-7.30558e-05, -1.88592e-05, 0.00232606, -1.76983e-05, 0.000291099, 0.000138367, 9.79823e-05, -0.00233177, 0.00842463, -3.27004e-05, -2.40179e-05, 0.00842463},
        };
    }

    public class FleetAnalysisModel
    {
        public MultiTailDataObject[] data { get; set; }
        public string serializedData { get; set; }
        public string MDSs { get; set; }
        public string[] Tails { get; set; }
        public int[] PitValues { get; set; }
        public List<int> LargestPittValues { get; set; }
        public List<string> TailNames { get; set; }

        public FleetAnalysisModel()
        {
            LargestPittValues = new List<int>();
            TailNames = new List<string>();
        }

        public void generateData()
        {
            Random random = new Random();
            for (var i = 0; i < Tails.Length; i++)
            {
                MultiTailDataObject dO = new MultiTailDataObject
                {
                    Id = i,
                    MDS = MDSs,
                    TailNumber = Tails[i],
                    LocalizedCorrosion = (int)((250 / PitValues[i]) * 100),
                    FatigueDamage = (int)((250 / PitValues[i]) * 100)
                };

                data[i] = dO;
            }
            //System.Web.Script.Serialization.JavaScriptSerializer oSerializer =
            //new System.Web.Script.Serialization.JavaScriptSerializer();
            //serializedData = oSerializer.Serialize(data);
        }

        public void initilizeTails(List<string> items)
        {
            data = new MultiTailDataObject[items.Count];
            Tails = new string[items.Count];
            int index = 0;
            foreach (string item in items)
            {
                Tails[index] = item;
                index++;
            }
        }

        public void initilizePits(List<int> items)
        {
            data = new MultiTailDataObject[items.Count];
            PitValues = new int[items.Count];
            int index = 0;
            foreach (int item in items)
            {
                PitValues[index] = item;
                index++;
            }
        }

    }

    public class MultiTailDataObject
    {
        public int Id { get; set; }
        public string MDS { get; set; }
        public string TailNumber { get; set; }
        public int LocalizedCorrosion { get; set; }
        public int FatigueDamage { get; set; }
    }

    public class EnvironmentSelectObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public EnvironmentSelectObject(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
