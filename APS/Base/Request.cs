
namespace APS.Base
{
    public class Request
    {
        public int SourceId { get; set; }
        public string? RequestId { get; set; }
        public double ArrivalTime { get; set; }
        public double ServiceTime { get; set; }
        public double DepartureTime { get; set; }
    }

}
