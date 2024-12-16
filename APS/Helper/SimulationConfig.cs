namespace APS.Helper
{
    public class SimulationConfig
    {
        public int NumberOfSources { get; set; }
        public int NumberOfBuffers { get; set; }
        public int BufferCapacity { get; set; }
        public double MeanServiceTime { get; set; }
        public double MinInterArrivalTime { get; set; }
        public double MaxInterArrivalTime { get; set; }
        public int MaxRequests { get; set; }
        public int NumberOfDevices { get; set; }
        public double ConfidenceLevel { get; set; }
        public double RelativePrecision { get; set; } 
    }


    public class ConfigRoot
    {
        public SimulationConfig SimulationConfig { get; set; }
    }

}
