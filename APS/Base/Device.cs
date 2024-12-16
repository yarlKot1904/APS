namespace APS.Base
{
    public class Device
    {
        public int DeviceId { get; set; }
        public bool IsBusy { get; set; }
        public double RemainingTime { get; set; }
        public Request CurrentRequest { get; set; }

        public Device(int id)
        {
            DeviceId = id;
            IsBusy = false;
            RemainingTime = 0;
            CurrentRequest = null;
        }

        public void StartService(Request request, double serviceTime)
        {
            IsBusy = true;
            RemainingTime = serviceTime;
            CurrentRequest = request;
        }

        public void Update()
        {
            IsBusy = false;
            CurrentRequest = null;
        }
    }


}
