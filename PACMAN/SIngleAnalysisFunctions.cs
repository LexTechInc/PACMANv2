using MathNet.Numerics.Distributions;
using RandomNumberGenerator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PACMAN
{
    public class SingleAnalysisFunctions
    {
        private Dictionary<string, PollutionModel> PollutionData;

        Random r = new Random();
        MathNet.Numerics.Distributions.Normal normalDist = new Normal(0, 1);

        public CorrosionData BeginAnalysisSingle(List<CombinedDataViewModel> fileData, int location, Dictionary<string, PollutionModel> pollutionData, bool createNewDamageCharts)
        {
            SimpleRNG.SetSeedFromSystemTime();
            CorrosionData cd = new CorrosionData();
            int numTasks = 1;
            cd.BaseNames = new string[numTasks];
            cd.cutoffValues = new int[numTasks];
            cd.cutoffValues[0] = fileData.Count + 1;
            cd.cutoffValuesDroplets = new int[numTasks];
            cd.AircraftTail = "Custom Tail";

            List<List<enviroP>> enviroP_List = new List<List<enviroP>>();
            List<combinedData> sensorData = new List<combinedData>();

            PollutionData = pollutionData;


            foreach (CombinedDataViewModel s in fileData)
            {
                combinedData sd = new combinedData();
                sd.Id = s.ID;
                sd.col0 = s.Record_Number;
                sd.col1 = (double)s.TOWSensor;
                sd.col2 = (double)s.TOWCalc;
                sd.col3 = s.SurfaceTemp;
                sd.col4 = s.TimeInterval;
                sd.col5 = s.ATM_RH;
                sd.col6 = s.ATM_Temp;
                sd.col7 = s.Rain;
                sd.col8 = s.Timestamp;
                sd.col9 = s.Month;
                if (location == 0)
                {
                    sd.Location = Convert.ToInt32(ConfigurationManager.AppSettings["Location"]);
                }
                else
                {
                    sd.Location = location;
                }
                sd.col10 = s.DataSourceType;
                sd.corr1 = s.corr1;
                sd.corr2 = s.corr2;
                sd.corrTemp = s.corrTemp;
                sensorData.Add(sd);
            }

            cd.BaseNames[0] = "Custom Base";



            AnalysisTask _task = new AnalysisTask();
            _task.WashFreq = 0;
            _task.DetergentWash = false;
            _task.MetDataType = MetDataTypes.Meteorological;
            _task.CrystalSize = 8;

            SensorCalib(_task, ref enviroP_List, sensorData, ref cd, numTasks);
            double[,,] PitString = coelesence(ref enviroP_List, _task);
            pit(ref enviroP_List, _task, PitString, ref cd);
            if(createNewDamageCharts) 
            {
                newCorr(sensorData, ref cd);
            }

            return cd;
        }

        public CorrosionData BeginAnalysisMultiple(List<AnalysisTask> _tasks, string tailNumber, List<CombinedDataViewModel> fileData, Dictionary<string, PollutionModel> pollutionData, ref CorrosionData cd)
        {
            SimpleRNG.SetSeedFromSystemTime();
            int numTasks = _tasks.Count();
            cd.cutoffValuesDroplets = new int[numTasks];
            cd.AircraftTail = tailNumber;

            List<List<enviroP>> enviroP_List = new List<List<enviroP>>();

            PollutionData = pollutionData;

            AnalysisTask _task = new AnalysisTask();
            _task.WashFreq = 0;
            _task.DetergentWash = false;
            _task.MetDataType = MetDataTypes.Meteorological;
            _task.CrystalSize = 6;

            List<combinedData> sensorData = new List<combinedData>();
            int index = 0;
            int cutOffIndex = 0;
            int taskIndex = 0;
            int location = _tasks[taskIndex].Location;
            foreach (CombinedDataViewModel s in fileData)
            {
                if (index == cd.cutoffValues[cutOffIndex])
                {
                    cutOffIndex++;
                    if (cutOffIndex < cd.cutoffValues.Count())
                    {
                        taskIndex++;
                        location = _tasks[taskIndex].Location;
                    }
                }
                combinedData sd = new combinedData();
                sd.Id = s.ID;
                sd.col0 = s.Record_Number;
                sd.col1 = (double)s.TOWSensor;
                sd.col2 = (double)s.TOWCalc;
                sd.col3 = s.SurfaceTemp;
                sd.col4 = s.TimeInterval;
                sd.col5 = s.ATM_RH;
                sd.col6 = s.ATM_Temp;
                sd.col7 = s.Rain;
                sd.col8 = s.Timestamp;
                sd.col9 = s.Month;
                if (location == 0)
                {
                    sd.Location = 3;
                }
                else
                {
                    sd.Location = location;
                }
                sd.col10 = s.DataSourceType;
                sensorData.Add(sd);
                index++;
            }


            SensorCalib(_task, ref enviroP_List, sensorData, ref cd, numTasks);
            double[,,] PitString = coelesence(ref enviroP_List, _task);
            pit(ref enviroP_List, _task, PitString, ref cd);

            return cd;
        }

        public CorrosionData SensorCalib(AnalysisTask _task, ref List<List<enviroP>> enviroP_List, List<combinedData> sensorData, ref CorrosionData cd, int numTasks)
        {
            int numberOfDays = 1;
            int count = 0;
            int entriesPerDay = 0;
            DateTime currentDate = sensorData[0].col8;
            double temperatureSum = 0;
            double relativeHumiditySum = 0;
            DateTime savedMinDate = DateTime.MinValue;
            double[,] Pollutant;

            Random rng = new Random();
            int p = 0;
            int q = 0;
            int t = 0;

            List<double> temp = new List<double>();
            double[] IonConc = new double[15];
            double[,] SaltOut = new double[3, 11];


            double PredA = 0, CondRH, AvgTemp;
            double TotDrop = 0, NCCon = 0, MCCon = 0, NSCon = 0, NNCon = 0, NHSCon = 0, TotCon = 0, NCVol = 0, MCVol = 0, NSVol = 0, NNVol = 0, NHSVol = 0, TotVol = 0, AvgRad = 0, CCCon = 0, KCCon = 0, MSCon = 0, KSCon = 0, CCVol = 0, KCVol = 0, MSVol = 0, KSVol = 0;
            double TotArea = 0, NCMass = 0, MCMass = 0, NSMass = 0, NNMass = 0, NHSMass = 0, TotMass = 0, CaSul = 0, CrysDens = 0, DropRad = 0, CrysMass = 0, DropArea = 0, DropVol = 0, CCMass = 0, KCMass = 0, MSMass = 0, KSMass = 0, AtSatPH2O = 0, PlSatPH2O = 0, PH2O = 0;
            double crysize = _task.CrystalSize; // int crysize = 0;

            double NdropsR = 0, NdropsT = 0, CumRH = 0, CumRHT = 0, Oxyconc = 0, Lim2024 = 0, Lim1010 = 0, DO2 = 0, OxyconcA = 0, CumTime, CumTemp, x, ph,
                HSMass, HNMass, HSVol, HNVol, HSCon, HNCon, CSMass, CSVol, CSCon;

            double NDD2024 = 0;
            double NDD1010 = 0;
            double Wetmass = 0;
            DateTime start = new DateTime();
            DateTime finish = new DateTime();
            DateTime Lrain = new DateTime();
            double duration = 0;
            double PlateRH = 0;
            double z = 0;
            HNCon = 0; //delete this line if we figure out the issue with PlateRH with pollfactors on 11
            HSVol = 0;
            HNVol = 0;
            CSVol = 0;
            HSCon = 0;
            CSCon = 0;
            x = 0;

            int periodCount = 0;

            List<enviroP> tempList = new List<enviroP>();
            enviroP tempEnviroP = new enviroP();
            tempList.Add(tempEnviroP);
            enviroP_List.Add(tempList);

            //ADD THE FIRST ARRAY INDEX WHICH WILL CONTAIN THE PERIODS
            List<int> period_list = new List<int>();
            //insert the 0th element, make it 0.
            period_list.Add(0);

            //Initilize SaltOut
            SaltOut[0, 0] = 0.25537;
            SaltOut[0, 1] = 0.36996;
            SaltOut[0, 2] = 0.74238;
            SaltOut[0, 3] = 0.42011;
            SaltOut[0, 4] = 0.20129;
            SaltOut[0, 5] = 0.07363;
            SaltOut[0, 6] = 0.34344;
            SaltOut[0, 7] = 0.54051;
            SaltOut[0, 8] = 0.44644;
            SaltOut[0, 9] = 0.21833;
            SaltOut[0, 10] = 0.02118;
            SaltOut[1, 0] = -0.03255;
            SaltOut[1, 1] = -0.06342;
            SaltOut[1, 2] = -0.34968;
            SaltOut[1, 3] = -0.08113;
            SaltOut[1, 4] = -0.02242;
            SaltOut[1, 5] = -0.00283;
            SaltOut[1, 6] = -0.05652;
            SaltOut[1, 7] = -0.13276;
            SaltOut[1, 8] = -0.11706;
            SaltOut[1, 9] = -0.0236;
            SaltOut[1, 10] = -0.000209;
            SaltOut[2, 0] = 0.00174;
            SaltOut[2, 1] = 0.00428;
            SaltOut[2, 2] = 0.07863;
            SaltOut[2, 3] = 0.00631;
            SaltOut[2, 4] = 0.00117;
            SaltOut[2, 5] = 5.64e-05;
            SaltOut[2, 6] = 0.00414;
            SaltOut[2, 7] = 0.01458;
            SaltOut[2, 8] = 0.01724;
            SaltOut[2, 9] = 0.00104;
            SaltOut[2, 10] = -1.72e-06;

            //Pollutant Levels
            double Day = 0;
            double Days = 0;

            int size = sensorData.Count;

            Pollutant = new double[size + 1, 4];
            double[] Chloride = new double[size + 1];
            Pollutant[0, 0] = 0;
            for (int w = 1; w < size; w++)
            {

                Wetmass = 0;
                Pollutant[w, 0] = Pollutant[w - 1, 0] + (getLocationStats(sensorData[w].Location, sensorData[w].col9, "Deposit") * sensorData[w].col4) / (3600d * 24d);

                //TIME CALCULATION
                Day = (sensorData[w].col8 - sensorData[0].col8).TotalDays;
                Days = (sensorData[w].col8 - sensorData[0].col8).TotalDays;
                Pollutant[w, 2] = Days;

                sensorData[w].col2 = 0;

                if (sensorData[w].col5 >= 99)
                    sensorData[w].col5 = 99;

                if (sensorData[w].col7 == 0)
                {
                    Pollutant[w, 1] = 0;
                }
                else
                {
                    Pollutant[w, 1] = sensorData[w].col7;

                    Lrain = sensorData[w].col8;

                    if (sensorData[w - 1].col7 > 0)
                    {
                        Pollutant[w, 1] = Pollutant[w - 1, 1] + sensorData[w].col7;
                    }
                }

                if (Pollutant[w, 1] > 0.009)
                    Pollutant[w, 0] = 0;
                else
                    Pollutant[w, 0] = Pollutant[w, 0] + (sensorData[w].col7 * 25.4) * getLocationStats(sensorData[w].Location, sensorData[w].col9, "RainConc");

                if (_task.WashFreq > 0)
                {
                    if (Day >= _task.WashFreq)
                    {
                        Day = Day - _task.WashFreq;
                        Pollutant[w, 1] = 10;
                        if (_task.DetergentWash)
                        {
                            Pollutant[w, 0] = 0;
                        }
                        else
                        {
                            Pollutant[w, 0] = 10;
                        }
                    }
                }

                //number of crystals
                NCMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "NaCl") / 100;
                MCMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "MgC12") / 100;
                NSMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "Na2SO4") / 100;
                CCMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "CaC12") / 100;
                KCMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "KCI") / 100;
                NNMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "AmmNit") / 100;
                NHSMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "AmmSul") / 100;
                MSMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "MgSO4") / 100;
                KSMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "K2SO4") / 100;
                CSMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "CaSO4") / 100;
                HSMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "HSMass") / 100;
                HNMass = getLocationStats(sensorData[w].Location, sensorData[w].col9, "HNMass") / 100;

                CaSul = 1 - NCMass - MCMass - CCMass - KCMass - NNMass - NHSMass - MSMass;
                ph = getLocationStats(sensorData[w].Location, sensorData[w].col9, "RainConc");

                CrysDens = NCMass * 2.165 + MCMass * 2.32 + NSMass * 2.66 + CCMass * 2.15 + KCMass * 1.98 + NNMass * 1.725 + NHSMass * 1.77 + MSMass * 2.66 + KSMass * 2.66 + CSMass * 2.96 + HSMass * 1 + HNMass * 1 + CSMass * 2.32;
                CrysMass = (((4.0 / 3.0) * Math.PI * (Math.Pow((crysize / 10000), 3)) * CrysDens));

                if (sensorData[w].col10 == 1)
                {
                    AtSatPH2O = 0.61158 + (0.04401 * sensorData[w].col6) + (0.00147 * (Math.Pow(sensorData[w].col6, 2)) + (2.58E-05 * (Math.Pow(sensorData[w].col6, 3))) + (2.86E-07 * (Math.Pow(sensorData[w].col6, 4))) + (2.72E-09 * (Math.Pow(sensorData[w].col6, 5))));

                    PlSatPH2O = 0.61158 + (0.04401 * sensorData[w].col3) + (0.00147 * (Math.Pow(sensorData[w].col3, 2))) + (2.58E-05 * (Math.Pow(sensorData[w].col3, 3))) + (2.86E-07 * (Math.Pow(sensorData[w].col3, 4))) + (2.72E-09 * (Math.Pow(sensorData[w].col3, 5)));

                    PH2O = sensorData[w].col5 * AtSatPH2O / 100;

                    PlateRH = 100 * (PH2O / PlSatPH2O);
                }
                else
                {
                    if (sensorData[w].col4 > 600)
                    {
                        PlateRH = sensorData[w].col5;
                        sensorData[w].col3 = sensorData[w].col6;
                    }
                    else
                    {
                        sensorData[w].col3 = sensorData[w - 1].col3;
                    }
                }

                CondRH = PlateRH;


                if (PlateRH > 99)
                {
                    PlateRH = 99;
                }

                sensorData[w].col2 = CondRH;

                if (CondRH > 100)
                {
                    enviroP_List[p][0].col17 = 1;
                }

                if (PlateRH >= ConstData.PollFactors[0, 0])
                {
                    if (NCMass > 0)
                    {
                        NCCon = ConstData.PollFactors[1, 0] + ConstData.PollFactors[2, 0] * PlateRH + ConstData.PollFactors[3, 0] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 0] * (Math.Pow(PlateRH, 3));
                        NCVol = (NCMass / 58.4) / NCCon;
                        Wetmass = Wetmass + NCMass;
                    }
                }
                else
                {
                    NCMass = 0;
                    NCCon = 0;
                    NCVol = 0;
                }

                if (PlateRH >= ConstData.PollFactors[0, 1])
                {
                    if (MCMass > 0)
                    {
                        MCCon = ConstData.PollFactors[1, 1] + ConstData.PollFactors[2, 1] * PlateRH + ConstData.PollFactors[3, 1] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 1] * (Math.Pow(PlateRH, 3));
                        MCVol = (MCMass / 95.22) / MCCon;
                        Wetmass = Wetmass + MCMass;
                    }
                }
                else
                {
                    MCMass = 0;
                    MCCon = 0;
                    MCVol = 0;
                }
                if (PlateRH >= ConstData.PollFactors[0, 2])
                {
                    if (NSMass > 0)
                    {
                        NSCon = ConstData.PollFactors[1, 2] + ConstData.PollFactors[2, 2] * PlateRH + ConstData.PollFactors[3, 2] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 2] * (Math.Pow(PlateRH, 3));
                        NSVol = (NSMass / 142.04) / NSCon;
                        Wetmass = Wetmass + NSMass;
                    }
                }
                else
                {
                    NSMass = 0;
                    NSCon = 0;
                    NSVol = 0;
                }
                if (PlateRH >= ConstData.PollFactors[0, 3])
                {
                    if (CCMass > 0)
                    {
                        CCCon = ConstData.PollFactors[1, 3] + ConstData.PollFactors[2, 3] * PlateRH + ConstData.PollFactors[3, 3] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 3] * (Math.Pow(PlateRH, 3));
                        CCVol = (CCMass / 110.98) / CCCon;
                        Wetmass = Wetmass + CCMass;
                    }
                }
                else
                {
                    CCMass = 0;
                    CCCon = 0;
                    CCVol = 0;
                }
                if (PlateRH >= ConstData.PollFactors[0, 4])
                {
                    if (KCMass > 0)
                    {
                        KCCon = ConstData.PollFactors[1, 4] + ConstData.PollFactors[2, 4] * PlateRH + ConstData.PollFactors[3, 4] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 4] * (Math.Pow(PlateRH, 3));
                        KCVol = (KCMass / 74.55) / KCCon;
                        Wetmass = Wetmass + KCMass;
                    }
                }
                else
                {
                    KCMass = 0;
                    KCCon = 0;
                    KCVol = 0;
                }
                if (PlateRH >= ConstData.PollFactors[0, 5])
                {
                    if (NNMass > 0)
                    {
                        NNCon = ConstData.PollFactors[1, 5] + ConstData.PollFactors[2, 5] * PlateRH + ConstData.PollFactors[3, 5] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 5] * (Math.Pow(PlateRH, 3));
                        NNVol = (NNMass / 80.08) / NNCon;
                        Wetmass = Wetmass + NNMass;
                    }
                }
                else
                {
                    NNMass = 0;
                    NNCon = 0;
                    NNVol = 0;
                }
                if (PlateRH >= ConstData.PollFactors[0, 6])
                {
                    if (NHSMass > 0)
                    {
                        NHSCon = ConstData.PollFactors[1, 6] + ConstData.PollFactors[2, 6] * PlateRH + ConstData.PollFactors[3, 6] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 6] * (Math.Pow(PlateRH, 3));
                        NHSVol = (NHSMass / 132.2) / NHSCon;
                        Wetmass = Wetmass + NHSMass;
                    }
                }
                else
                {
                    NHSMass = 0;
                    NHSCon = 0;
                    NHSVol = 0;
                }
                if (PlateRH >= ConstData.PollFactors[0, 7])
                {
                    if (MSMass > 0)
                    {
                        MSCon = ConstData.PollFactors[1, 7] + ConstData.PollFactors[2, 7] * PlateRH + ConstData.PollFactors[3, 7] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 7] * (Math.Pow(PlateRH, 3));
                        MSVol = (MSMass / 120.37) / MSCon;
                        Wetmass = Wetmass + MSMass;
                    }
                }
                else
                {
                    MSMass = 0;
                    MSCon = 0;
                    MSVol = 0;
                }
                if (PlateRH >= ConstData.PollFactors[0, 8])
                {
                    if (KSMass > 0)
                    {
                        KSCon = ConstData.PollFactors[1, 8] + ConstData.PollFactors[2, 8] * PlateRH + ConstData.PollFactors[3, 8] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 8] * (Math.Pow(PlateRH, 3));
                        KSVol = (KSMass / 120.37) / KSCon;
                        Wetmass = Wetmass + KSMass;
                    }
                }
                else
                {
                    KSMass = 0;
                    KSCon = 0;
                    KSVol = 0;
                }
                if (PlateRH >= ConstData.PollFactors[0, 9])
                {
                    if (HSMass > 0)
                    {
                        HSCon = ConstData.PollFactors[1, 9] + ConstData.PollFactors[2, 9] * PlateRH + ConstData.PollFactors[3, 9] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 9] * (Math.Pow(PlateRH, 3));
                        HSVol = (HSMass / 98.06) / HSCon;
                        Wetmass = Wetmass + HSMass;
                    }
                }
                else
                {
                    HSMass = 0;
                    HSCon = 0;
                    HSVol = 0;
                }
                if (PlateRH >= ConstData.PollFactors[0, 10])
                {
                    if (HNMass > 0)
                    {
                        HNCon = ConstData.PollFactors[1, 10] + ConstData.PollFactors[2, 10] * PlateRH + ConstData.PollFactors[3, 10] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 10] * (Math.Pow(PlateRH, 3));
                        HNVol = (HNMass / 63) / HNCon;
                        Wetmass = Wetmass + HNMass;
                    }
                }
                else
                {
                    HNMass = 0;
                    HNCon = 0;
                    HNVol = 0;
                }
                //if (PlateRH >= ConstData.PollFactors[0, 11])
                //{
                //    if (CSMass > 0)
                //    {
                //        CSCon = ConstData.PollFactors[1, 11] + ConstData.PollFactors[2, 11] * PlateRH + ConstData.PollFactors[3, 11] * (Math.Pow(PlateRH, 2)) + ConstData.PollFactors[4, 11] * (Math.Pow(PlateRH, 3));
                //        CSVol = (CSMass / 136) / CSCon;
                //        Wetmass = Wetmass + CSMass;
                //    }
                //}
                //else
                //{
                //    CSMass = 0;
                //    CSCon = 0;
                //    CSVol = 0;
                //}



                TotVol = NCVol + MCVol + NSVol + CCVol + KCVol + NNVol + NHSVol + MSVol + KSVol + HSVol + HNVol + CSVol;
                DropVol = (TotVol * CrysMass) * 1000;
                DropRad = (Math.Pow((1.5 * (DropVol) / Math.PI), (1.0 / 3.0)));
                DropArea = Math.Pow(DropRad, 2) * Math.PI;

                if (TotVol == 0)
                {
                    DropArea = 2.8274E-7;
                    DropRad = 0.0003;
                }

                if (NCCon > 0)
                    NCCon = (NCMass / 58.4) / (TotVol);

                if (MCCon > 0)
                    MCCon = (MCMass / 95.22) / (TotVol);

                if (NSCon > 0)
                    NSCon = (NSMass / 142.04) / (TotVol);

                if (NNCon > 0)
                    NNCon = (NNMass / 80.08) / (TotVol);

                if (CCCon > 0)
                    CCCon = (CCMass / 110.98) / (TotVol);

                if (KCCon > 0)
                    KCCon = (KCMass / 74.55) / (TotVol);

                if (NHSCon > 0)
                    NHSCon = (NHSMass / 132.2) / (TotVol);

                if (MSCon > 0)
                    MSCon = (MSMass / 120.37) / (TotVol);

                if (KSCon > 0)
                    KSCon = (KSMass / 174.26) / (TotVol);

                if (HSCon > 0)
                    HSCon = (HSMass / 98.06) / TotVol;

                if (HNCon > 0)
                    HNCon = (HNMass / 63) / TotVol;

                if (CSCon > 0)
                    CSCon = (CSMass / 136) / TotVol;


                TotCon = NCCon + MCCon + NSCon + CCCon + KCCon + NNCon + NHSCon + MSCon + KSCon + HSCon + HNCon;

                if (sensorData[w].col10 == 2)
                {
                    if (Pollutant[w, 0] > 0)
                    {
                        TotMass = Pollutant[w, 0] / (1000.00 * 10000.00);
                        TotDrop = TotMass / CrysMass;
                    }
                    else
                    {
                        TotMass = 0;
                        TotDrop = 0;
                    }
                }

                if (sensorData[w].col10 == 1)
                {
                    TotArea = sensorData[w].col1 / 0.2;
                    if (TotArea > 1)
                    {
                        TotArea = 1;
                    }
                }
                else
                {
                    if (sensorData[w].col4 > 600)
                    {
                        TotArea = DropArea * TotDrop;
                        sensorData[w].col1 = TotArea * 0.2;
                    }
                    else
                    {
                        sensorData[w].col1 = sensorData[w - 1].col1;
                    }
                }
                if (TotArea > 1)
                {
                    TotArea = 1;
                }

                //Oxygen concentration calc for salt solution
                IonConc[0] = NCCon + MCCon * 2 + CCCon * 2 + KCCon;

                IonConc[1] = (TotCon * SaltOut[0, 0] + Math.Pow(TotCon, 2) * SaltOut[1, 0] + Math.Pow(TotCon, 3) * SaltOut[2, 0]) * NCMass / Wetmass;

                IonConc[2] = (TotCon * SaltOut[0, 1] + Math.Pow(TotCon, 2) * SaltOut[1, 1] + Math.Pow(TotCon, 3) * SaltOut[2, 1]) * MCMass / Wetmass;

                IonConc[3] = (TotCon * SaltOut[0, 2] + Math.Pow(TotCon, 2) * SaltOut[1, 2] + Math.Pow(TotCon, 3) * SaltOut[2, 2]) * NSMass / Wetmass;

                IonConc[4] = (TotCon * SaltOut[0, 3] + Math.Pow(TotCon, 2) * SaltOut[1, 3] + Math.Pow(TotCon, 3) * SaltOut[2, 3]) * CCMass / Wetmass;

                IonConc[5] = (TotCon * SaltOut[0, 4] + Math.Pow(TotCon, 2) * SaltOut[1, 4] + Math.Pow(TotCon, 3) * SaltOut[2, 4]) * KCMass / Wetmass;

                IonConc[6] = (TotCon * SaltOut[0, 5] + Math.Pow(TotCon, 2) * SaltOut[1, 5] + Math.Pow(TotCon, 3) * SaltOut[2, 5]) * NNMass / Wetmass;

                IonConc[7] = (TotCon * SaltOut[0, 6] + Math.Pow(TotCon, 2) * SaltOut[1, 6] + Math.Pow(TotCon, 3) * SaltOut[2, 6]) * NHSMass / Wetmass;

                IonConc[8] = (TotCon * SaltOut[0, 7] + Math.Pow(TotCon, 2) * SaltOut[1, 7] + Math.Pow(TotCon, 3) * SaltOut[2, 7]) * MSMass / Wetmass;

                IonConc[9] = (TotCon * SaltOut[0, 8] + Math.Pow(TotCon, 2) * SaltOut[1, 8] + Math.Pow(TotCon, 3) * SaltOut[2, 8]) * KSMass / Wetmass;

                IonConc[10] = (TotCon * SaltOut[0, 9] + Math.Pow(TotCon, 2) * SaltOut[1, 9] + Math.Pow(TotCon, 3) * SaltOut[2, 9]) * HSMass / Wetmass;

                IonConc[11] = (TotCon * SaltOut[0, 10] + Math.Pow(TotCon, 2) * SaltOut[1, 10] + Math.Pow(TotCon, 3) * SaltOut[2, 10]) * NSMass / Wetmass;

                IonConc[12] = NSCon + NHSCon + MSCon + KSCon + HSCon;


                x = 0;
                if (IonConc[0] > 0)
                {
                    if (IonConc[12] > 0)
                    {
                        x = IonConc[12] / IonConc[0];
                    }
                }


                //Oxygen conc in pure water
                PredA = ConstData.polyConst[0, 0] + (ConstData.polyConst[1, 0] * sensorData[w].col3) + (ConstData.polyConst[2, 0] * (Math.Pow(sensorData[w].col3, 2))) + (ConstData.polyConst[3, 0] * (Math.Pow(sensorData[w].col3, 3)));
                IonConc[13] = PredA;

                // Salting out calcs
                IonConc[14] = IonConc[1] + IonConc[2] + IonConc[3] + IonConc[4] + IonConc[5] + IonConc[6] + IonConc[7] + IonConc[8] + IonConc[9] + IonConc[10] + IonConc[11];
                //IonConc[12] = IonConc[14];
                Oxyconc = PredA * (1 - IonConc[14]);
                if (TotCon == 0)
                {
                    Oxyconc = PredA;
                }
                Pollutant[w, 3] = Oxyconc;

                if (sensorData[w].col1 >= 0.0003)
                {
                    if (sensorData[w - 1].col1 < 0.0003)
                    {
                        p = p + 1;
                        t = 1;
                        if (period_list.ElementAtOrDefault(p) == null || period_list.ElementAtOrDefault(p) == 0)
                        {
                            while (period_list.ElementAtOrDefault(p) == null || period_list.ElementAtOrDefault(p) == 0)
                            {
                                period_list.Add(1);
                            }
                        }
                        else
                        {
                            period_list[p] = 1;
                        }
                        duration = sensorData[w].col4;
                        start = sensorData[w].col8;
                        enviroP tempE2 = new enviroP();
                        tempE2.col11 = 0;
                        tempE2.col10 = 0;

                        if ((Lrain - start).TotalDays > -0.0625)
                        {
                            tempE2.col10 = (Lrain - start).TotalDays * -1440;
                        }
                        List<enviroP> tempEPList = new List<enviroP>();
                        tempEPList.Add(tempE2);
                        while (enviroP_List.ElementAtOrDefault(p) == null)
                        {
                            enviroP_List.Add(tempEPList);
                        }
                    }
                    else
                    {
                        t = t + 1;
                        duration = duration + sensorData[w].col4;
                        period_list[p] = period_list[p] + 1;
                    }
                    if (sensorData[w].col10 == 1)
                    {
                        TotDrop = (sensorData[w].col1 / 0.2) / DropArea;
                    }
                    enviroP tempE = new enviroP();
                    tempE.col0 = sensorData[w].col3;    //temp
                    tempE.col1 = sensorData[w].col5;        //atm RH
                    tempE.col2 = sensorData[w].col4 / 3600.00;        //Time period
                    tempE.col3 = TotCon;        //Total concentration
                    tempE.col4 = (DropRad / (crysize / 10000.00));     //Rw/Rs
                    tempE.col5 = DropArea;      // Drop area
                    tempE.col6 = TotArea * 100.00;        //Percentage coverage
                    tempE.col7 = Days;          // days from start
                    tempE.col8 = TotDrop;       // number of deposited salt crystals
                    tempE.col9 = sensorData[w].col6;        // Atm temp
                    tempE.col10 = TotDrop * CrysMass * 10000000.00;        //Salt density
                    tempE.col11 = sensorData[w].col7;       // Rain/washing amount
                    tempE.col17 = sensorData[w].col2;       //plateRH //Chloride conc
                    if (sensorData[w].col7 > 0)
                    {
                        if (enviroP_List[p][0].col11 <= 0)
                        {
                            enviroP_List[p][0].col11 = t;
                        }
                    }

                    //Reporting of oxygen parameters
                    OxyconcA = (Oxyconc / 31.9998) * 1000;
                    tempE.col12 = OxyconcA;
                    DO2 = (1.10498 + 0.04211 * sensorData[w].col3 + 0.000417833 * (Math.Pow(sensorData[w].col3, 2)) + ((-0.28202 - 0.01265 * sensorData[w].col3 - 0.000339991 * (Math.Pow(sensorData[w].col3, 2))) * TotCon) + (0.03366 + 0.00148 * sensorData[w].col3 + 0.000113536 * (Math.Pow(sensorData[w].col3, 2))) * (Math.Pow(TotCon, 2)));
                    DO2 = DO2 / 1e9;
                    tempE.col15 = DO2;

                    //Limiting cathodic current density
                    NDD2024 = (1.30964 - 0.20903 * TotCon + 0.01702 * (Math.Pow(TotCon, 2)));
                    Lim2024 = (4 * 96485 * DO2 * (OxyconcA / 1000)) / NDD2024 * 100000;
                    tempE.col14 = Lim2024;                                          //AA2024-T3 Lim2024
                    NDD1010 = ((1.0016 - 0.15986 * TotCon + 0.01302 * (Math.Pow(TotCon, 2))) * 0.246937911);
                    Lim1010 = (3 * 96485 * DO2 * (OxyconcA / 1000)) / NDD1010 * 100000;
                    tempE.col15 = Lim1010;

                    //Pit Probabilities
                    z = Math.Log10(IonConc[0]);
                    tempE.col16 = 8.36254E-04 - (7.48642E-04 * z) - 5.88611E-04 * Math.Pow(z, 2) - 8.75229E-05 * Math.Pow(z, 3);   //AA2024 Pit Prob

                    if (x > 0)
                    {
                        tempE.col16 = tempE.col16 * (0.19267 + -0.24656 * Math.Pow(x, 1) + 0.07748 * Math.Pow(x, 2)); // effect of sulphate
                    }

                    tempE.col16 = tempE.col16 * (-0.47137 + 0.23163 * ph);

                    //if (IonConc[12] != 0)
                    //{
                    //    tempE.col16 = tempE.col16 * (0.5 + Math.Log10(IonConc[0] / IonConc[12]) * 0.5);
                    //}
                    if (tempE.col16 < 0 || Double.IsNaN(tempE.col16))
                    {
                        tempE.col16 = 0;
                    }

                    if (TotCon == 0)
                    {
                        tempE.col16 = 0;
                    }

                    if (enviroP_List[p].ElementAtOrDefault(t) != null)
                    {
                        enviroP_List[p][t] = tempE;
                    }
                    else
                    {
                        enviroP_List[p].Add(tempE);
                    }
                }

                if (sensorData[w - 1].col1 >= 0.0003)
                {
                    if (sensorData[w].col1 < 0.0003)
                    {
                        finish = sensorData[w].col8;
                        if (p >= 1)
                        {
                            enviroP_List[p][0].col1 = finish.ToOADate();
                            enviroP_List[p][0].col2 = (finish - start).TotalSeconds;
                        }
                        if (duration < 600 && p > 0)
                        {
                            p = p - 1;
                        }
                    }
                }

                combinedData item = sensorData[w];
                //this code block takes daily average of temp and rh
                //if (item.col8.Day == currentDate.Day && item.col8.Month == currentDate.Month && item.col8.Year == currentDate.Year)
                //{
                //    if (savedMinDate == DateTime.MinValue)
                //    {
                //        savedMinDate = item.col8;
                //    }
                //    temperatureSum = temperatureSum + item.col6;
                //    relativeHumiditySum = relativeHumiditySum + item.col5;
                //    entriesPerDay++;
                //}
                //else
                //{
                //    cd.Temperature.Add(temperatureSum / entriesPerDay);
                //    cd.RelativeHumidity.Add(relativeHumiditySum / entriesPerDay);
                //    cd.Y.Add(currentDate);

                //    currentDate = item.col8;
                //    temperatureSum = item.col6;
                //    relativeHumiditySum = item.col5;
                //    entriesPerDay = 1;
                //}

                cd.Temperature.Add(item.col6);
                cd.RelativeHumidity.Add(item.col5);
                cd.Y.Add(item.col8);

                if (item.col7 != 0)
                {
                    cd.Precipitation.Add(item.col7);
                    cd.PrecipitationDates.Add(item.col8);
                }
                if (item.col1 != 0)
                {
                    cd.TimeofWetness.Add(item.col1);
                    cd.TOWDates.Add(item.col8);
                }
                if (count < size)
                {
                    cd.SurfacePollutants.Add(Pollutant[count, 0]);
                    cd.PollutantDates.Add(item.col8);

                }
                count++;

            }//endfor 
            cd.Temperature.Add(temperatureSum / entriesPerDay);
            cd.RelativeHumidity.Add(relativeHumiditySum / entriesPerDay);
            cd.Y.Add(currentDate);


            enviroP_List[0][0].col0 = p;
            enviroP_List[0][0].col1 = _task.CrystalSize;
            enviroP_List[0][0].col7 = Days;

            int largestPeriodCount = 0;

            foreach (int period in period_list)
            {
                periodCount = periodCount + period;
                if (period > largestPeriodCount)
                {
                    largestPeriodCount = period;
                }
            }

            enviroP_List[0][0].col17 = periodCount;
            enviroP_List[0][0].col16 = largestPeriodCount;

            for (q = 1; q <= p; q++)
            {
                NdropsR = 0;
                CumRH = 0;
                CumTime = 0;
                AvgRad = 0;
                AvgTemp = 0;

                for (int r = 1; r <= period_list[q]; r++)
                {
                    CumTime = CumTime + enviroP_List[q][r].col2;
                    NdropsR = NdropsR + enviroP_List[q][r].col8 * enviroP_List[q][r].col2;
                    NdropsT = NdropsR / r;
                    CumRH = CumRH + enviroP_List[q][r].col1 * enviroP_List[q][r].col2;
                    CumRHT = CumRH / r;
                    AvgRad = AvgRad + enviroP_List[q][r].col4 * enviroP_List[q][r].col2;
                    AvgTemp = AvgTemp + enviroP_List[q][r].col0 * enviroP_List[q][r].col2;
                }//endfor

                enviroP tempE = new enviroP();
                tempE.col0 = NdropsR / CumTime;
                tempE.col1 = CumTime;
                tempE.col2 = CumRH / CumTime;
                tempE.col3 = tempE.col0 * ((4.0 / 3.0) * 3.14159265358979 * (Math.Pow((crysize / 10000), 3)) * CrysDens) * 10000000;
                tempE.col4 = enviroP_List[q][1].col7;
                tempE.col5 = AvgRad / CumTime;
                tempE.col6 = AvgTemp / CumTime;

                enviroP_List[0].Add(tempE);

                cd.Droplets.Add(tempE.col0);
                cd.DropletsY.Add(q);
            }

            int index = 0;
            foreach (List<enviroP> list in enviroP_List)
            {
                Debug.WriteLine("List " + index);
                foreach (enviroP e in list)
                {
                    Debug.WriteLine(e.col0 + "," + e.col1 + "," + e.col2 + "," + e.col3 + "," + e.col4 + "," + e.col5 + "," + e.col6 + "," + e.col7 + "," + e.col8 + "," + e.col9 + "," + e.col10 + "," + e.col11 + "," + e.col12 + "," + e.col13 + "," + e.col14 + "," + e.col15 + "," + e.col16 + "," + e.col17);
                }
                index++;
            }

            return cd;

        }

        public double[,,] coelesence(ref List<List<enviroP>> _enviroP_List, AnalysisTask _task)
        {
            double RH;
            double SDensity;
            double NoCNoD;
            double CrySize;
            double NDrops;
            double NoClust;
            int NoPits = 1000;

            double DropArea;
            double WetArea;
            double ClustScale = 0;
            double LargeClust = 0;
            double DropVol = 0;
            double DropRadius = 0;

            double Yo, A1, t1, Int, B1, B2, B3, B4, P1, P2, P3, P4, eqn, Drop1, SaltPmass, y, w, k, l, m, n;
            int q;
            int x;
            int p;
            int g;
            int z;
            Yo = A1 = t1 = Int = B1 = B2 = B3 = B4 = P1 = P2 = P3 = P4 = eqn = Drop1 = SaltPmass = y = w = k = l = m = n = q = 0;

            double[] DropProb = new double[1001];
            double[] ClusterNoSize = new double[1001];
            int[] PitData = new int[1001];

            double[,] ClustSize = new double[4, 1001];
            int[] PitSDrop = new int[1001];
            ClustScale = 0;
            double[] CellSize = new double[1001];
            double[] wave0 = new double[1001];


            double[,] DCfuncVals = new double[4, 4];
            DCfuncVals[0, 0] = 3029.56;
            DCfuncVals[0, 1] = -310.24;
            DCfuncVals[0, 2] = 2360.31;
            DCfuncVals[0, 3] = -960.335;

            DCfuncVals[1, 0] = -98.6434;
            DCfuncVals[1, 1] = 8.11588;
            DCfuncVals[1, 2] = -54.734;
            DCfuncVals[1, 3] = 70.839;

            DCfuncVals[2, 0] = 1.06094;
            DCfuncVals[2, 1] = -0.0536509;
            DCfuncVals[2, 2] = -27.1784;
            DCfuncVals[2, 3] = -0.839702;

            DCfuncVals[3, 0] = -0.00377393;
            DCfuncVals[3, 1] = 0;
            DCfuncVals[3, 2] = 0;
            DCfuncVals[3, 3] = 0.00332268;

            double[,] PredG100 = new double[7, 3];
            PredG100[0, 0] = 0.990431;
            PredG100[0, 1] = -0.70891;
            PredG100[0, 2] = 0.779176;

            PredG100[1, 0] = 5.24872e-05;
            PredG100[1, 1] = -0.0024989;
            PredG100[1, 2] = -7.87883;

            PredG100[2, 0] = -6.72446e-08;
            PredG100[2, 1] = 3.9125e-06;
            PredG100[2, 2] = 557.128;

            PredG100[3, 0] = 2.36908e-11;
            PredG100[3, 1] = -1.2136e-09;
            PredG100[3, 2] = -89.0678;

            PredG100[4, 0] = 0;
            PredG100[4, 1] = 0;
            PredG100[4, 2] = 69.6577;

            PredG100[5, 0] = 0;
            PredG100[5, 1] = 0;
            PredG100[5, 2] = -1399.5;

            PredG100[6, 0] = 0;
            PredG100[6, 1] = 0;
            PredG100[6, 2] = 22.2394;



            double[,] PredG50 = new double[5, 3];
            PredG50[0, 0] = 0.994596;
            PredG50[0, 1] = -5.26042;
            PredG50[0, 2] = -38.4971;

            PredG50[1, 0] = 397.86;
            PredG50[1, 1] = 0.223299;
            PredG50[1, 2] = -8751.6;

            PredG50[2, 0] = 5.93169;
            PredG50[2, 1] = -0.00418462;
            PredG50[2, 2] = -14.5467;

            PredG50[3, 0] = 0;
            PredG50[3, 1] = 3.44337e-05;
            PredG50[3, 2] = 0;

            PredG50[4, 0] = 0;
            PredG50[4, 1] = -1.05814e-07;
            PredG50[4, 2] = 0;

            double[,] PredG25 = new double[3, 4];
            PredG25[0, 0] = -0.377491;
            PredG25[0, 1] = 0.00670168;
            PredG25[0, 2] = -2.51182e-05;
            PredG25[0, 3] = 1.99508e-08;

            PredG25[1, 0] = 0.0234332;
            PredG25[1, 1] = -0.000347875;
            PredG25[1, 2] = 1.27175e-06;
            PredG25[1, 3] = -9.4274e-10;

            PredG25[2, 0] = -0.000253511;
            PredG25[2, 1] = 5.34527e-06;
            PredG25[2, 2] = -1.66806e-08;
            PredG25[2, 3] = 1.14792e-11;

            double[,] PredG125 = new double[3, 4];
            PredG125[0, 0] = -28.6912;
            PredG125[0, 1] = -26.0461;
            PredG125[0, 2] = -0.04783;
            PredG125[0, 3] = 0.000115008;

            PredG125[1, 0] = 1.87504;
            PredG125[1, 1] = 1.4482;
            PredG125[1, 2] = 0.0078;
            PredG125[1, 3] = -1.71919e-05;

            PredG125[2, 0] = -0.0351;
            PredG125[2, 1] = -0.20629;
            PredG125[2, 2] = 0;
            PredG125[2, 3] = 7.97443e-07;

            CrySize = Convert.ToDouble(_task.CrystalSize);

            int size = Convert.ToInt32(_enviroP_List[0][0].col0) - 1;//_enviroP_List[0].Count;

            int pitstringlastindexvalue = 1001;
            if ((size + 1) > 1001)
            {
                pitstringlastindexvalue = (size + 1);
            }

            double[,,] PitString = new double[size + 3, 3, pitstringlastindexvalue];


            if (size <= 0)
            {
                NDrops = _enviroP_List[0][q].col0;
                DropRadius = (_enviroP_List[0][q].col5 / 10000d) * CrySize;
                DropArea = Math.Pow(DropRadius, 2) * Math.PI;

                WetArea = NDrops * DropArea;
                NoCNoD = 1034.95140648585d * Math.Exp(-WetArea / 0.368319971076184d) + -6.0482651588319d;

                if (NoCNoD <= 1)
                {
                    NoCNoD = 1;
                }

                if (NoCNoD >= 1000)
                {
                    NoCNoD = 1000;
                }

                PitString[q, 0, 0] = NoCNoD;
                PitString[0, 2, q] = NoCNoD;
            }
            for (q = 1; q < size; q++)
            {
                RH = _enviroP_List[0][q].col2;
                SDensity = _enviroP_List[0][q].col3;
                NDrops = _enviroP_List[0][q].col0;
                DropRadius = (_enviroP_List[0][q].col5 / 10000.00) * CrySize;
                DropArea = Math.Pow(DropRadius, 2.00) * Math.PI;
                DropVol = Math.Pow(DropRadius, 3.00) * Math.PI * (2.0 / 3.0);
                WetArea = NDrops * DropArea;
                PitString[0, 0, q] = WetArea;
                PitString[0, 1, q] = NDrops;

                NoCNoD = 1034.95140648585d * Math.Exp(-WetArea / 0.368319971076184d) + -6.0482651588319d;

                if (NoCNoD <= 1)
                {
                    NoCNoD = 1;
                }

                if (NoCNoD >= 1000)
                {
                    NoCNoD = 1000;
                }

                //layer = q, rows = 2nd, cols = 3rd
                PitString[q, 0, 0] = NoCNoD;
                PitString[0, 2, q] = NoCNoD;

                if (NoCNoD >= 25)
                    if (NoCNoD <= 63)
                        ClustScale = (-103.777 + 16.2082 * Math.Exp(-NoCNoD / -16.6008)) + (6.3097E-01 + 2.7252E-02 * NoCNoD - 6.3467E-04 * (Math.Pow(NoCNoD, 2))) * NDrops;
                    else if (NoCNoD <= 150)
                        ClustScale = (95.1798 + 9075.81 * Math.Exp(-NoCNoD / 21.5502)) + (1.8891E-01 + -2.5593E-03 * NoCNoD + 9.1113E-06 * (Math.Pow(NoCNoD, 2))) * NDrops;
                    else if (NoCNoD <= 750)
                        ClustScale = (8.74328 + 262.0219 * Math.Exp(-NoCNoD / 126.093)) + (3.5052E-04 + 1.5496E-01 * Math.Exp(-NoCNoD / 3.9332E+01)) * NDrops;
                    else
                        ClustScale = 0;

                Yo = 0;
                A1 = 0;
                t1 = 0;

                // Probablity values vs cluster size
                if (NoCNoD >= 100)
                {
                    Yo = PredG100[0, 0] + PredG100[1, 0] * NoCNoD + PredG100[2, 0] * (Math.Pow(NoCNoD, 2)) + PredG100[3, 0] * (Math.Pow(NoCNoD, 3));
                    A1 = PredG100[0, 1] + PredG100[1, 1] * NoCNoD + PredG100[2, 1] * (Math.Pow(NoCNoD, 2)) + PredG100[3, 1] * (Math.Pow(NoCNoD, 3));
                    t1 = PredG100[1, 2] * (Math.Exp(-NoCNoD / PredG100[2, 2])) + PredG100[3, 2] * (Math.Exp(-NoCNoD / PredG100[4, 2])) + PredG100[5, 2] * (Math.Exp(-NoCNoD / PredG100[6, 2])) + PredG100[0, 2];
                    eqn = 0;
                    Drop1 = Yo + A1 * Math.Exp(1 / t1);
                }
                else if (NoCNoD >= 50)
                {
                    Yo = PredG50[0, 0] + PredG50[1, 0] * (Math.Exp(-NoCNoD / PredG50[2, 0]));
                    A1 = PredG50[0, 1] + PredG50[1, 1] * NoCNoD + PredG50[2, 1] * (Math.Pow(NoCNoD, 2)) + PredG50[3, 1] * (Math.Pow(NoCNoD, 3)) + PredG50[4, 1] * (Math.Pow(NoCNoD, 4));
                    t1 = PredG50[0, 2] + PredG50[1, 2] * Math.Exp(NoCNoD / PredG50[2, 2]);
                    eqn = 1;
                    Drop1 = Yo + A1 * Math.Exp(1 / t1);

                }
                else if (NoCNoD >= 25)
                {
                    Int = PredG25[0, 0] + PredG25[1, 0] * NoCNoD + PredG25[2, 0] * Math.Pow(NoCNoD, 2);
                    B1 = PredG25[0, 1] + PredG25[1, 1] * NoCNoD + PredG25[2, 1] * Math.Pow(NoCNoD, 2);
                    B2 = PredG25[0, 2] + PredG25[1, 2] * NoCNoD + PredG25[2, 2] * Math.Pow(NoCNoD, 2);
                    B3 = PredG25[0, 3] + PredG25[1, 3] * NoCNoD + PredG25[2, 3] * Math.Pow(NoCNoD, 2);
                    eqn = 2;
                    Drop1 = Int + B1 + B2 + B3;

                }
                else
                {
                    eqn = 4;
                    Drop1 = 1;
                }

                DropProb[0] = 0;

                for (x = 1; x <= 1000; x++)
                {
                    if (eqn == 0)
                    {
                        DropProb[x] = Yo + A1 * Math.Exp(x / t1);

                        if (DropProb[x] > 0.9)
                            if (DropProb[x] - DropProb[x - 1] < 0.000001)
                                DropProb[x] = 1;

                        if (DropProb[x] > 1)
                            DropProb[x] = 1;
                    }
                    else if (eqn == 1)
                    {
                        DropProb[x] = Yo + A1 * Math.Exp(x / t1);

                        if (DropProb[x] > 0.9)
                            if (DropProb[x] - DropProb[x - 1] < 0.000001)
                                DropProb[x] = 1;

                        if (DropProb[x] > 1)
                            DropProb[x] = 1;
                    }
                    else if (eqn == 2)
                    {
                        DropProb[x] = Int + B1 * x + B2 * Math.Pow(x, 2) + B3 * Math.Pow(x, 3);

                        if (DropProb[x] > 0.9)
                            if (DropProb[x] - DropProb[x - 1] < 0.000001)
                                DropProb[x] = 1;

                        if (DropProb[x] > 1)
                            DropProb[x] = 1;
                    }
                    else if (eqn == 4)
                    {
                        for (int j = 1; j <= 999; j++)
                            DropProb[j] = 0;

                        DropProb[1000] = 1;
                    }


                }//endfor

                // Developing cluster information
                ClusterNoSize[0] = 0;
                w = 0;
                NoClust = 0;
                p = 0;

                for (x = 1; x <= 1000; x++)
                {
                    if (DropProb[x - 1] == 1)
                        ClusterNoSize[x] = 0;
                    else
                    {
                        ClusterNoSize[x] = (((NDrops * ((DropProb[x] - DropProb[x - 1])))) / x);

                        if (k / x >= 1)
                            ClusterNoSize[x] = ClusterNoSize[x] + (k / x);

                        if (k / x <= 1)
                            ClusterNoSize[x] = ClusterNoSize[x] + (k / x);

                        w = w + (ClusterNoSize[x] * x);
                        l = (NDrops * (DropProb[x]));
                        k = l - w;
                        NoClust = NoClust + ClusterNoSize[x];
                    }

                    if (eqn != 4)
                    {
                        if (DropProb[x - 1] == 1)
                        {
                            ClusterNoSize[x] = -1;
                            ClusterNoSize[x - 1] = ClusterNoSize[x - 2];
                            LargeClust = x - 1;
                            ClustScale = (ClustScale / LargeClust) / (LargeClust - 1);
                            x = 1000;
                        }
                    }

                }//end for

                for (x = 1; x <= LargeClust; x++)
                {
                    ClusterNoSize[x] = ClusterNoSize[x] / ((x - 1) * ClustScale + 1);
                }//end for

                //RANDOM NUMBER
                Random rnd = new Random(1);
                //Introduce pitting
                ClusterNoSize[0] = NDrops;

                //totally wet
                if (NoCNoD != 0)
                {
                    if (NoCNoD < 25)
                    {
                        for (p = 1; p <= NoPits; p++)
                        {
                            PitString[q, 0, p] = 1;
                            ClustSize[1, p] = NDrops;
                            CellSize[p] = 1;
                            wave0[p] = (ClustSize[1, p] * DropVol) / CellSize[p];
                            PitString[q, 1, p] = 1;
                            PitString[q, 2, p] = ((ClustSize[1, p] * DropVol) / CellSize[p]);
                        }
                    }
                }

                //total dry
                if (NoCNoD == 0)
                {
                    for (p = 1; p <= NoPits; p++)
                    {
                        PitString[q, 0, p] = 0;
                        ClustSize[1, p] = 0;
                        CellSize[p] = 0;
                        wave0[p] = 0;
                        PitString[q, 1, p] = 0;
                        PitString[q, 2, p] = 0;
                    }

                }

                // Isolated droplets
                if (NoCNoD > 300)
                {
                    for (p = 1; p <= NoPits; p++)
                    {
                        y = (enoise(1) + 1) / 2;
                        for (x = 1; x <= 1000; x++)
                        {
                            if (y <= DropProb[x])
                            {
                                ClustSize[0, p] = x;
                                l = (enoise(1) + 1.0) / 2.0;
                                PitData[p] = Convert.ToInt32(Math.Ceiling(l * ClusterNoSize[x]));
                                CellSize[p] = x * DropArea;
                                PitString[q, 1, p] = x * DropArea;
                                PitString[q, 2, p] = DropRadius * 2.0 / 3.0;
                                x = 1000;
                            }
                        }
                    }

                    g = 1;

                    for (p = 1; p <= NoPits; p++)
                    {
                        PitString[q, 0, p] = p;
                        PitSDrop[p] = 0;
                        l = 0;
                        k = 0;
                        PitString[q, 0, p] = g;

                        for (z = 1; z <= NoPits; z++)
                        {
                            if (ClustSize[1, p] == ClustSize[1, z])
                                if (PitData[p] == PitData[z])
                                    if (p != z)
                                    {
                                        PitSDrop[p] = PitSDrop[p] + 1;
                                        if (p > z)
                                        {
                                            l = 1;
                                            k = k + 1;
                                            if (k == 1)
                                                PitString[q, 0, p] = PitString[q, 0, z];
                                        }
                                    }

                        }
                        g = g + 1 - Convert.ToInt32(l);
                    }
                }//ENDIF NoCNoD > 300

                //Percolation Droplets
                if (NoCNoD <= 300)
                {
                    if (NoCNoD >= 25)
                    {
                        ClustSize[2, 0] = 0;
                        for (p = 1; p <= NoPits; p++)
                        {
                            y = (enoise(1) + 1.0) / 2.0;
                            for (x = 1; x <= 1000; x++)
                            {
                                if (y <= DropProb[x])
                                {
                                    ClustSize[0, p] = x;
                                    ClustSize[1, p] = ((ClustScale * (x - 1.0)) + 1.0) * x;
                                    ClustSize[2, p] = ClustSize[2, p - 1] + ClustSize[1, p];
                                    l = (enoise(1) + 1.0) / 2.0;
                                    PitData[p] = Convert.ToInt32(Math.Ceiling(l * ClusterNoSize[x]));
                                    PitData[p] = Convert.ToInt32(Math.Ceiling(l * ClusterNoSize[x]));
                                    x = 1000;
                                } //endif
                            }//endfor
                        }//endfor

                        g = 1;
                        for (p = 1; p < NoPits; p++)
                        {
                            PitString[q, 0, p] = p;
                            PitSDrop[p] = 0;
                            l = 0;
                            k = 0;
                            PitString[q, 0, p] = g;
                            for (z = 1; z < NoPits; z++)
                            {
                                if (ClustSize[0, p] == ClustSize[0, z])
                                {
                                    if (PitData[p] == PitData[z])
                                    {
                                        if (p != z)
                                        {
                                            PitSDrop[p] = PitSDrop[p] + 1;
                                            if (p > z)
                                            {
                                                l = 1;
                                                k = k + 1;
                                                if (k == 1)
                                                {
                                                    PitString[q, 0, p] = PitString[q, 0, z];
                                                    ClustSize[2, p] = ClustSize[2, p] - ClustSize[1, p];
                                                }//endif
                                            }//endif
                                        } //endif
                                    }//endif
                                }//endif
                            }//end for
                            g = g + 1 - Convert.ToInt32(l);
                        }//end for
                    }//endif
                }//endif

                if (ClustSize[2, 1000] >= NDrops)
                    eqn = 7;

                //large clustered droplets
                if (eqn == 7)
                {
                    if (NoCNoD >= 25)
                    {
                        g = 1;
                        m = 0;
                        n = 0;

                        ClustSize[2, 0] = 0;
                        ClustSize[3, 0] = 0;

                        for (p = 1; p <= NoPits; p++)
                        {
                            y = (enoise(1) + 1) / 2;
                            for (x = 1; x <= 1000; x++)
                            {
                                if (y <= DropProb[x])
                                {
                                    if (m != 1)
                                    {
                                        ClustSize[0, p] = 0;
                                        ClustSize[1, p] = ((ClustScale * (x - 1.0)) + 1.0) * x;
                                        ClustSize[2, p] = ClustSize[2, p - 1] + ClustSize[1, p];
                                        ClustSize[3, p] = ClustSize[3, p - 1] + (ClustSize[1, p] / NDrops);
                                        if (ClustSize[2, p] >= NDrops)
                                        {
                                            ClustSize[2, p] = NDrops;
                                            ClustSize[1, p] = NDrops - ClustSize[2, p - 1];
                                        }//endif (ClustSize[2, p] >= NDrops)

                                        l = (enoise(1) + 1.0) / 2.0;
                                        PitData[p] = Convert.ToInt32(Math.Ceiling(l * ClusterNoSize[x]));
                                        x = 1000;
                                    }//endif (m!=1)
                                }//endif (y=DropProb)
                            }//endfor (x=1)

                            if (m != 1)
                            {
                                PitString[q, 0, p] = 0;
                                PitSDrop[p] = 0;
                                l = 0;
                                k = 0;
                                PitString[q, 0, p] = g;
                                for (z = 1; z <= NoPits; z++)
                                {
                                    if (ClustSize[0, p] == ClustSize[0, z])
                                    {
                                        if (PitData[p] == PitData[z])
                                        {
                                            if (p > z)
                                            {
                                                l = 1;
                                                k = k + 1;
                                                if (k == 1)
                                                {
                                                    PitString[q, 0, p] = PitString[q, 0, z];
                                                    ClustSize[1, p] = ClustSize[1, z];
                                                    ClustSize[2, p] = ClustSize[2, p] - ClustSize[1, p];
                                                    ClustSize[3, p] = ClustSize[3, p] - (ClustSize[1, p] / NDrops);
                                                }//endif (k ==1)
                                            }//endif (p > z)
                                        }//endif (PitData[p] == PitData[z])
                                    }//endif (ClustSize[0, p] == ClustSize[0, z])
                                }//endfor (z = 1)
                                g = g + 1 - 1;
                                n = n + 1;
                            }//endif (m!=1)
                            if (ClustSize[2, p] >= NDrops)
                            {
                                m = 1;
                            }//endif  (ClustSize[2, p] >= NDrops)
                        }//endfor (p=1)
                    }//endif (NoCNoD >= 25)

                    if (m == 1)
                    {
                        for (k = n + 1; k <= NoPits; k++)
                        {
                            y = (enoise(1) + 1) / 2;
                            for (x = 1; x <= n; x++)
                            {
                                if (y <= ClustSize[3, x])
                                {
                                    PitString[q, 0, Convert.ToInt32(k)] = x;
                                    PitData[x] = PitSDrop[x] + 1;
                                    ClustSize[1, Convert.ToInt32(k)] = ClustSize[1, x];
                                    x = Convert.ToInt32(n);
                                }//endif (y <= ClustSize[3, x])
                            }//endfor (x=1)
                        }//endfor (k=n+1)
                    }//endif (m==1)

                }//endif (eqn == 7)


                if (NoCNoD <= 300)
                {
                    if (NoCNoD >= 25)
                    {
                        CellSize[0] = 0;
                        PitString[q, 2, 0] = 0;
                        PitString[q, 1, 0] = 0;
                        for (x = 1; x < NoPits; x++) //for (x = 1; x < 1001; x++)
                        {
                            CellSize[x] = (1.0 - (ClustSize[1, x] / NDrops * 0.291538589728257)) * ClustSize[1, x] * DropArea;
                            CellSize[0] = CellSize[0] + CellSize[x];
                            wave0[x] = (ClustSize[1, x] * DropVol) / CellSize[x];
                            PitString[q, 1, 0] = CellSize[0] + CellSize[x];
                            PitString[q, 1, x] = (1 - (ClustSize[1, x] / NDrops * 0.291538589728257)) * ClustSize[1, x] * DropArea;
                            PitString[q, 2, x] = ((ClustSize[1, x] * DropVol) / CellSize[x]) * (2.0 / 3.0);
                            if (Double.IsNaN(PitString[q, 2, x]))
                            {
                                PitString[q, 2, x] = 0;
                            }
                            PitString[q, 2, 0] = PitString[q, 2, 0] + PitString[q, 2, x];
                        }
                    }
                }

                PitString[q, 2, 0] = 0;
                PitString[q, 1, 0] = 0;
                for (x = 1; x < NoPits; x++)
                {
                    PitString[q, 1, 0] = PitString[q, 1, 0] + CellSize[x];
                    PitString[q, 2, 0] = PitString[q, 2, 0] + PitString[q, 2, x];

                }

                double testValue = PitString[q, 2, 0];
                PitString[q, 2, 0] = (PitString[q, 2, 0] / 1000.0);// * 2.0 / 3.0;
                double testValue1 = PitString[q, 2, 0];


            }

            return PitString;

        }

        public void pit(ref List<List<enviroP>> _enviroP_List, AnalysisTask _task, double[,,] _pitString, ref CorrosionData cd)
        {
            double CrDrop, DropArea, NoCrys, a, b, e, g, crysize, ClustAmpi, i, st, pit, tim, tt, derv, rainstart;
            double pitprob, temp, PRH, wetness, TfromStart, ATMtemp, TOW, oxyconc, oxyconcA, DO2, Lim2024, Lim1010, ClustD;
            double stdev, pitvariance, pitbase, pits, z, q, NDD2024, NDD1010; //Condstart, Meltstart, TotCon*/;
            double DropVol, /*DropVolA,*/ Steeli;

            rainstart = 0;

            double TotCon = 0;

            int z1 = _enviroP_List[0].Count() + 10;

            int runs = 1;

            int periodCount = (int)_enviroP_List[0][0].col17;
            double[] hours = new double[1005];
            double[] pitvol = new double[1005];
            double[] pitsurf = new double[1005];
            double[] pitclust = new double[z1];

            double[,] pithisttotal = new double[60, 1000];
            double[] pithistogram = new double[60];
            double[] pitintermediate = new double[20000];
            double[,] wave0 = new double[5000, 3];
            double[,] bintotal = new double[60, 4];
            double[] largepit = new double[500];
            double[,,] wetdam = new double[200, z1, 2];
            double[] dam = new double[500];
            double[,,] Drops = new double[(int)_enviroP_List[0][0].col0 + 1, (int)_enviroP_List[0][0].col16 + 1, 4];
            double[,,] SteelWL = new double[(int)(_enviroP_List[0][0].col0) + 1, 3, 2];
            double[,] SteelCorr = new double[periodCount + 3, runs + 1];

            double[,] pitdiam = new double[(int)(_enviroP_List[0][0].col0) + 1, (int)_enviroP_List[0][0].col16 + 1];
            double[] pitdiamtotal = new double[20000];
            DropVol = 1.289260401394123;
            st = 0;
            List<double> pits1 = new List<double>();
            List<double> pits2 = new List<double>();
            List<double> pits3 = new List<double>();
            pit = 0;
            int l, r, t, p;
            l = r = 0;
            a = 0;

            try
            {
                for (r = 1; r <= runs; r++)
                {
                    SteelWL[l, 2, r] = 1;
                    q = 0;
                    l = 1;
                    tt = 0;
                    st = 0;
                    do
                    {
                        b = 0;
                        e = 0;
                        g = 0;

                        pits = 0;
                        p = 0;
                        z = 0;
                        crysize = _pitString[l, 0, 0];

                        SteelWL[l, 2, r] = 1;
                        t = 1;
                        SteelWL[l, 0, r] = 0;

                        //Condstart = 0;
                        rainstart = 0;
                        //pitdiam[l, 0] = 1;
                        do
                        {
                            // CORROSION PREDITION
                            temp = _enviroP_List[l][t].col0;
                            PRH = _enviroP_List[l][t].col1;
                            tim = _enviroP_List[l][t].col2;
                            TotCon = _enviroP_List[l][t].col3;
                            CrDrop = _enviroP_List[l][t].col4;
                            DropArea = _enviroP_List[l][t].col5;
                            wetness = _enviroP_List[l][t].col6;
                            TfromStart = _enviroP_List[l][t].col7;
                            NoCrys = _enviroP_List[l][t].col8;
                            ATMtemp = _enviroP_List[l][t].col9;
                            TOW = _enviroP_List[l][t].col10;
                            oxyconc = _enviroP_List[l][t].col11;
                            oxyconcA = _enviroP_List[l][t].col12;
                            DO2 = _enviroP_List[l][t].col13;
                            Lim2024 = _enviroP_List[l][t].col14;
                            Lim1010 = _enviroP_List[l][t].col15;
                            PRH = _enviroP_List[l][t].col17;

                            DropVol = (_enviroP_List[l][t].col4) / (_enviroP_List[0][l].col5);
                            NDD2024 = (1.30964 - 0.20903 * TotCon + 0.01702 * (Math.Pow(TotCon, 2)));

                            NDD1010 = (1.0016 - 0.15986 * TotCon + 0.01302 * (Math.Pow(TotCon, 2))) * 0.246937911;

                            if (_enviroP_List[l][t].col11 > 0)
                            {
                                derv = (_enviroP_List[l][t].col6 - _enviroP_List[l][t - 1].col6) / (_enviroP_List[l][t].col2 / 24.0);
                                if (derv >= 10000.0)
                                {
                                    rainstart = 1;
                                }
                            }


                            // 	Steel General Corrosion

                            if (rainstart == 0)
                            {
                                ClustD = DropVol * _pitString[l, 2, 0];
                            }
                            else
                            {
                                ClustD = NDD1010 / 10;
                            }


                            if (Double.IsNaN(ClustD))
                            {
                                ClustD = 0;
                            }

                            ClustAmpi = (Lim1010 * NDD1010) / (10.00 * ClustD);

                            if (Double.IsInfinity(ClustAmpi))
                            {
                                ClustAmpi = 0;
                            }

                            Steeli = (wetness / 100.00) * (ClustAmpi / 1.00e6) * ((1.11315690101385E-5 + 1.78129309341292E-5 * Math.Exp(-st / 4.23880161232376)) / 2.89441e-05);
                            SteelWL[l, 0, r] = SteelWL[l, 0, r] + (Steeli * (_enviroP_List[l][t].col2 * 3600.00));
                            st = st + (Steeli * (_enviroP_List[l][t].col2 * 3600.00));
                            SteelWL[l, 1, r] = st * 55.845 / (96485.00 * 3.00);
                            SteelWL[l, 2, r] = (1.11315690101385E-5 + 1.78129309341292E-5 * Math.Exp(-st / 4.23880161232376)) / 2.89441e-05;
                            tt = tt + 1;
                            SteelCorr[Convert.ToInt32(tt), r] = SteelWL[l, 1, r];
                            double testValue = SteelWL[l, 1, r];
                            SteelCorr[Convert.ToInt32(tt), 0] = _enviroP_List[l][t].col7;

                            if (t == 497)
                            {

                            }
                            //PIT PROBABILITY CALCS (AA2024)
                            if (rainstart == 0)
                            {
                                pitprob = _enviroP_List[l][t].col16 * (1 - Math.Pow(0.57414, (wetness / 100.00))) * 2.34817 * (_enviroP_List[l][t].col2 * 3600.00);
                                if (pitprob <= 0)
                                {
                                    pitprob = 0.00;
                                }
                                //else //Tony's latest version has added this else statement, with it, no pits will plot because probability is always 0?
                                //{
                                //    pitprob = 0.00;
                                //}

                                //PROBABILISTIC PITTING
                                stdev = pitprob * 0.5;
                                if (stdev != 0)
                                {
                                    pitvariance = (normalDist.Sample()) * stdev;
                                }
                                else
                                {
                                    pitvariance = 0;
                                }
                                pitbase = (pitprob + pitvariance);
                                if (pitbase < 0)
                                {
                                    pitbase = 0.00;
                                }

                                if (pitbase > 1)
                                {
                                    pitbase = 1.00;
                                }
                                //pitbase = pitbase * 0.5;
                                //PIT GENERATION		
                                pits = (enoise(0.5) + 0.5);//(rnd.NextDouble()); //pits = enoise(0.5) + 0.5
                                if (pitprob != 0)
                                {

                                }
                                if (pits <= pitbase)
                                {
                                    p = p + 1;
                                    b = b + 1;
                                    a = _pitString[l, 0, p];
                                    pit = pit + 1;

                                    pitvol[p] = 0.000019;
                                    pitsurf[p] = 2.82961E-06;

                                    if (a < b)
                                    {
                                        Drops[l, Convert.ToInt32(a), 0] = Drops[l, Convert.ToInt32(a), 0] + l;
                                        Drops[l, Convert.ToInt32(a), 2] = Drops[l, Convert.ToInt32(a), 2] + 2.82961E-06;
                                        b = b - 1;
                                    }
                                    else
                                    {
                                        Drops[l, Convert.ToInt32(b), 0] = l;
                                        Drops[l, Convert.ToInt32(b), 2] = 2.82961E-06;
                                        Drops[l, Convert.ToInt32(b), 1] = _pitString[l, 1, Convert.ToInt32(b)];
                                    }
                                }


                            }

                            //PIT GROWTH CURRENT	
                            for (e = 1; e <= b; e += 1)
                            {
                                ClustD = DropVol * _pitString[l, 2, Convert.ToInt32(e)];
                                if (rainstart == 0)
                                {
                                    ClustAmpi = (Lim2024 * NDD2024) / (10 * ClustD);
                                }
                                else
                                {
                                    ClustAmpi = 0;
                                }

                                i = ClustAmpi * Drops[l, Convert.ToInt32(e), 1];

                                if (Drops[l, Convert.ToInt32(e), 2] * 0.035 > ((((ClustAmpi) / 1e6) - 1.75e-6) * (Drops[l, Convert.ToInt32(e), 1])) + Drops[l, Convert.ToInt32(e), 2] * 0.00019)
                                {
                                    Drops[l, Convert.ToInt32(e), 3] = ((((ClustAmpi) / 1e6) - 1.75e-6) * Drops[l, Convert.ToInt32(e), 1]) / Drops[l, Convert.ToInt32(e), 2];
                                }
                                else
                                {
                                    Drops[l, Convert.ToInt32(e), 3] = 0.0035;
                                }

                                //PIT GROWTH
                                Drops[l, Convert.ToInt32(e), 2] = 0;
                                if (p > 0)
                                {
                                    for (z = 1; z <= p; z++)
                                    {
                                        if (_pitString[l, 0, Convert.ToInt32(z)] == e)
                                        {
                                            pitvol[Convert.ToInt32(z)] = pitvol[Convert.ToInt32(z)] + pitsurf[Convert.ToInt32(z)] * ((_enviroP_List[l][t].col2 * 3600.00) * (Drops[l, Convert.ToInt32(e), 3] + 0.00019));
                                            pitsurf[Convert.ToInt32(z)] = (Math.Pow((Math.Pow((pitvol[Convert.ToInt32(z)] * .000015906186), (1.0 / 3.0))), 2)) * 2.00 * 3.1415927;
                                            Drops[l, Convert.ToInt32(e), 2] = Drops[l, Convert.ToInt32(e), 2] + pitsurf[Convert.ToInt32(z)];
                                            pitdiam[l, Convert.ToInt32(z)] = (Math.Pow((pitvol[Convert.ToInt32(z)] * .000015906186), (1.0 / 3.0))) * 20000;
                                            pitdiamtotal[(Convert.ToInt32(q) + Convert.ToInt32(z))] = (Math.Pow((pitvol[Convert.ToInt32(z)] * .000015906186), (1.0 / 3.0))) * 20000;
                                            pitintermediate[(Convert.ToInt32(q) + Convert.ToInt32(z))] = (Math.Pow((pitvol[Convert.ToInt32(z)] * .000015906186), (1.0 / 3.0))) * 20000;
                                        }//endif
                                    }//endfor
                                }//endif
                            }//endfor

                            //loops
                            t += 1;
                        } while (t < _enviroP_List[l].Count() && _enviroP_List[l][t].col5 > 0);
                        q = q + p;
                        l += 1;
                        t = 1;
                    } while (l <= _enviroP_List[0][0].col0);//do-while

                }//end for

            }
            catch (Exception exp)
            {

            }

            List<double> pit_int = new List<double>();


            pit_int = pitintermediate.Where(val => (val > 0) && !double.IsInfinity(val) && !double.IsNaN(val)).ToList();



            MathNet.Numerics.Statistics.Histogram xxx = new MathNet.Numerics.Statistics.Histogram(pit_int, 59, 10.0, 305.0);

            List<double> c_y = new List<double>();
            List<double> c_x = new List<double>();
            List<double> c_x_bottom = new List<double>();

            c_x.Add(10);

            for (int c1 = 0; c1 < xxx.BucketCount; c1++)
            {
                c_y.Add(xxx[c1].Count);

                c_x_bottom.Add(0);
                if (c1 > 0)
                    c_x.Add(c_x[c1 - 1] + 5);
            }

            List<double> c_y1 = new List<double>();

            c_y1 = c_y.Where(val => (val > 0.0)).ToList();


            cd.PitDiameter.AddRange(c_x);
            cd.PitCount.AddRange(c_y);

            double largestPitCount = cd.PitCount.Last();
            double largestOtherPit = 0;
            if (largestPitCount > 20)
            {

                foreach (double size in cd.PitCount)
                {
                    if (size < largestOtherPit)
                    {
                        largestOtherPit = size;
                    }
                }
                cd.PitCount.RemoveAt(cd.PitCount.Count - 1);
                if (largestOtherPit == 0)
                {
                    cd.PitCount.Add(0);
                }
                else if (largestOtherPit < 20)
                {
                    cd.PitCount.Add(largestOtherPit - 1);
                }
                else
                {
                    cd.PitCount.Add(20);
                }
            }


            List<double> steel = new List<double>();
            List<double> enviro_4 = new List<double>();
            steel.Add(0);
            enviro_4.Add(0);
            int steelIndex = 0;
            int dayIndex = 1;
            foreach (enviroP environs in _enviroP_List[0])
            {
                try
                {
                    if (steelIndex < _enviroP_List[0].Count)
                    {
                        double environValue = environs.col4;
                        if (environValue != 0)
                        {
                            steel.Add(SteelWL[steelIndex, 1, 1]);
                            //enviro_4.Add(environs.col4);
                            enviro_4.Add(dayIndex);
                            dayIndex++;
                        }
                        else
                        {

                        }
                        if (steelIndex < ((_enviroP_List[0][0].col0)))
                        {
                            steelIndex++;
                        }
                    }
                }
                catch (Exception error)
                {

                }
            }

            cd.SteelWeightLoss.AddRange(steel);
            cd.SteelLossTime.AddRange(enviro_4);
        }

        public double getLocationStats(int Location, int month, string paramRequest)
        {
            double Deposit = 0;
            double Sea = 0;
            double AmmNit = 0;
            double AmmSul = 0;
            double RainDep = 0;
            double RainConc = 0;
            double NaCl = 0;
            double MgCl2 = 0;
            double Na2S04 = 0;
            double CaC12 = 0;
            double KCI = 0;
            double MgSO4 = 0;
            double K2SO4 = 0;
            double CaSO4 = 0;
            double total = 0;
            double HSMass = 0;
            double HNMass = 0;
            PollutionModel p = new PollutionModel();

            if (Location == 1)
            {
                p = PollutionData[Location + "-" + month];
                Deposit = p.Deposit;
                AmmNit = p.AmmNit;
                AmmSul = p.AmmSul;
                RainConc = p.RainConc;
                NaCl = p.NaCl;
                Sea = p.Sea;
                RainDep = p.RainDep;
                MgCl2 = p.MgCl2;
                Na2S04 = p.Na2S04;
                CaC12 = p.CaC12;
                KCI = p.KCI;
                MgSO4 = p.MgSO4;
                K2SO4 = p.K2SO4;
                CaSO4 = p.CaSO4;
                total = p.total;
            }
            else if (Location == 2)
            {
                p = PollutionData[Location + "-" + month];
                Deposit = p.Deposit;
                AmmNit = p.AmmNit;
                AmmSul = p.AmmSul;
                RainConc = p.RainConc;
                NaCl = p.NaCl;
                Sea = p.Sea;
                RainDep = p.RainDep;
                MgCl2 = p.MgCl2;
                Na2S04 = p.Na2S04;
                CaC12 = p.CaC12;
                KCI = p.KCI;
                MgSO4 = p.MgSO4;
                K2SO4 = p.K2SO4;
                CaSO4 = p.CaSO4;
                total = p.total;
            }
            else if (Location == 3)
            {
                p = PollutionData[Location + "-" + month];
                Deposit = p.Deposit;
                AmmNit = p.AmmNit;
                AmmSul = p.AmmSul;
                RainConc = p.RainConc;
                NaCl = p.NaCl;
                Sea = p.Sea;
                RainDep = p.RainDep;
                MgCl2 = p.MgCl2;
                Na2S04 = p.Na2S04;
                CaC12 = p.CaC12;
                KCI = p.KCI;
                MgSO4 = p.MgSO4;
                K2SO4 = p.K2SO4;
                CaSO4 = p.CaSO4;
                total = p.total;
            }
            else if (Location == 4)
            {
                p = PollutionData[Location + "-" + month];
                Deposit = p.Deposit;
                AmmNit = p.AmmNit;
                AmmSul = p.AmmSul;
                RainConc = p.RainConc;
                NaCl = p.NaCl;
                Sea = p.Sea;
                RainDep = p.RainDep;
                MgCl2 = p.MgCl2;
                Na2S04 = p.Na2S04;
                CaC12 = p.CaC12;
                KCI = p.KCI;
                MgSO4 = p.MgSO4;
                K2SO4 = p.K2SO4;
                CaSO4 = p.CaSO4;
                total = p.total;
            }
            else if (Location == 5)
            {
                p = PollutionData[Location + "-" + month];
                Deposit = p.Deposit;
                AmmNit = p.AmmNit;
                AmmSul = p.AmmSul;
                RainConc = p.RainConc;
                NaCl = p.NaCl;
                Sea = p.Sea;
                RainDep = p.RainDep;
                MgCl2 = p.MgCl2;
                Na2S04 = p.Na2S04;
                CaC12 = p.CaC12;
                KCI = p.KCI;
                MgSO4 = p.MgSO4;
                K2SO4 = p.K2SO4;
                CaSO4 = p.CaSO4;
                total = p.total;
            }
            else if (Location == 6)
            {
                p = PollutionData[Location + "-" + month];
                Deposit = p.Deposit;
                AmmNit = p.AmmNit;
                AmmSul = p.AmmSul;
                RainConc = p.RainConc;
                NaCl = p.NaCl;
                Sea = p.Sea;
                RainDep = p.RainDep;
                MgCl2 = p.MgCl2;
                Na2S04 = p.Na2S04;
                CaC12 = p.CaC12;
                KCI = p.KCI;
                MgSO4 = p.MgSO4;
                K2SO4 = p.K2SO4;
                CaSO4 = p.CaSO4;
                total = p.total;
            }
            else if (Location == 7)
            {
                p = PollutionData[Location + "-" + month];
                Deposit = p.Deposit;
                AmmNit = p.AmmNit;
                AmmSul = p.AmmSul;
                RainConc = p.RainConc;
                NaCl = p.NaCl;
                Sea = p.Sea;
                RainDep = p.RainDep;
                MgCl2 = p.MgCl2;
                Na2S04 = p.Na2S04;
                CaC12 = p.CaC12;
                KCI = p.KCI;
                MgSO4 = p.MgSO4;
                K2SO4 = p.K2SO4;
                CaSO4 = p.CaSO4;
                total = p.total;
            }
            else if (Location == 8)
            {
                p = PollutionData[Location + "-" + month];
                Deposit = p.Deposit;
                AmmNit = p.AmmNit;
                AmmSul = p.AmmSul;
                RainConc = p.RainConc;
                NaCl = p.NaCl;
                Sea = p.Sea;
                RainDep = p.RainDep;
                MgCl2 = p.MgCl2;
                Na2S04 = p.Na2S04;
                CaC12 = p.CaC12;
                KCI = p.KCI;
                MgSO4 = p.MgSO4;
                K2SO4 = p.K2SO4;
                CaSO4 = p.CaSO4;
                total = p.total;
            }
            else if (Location == 9)
            {
                p = PollutionData[Location + "-" + month];
                Deposit = p.Deposit;
                AmmNit = p.AmmNit;
                AmmSul = p.AmmSul;
                RainConc = p.RainConc;
                NaCl = p.NaCl;
                Sea = p.Sea;
                RainDep = p.RainDep;
                MgCl2 = p.MgCl2;
                Na2S04 = p.Na2S04;
                CaC12 = p.CaC12;
                KCI = p.KCI;
                MgSO4 = p.MgSO4;
                K2SO4 = p.K2SO4;
                CaSO4 = p.CaSO4;
                total = p.total;
                HSMass = (double)p.HSMass;
                HNMass = (double)p.HNMass;
            }
            else
            {
                Deposit = 17.3;
                NaCl = 0.4;
                AmmNit = 0.025;
                AmmSul = 0.125;
                RainDep = 2.20775;
                RainConc = 5.32;
                Sea = 0;
                MgCl2 = 0;
                Na2S04 = 0;
                CaC12 = 0;
                KCI = 0;
                MgSO4 = 0;
                K2SO4 = 0;
                CaSO4 = 0;
                total = 0;
            }
            switch (paramRequest)
            {
                case "Deposit":
                    return Deposit;
                case "Sea":
                    return Sea;
                case "NaCl":
                    return NaCl;
                case "AmmNit":
                    return AmmNit;
                case "AmmSul":
                    return AmmSul;
                case "RainDep":
                    return RainDep;
                case "RainConc":
                    return RainConc;
                case "MgC12":
                    return MgCl2;
                case "Na2SO4":
                    return Na2S04;
                case "CaC12":
                    return CaC12;
                case "KCI":
                    return KCI;
                case "MgSO4":
                    return MgSO4;
                case "K2SO4":
                    return K2SO4;
                case "CaSO4":
                    return CaSO4;
                case "total":
                    return total;
                case "HSMass":
                    return HSMass;
                case "HNMass":
                    return HNMass;
                default:
                    return 0;
            }
        }

        public double enoise(double value)
        {
            double rangeMin = value * -1;
            double rangeMax = value;
            double randomValue = rangeMin + (rangeMax - rangeMin) * r.NextDouble();
            return randomValue;
        }

        public void newCorr(List<combinedData> sensorData, ref CorrosionData cd)
        {
            List<double> corr1List = new List<double>();
            List<double> corr2List = new List<double>();
            List<DateTime> dateList = new List<DateTime>();

            if (sensorData.Count > 0)
            {
                double firstItemResistValue1 = ((0.00000012911 / ((0.0000000006796) * sensorData[0].corrTemp + 0.00000012917)) * sensorData[0].corr1) - (0.00000012911 * 0.065 / (0.00005 * 0.005));//e4
                double firstItemResistValue2 = ((0.00000012911 / ((0.0000000006796) * sensorData[0].corrTemp + 0.00000012917)) * sensorData[0].corr2) - (0.00000012911 * 0.065 / (0.00005 * 0.005));//e4
                double firstItemExposed1 = ((0.00000012911 / ((0.0000000006796) * sensorData[0].corrTemp + 0.00000012917)) * sensorData[0].corr1) - firstItemResistValue1; //e7
                double firstItemExposed2 = ((0.00000012911 / ((0.0000000006796) * sensorData[0].corrTemp + 0.00000012917)) * sensorData[0].corr2) - firstItemResistValue2; //e7

                double firstItemTemp = sensorData[0].corrTemp;

                foreach (combinedData sensorDataItem in sensorData)
                {
                    double corr1Value = sensorDataItem.corr1;
                    double corr2Value = sensorDataItem.corr1;
                    double temp = sensorDataItem.corrTemp;

                    double resist1 = (0.00000012911 / ((0.0000000006796) * temp + 0.00000012917)) * corr1Value;
                    double resist2 = (0.00000012911 / ((0.0000000006796) * temp + 0.00000012917)) * corr2Value;

                    double exposed1 = resist1 - firstItemResistValue1;
                    double exposed2 = resist2 - firstItemResistValue2;

                    double sectionalLoss1 = 51 - (firstItemExposed1 * 51 / exposed1);
                    double sectionalLoss2 = 50 - (firstItemExposed2 * 51 / exposed2);

                    corr1List.Add(sectionalLoss1);
                    corr2List.Add(sectionalLoss2);

                    dateList.Add(sensorDataItem.col8);

                }

                List <CorrosionDataPoint> corr1Values = new List < CorrosionDataPoint >();
                List<CorrosionDataPoint> corr2Values = new List<CorrosionDataPoint>();

                double corr1Total = 0;
                double corr2Total = 0;
                int items = 0;
                int days = 1;

                DateTime currentDate = dateList[0];
                for (int i = 0;i < dateList.Count;i++)
                {
                    if (dateList[i].Day == currentDate.Day)
                    {
                        corr1Total += corr1List[i];
                        corr2Total += corr2List[i];
                        items++;
                    }
                    else
                    {
                        CorrosionDataPoint cdp1 = new CorrosionDataPoint();
                        cdp1.Value = (corr1Total / items);
                        cdp1.Series = days;
                        corr1Values.Add(cdp1);

                        CorrosionDataPoint cdp2 = new CorrosionDataPoint();
                        cdp2.Value = (corr2Total / items);
                        cdp2.Series = days;
                        corr2Values.Add(cdp2);

                        corr1Total = 0;
                        corr2Total = 0;
                        items = 0;
                        currentDate = dateList[i];
                        days++;

                        corr1Total += corr1List[i];
                        corr2Total += corr2List[i];
                        items++;
                    }

                }
                if (items > 0)
                {
                    CorrosionDataPoint cdp1 = new CorrosionDataPoint();
                    cdp1.Value = (corr1Total / items);
                    cdp1.Series = days;
                    corr1Values.Add(cdp1);

                    CorrosionDataPoint cdp2 = new CorrosionDataPoint();
                    cdp2.Value = (corr2Total / items);
                    cdp2.Series = days;
                    corr2Values.Add(cdp2);
                }

                cd.Corr1Values = corr1Values;
                cd.Corr2Values = corr2Values;
            }

        }
    }

}
