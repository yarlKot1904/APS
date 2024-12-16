using APS.Base;
using Buffer = APS.Base.Buffer;
using Microsoft.Extensions.Logging;

namespace APS.Simulators
{

    public class Simulation
    {
        public double CurrentTime { get; private set; }
        public List<Request> AllRequests { get; private set; }
        public List<Buffer> Buffers { get; private set; }
        public List<Device> Devices { get; private set; }
        public EventQueue EventQueue { get; private set; }
        public int MaxRequests { get; private set; }
        public bool AutoMode { get; set; }

        // Stats
        public int[] RefusalCounts { get; private set; }
        public double[] TotalServiceTimes { get; private set; }
        public double[] TotalWaitTimes { get; private set; }
        public double[] TotalRequestsTimes { get; private set; }
        public List<double>[] ServiceTimes { get; private set; }
        public List<double>[] BufferWaitTimes { get; private set; }
        public double[] DeviceUsageTimes { get; private set; }
        public double[] DeviceUsageTimesBySources { get; private set; }

        private Random rand;
        private VisualSimulator visualSimulator;
        private int bufferCapacityPerBuffer;
        private double meanServiceTime;
        private double minInterArrivalTime;
        private double maxInterArrivalTime;
        private int lastDeviceIndex = -1;
        private int lastBufferIndex = -1;


        //поиск
        public event Action<int, double, int> OnIterationCompleted;

        private int currentSampleSize;
        private double currentRefusalProbability;
        private double confidenceLevel;
        private double relativePrecision;
        private const double tAlpha = 1.643; // Для α = 0.9

        public int FinalSampleSize { get; private set; }
        public double FinalRefusalProbability { get; private set; }

        public Simulation(int numberOfSources, int numberOfBuffers, int numberOfDevices, int maxRequests,
                          double meanServiceTime, double minInterArrivalTime, double maxInterArrivalTime, int bufferCapacity, double confidenceLevel, double relativePrecision,
                          VisualSimulator visual)
        {
            CurrentTime = 0;
            AllRequests = new List<Request>();
            Buffers = new List<Buffer>();
            bufferCapacityPerBuffer = bufferCapacity;
            for (int i = 0; i < numberOfBuffers; i++)
                Buffers.Add(new Buffer(i, bufferCapacityPerBuffer));

            Devices = new List<Device>();
            for (int i = 0; i < numberOfDevices; i++)
                Devices.Add(new Device(i));

            EventQueue = new EventQueue();
            MaxRequests = maxRequests;

            visualSimulator = visual;

            RefusalCounts = new int[numberOfSources];
            TotalServiceTimes = new double[numberOfSources];
            TotalWaitTimes = new double[numberOfSources];
            TotalRequestsTimes = new double[numberOfSources];
            ServiceTimes = new List<double>[numberOfSources];
            BufferWaitTimes = new List<double>[numberOfSources];
            DeviceUsageTimes = new double[numberOfDevices];
            DeviceUsageTimesBySources = new double[numberOfSources];

            for (int i = 0; i < numberOfSources; i++)
            {
                ServiceTimes[i] = new List<double>();
                BufferWaitTimes[i] = new List<double>();
            }

            rand = new Random();

            this.meanServiceTime = meanServiceTime;
            this.minInterArrivalTime = minInterArrivalTime;
            this.maxInterArrivalTime = maxInterArrivalTime;
            this.confidenceLevel = confidenceLevel;
            this.relativePrecision = relativePrecision;

            this.currentSampleSize = maxRequests;

            ScheduleNextArrival();
        }

        public void DetermineOptimalSampleSize()
        {
            bool isConverged = false;
            int iteration = 0;

            while (!isConverged)
            {
                iteration++;
                double p0 = RunSimulationWithSampleSize(currentSampleSize);

                OnIterationCompleted?.Invoke(iteration, p0, currentSampleSize);

                iteration++;

                int N1 = CalculateRequiredSampleSize(p0);

                double p1 = RunSimulationWithSampleSize(N1);

                double difference = Math.Abs(p0 - p1);
                if (difference < relativePrecision * p0)
                {
                    FinalSampleSize = N1;
                    FinalRefusalProbability = p1;
                    isConverged = true;

                    OnIterationCompleted?.Invoke(iteration, p1, FinalSampleSize);
                }
                else
                {
                    currentSampleSize = N1;
                    currentRefusalProbability = p1;

                    OnIterationCompleted?.Invoke(iteration, p1, currentSampleSize);
                }
                if(iteration > 100)
                {
                    break;
                }
            }
        }



        public double RunSimulationWithSampleSize(int sampleSize)
        {
            this.MaxRequests = sampleSize;

            ResetSimulation();

            this.AutoMode = true;
            this.Step();
            double p = (double)this.RefusalCounts.Sum() / sampleSize;

            return p;
        }

        private void ResetSimulation()
        {
            this.CurrentTime = 0;
            this.AllRequests.Clear();
            foreach (var buffer in Buffers)
                buffer.Requests.Clear();
            foreach (var device in Devices)
                device.Update();

            Array.Clear(this.RefusalCounts, 0, this.RefusalCounts.Length);
            Array.Clear(this.TotalServiceTimes, 0, this.TotalServiceTimes.Length);
            Array.Clear(this.TotalWaitTimes, 0, this.TotalWaitTimes.Length);
            foreach (var list in this.ServiceTimes)
                list.Clear();
            foreach (var list in this.BufferWaitTimes)
                list.Clear();
            Array.Clear(this.DeviceUsageTimes, 0, this.DeviceUsageTimes.Length);

            this.EventQueue = new EventQueue();

            visualSimulator.ClearAll();
            ScheduleNextArrival();

        }

        private int CalculateRequiredSampleSize(double p)
        {
            double numerator = Math.Pow(tAlpha, 2) * (1 - p);
            double denominator = p * Math.Pow(relativePrecision, 2);
            int requiredN = (int)Math.Ceiling(numerator / denominator);
            return requiredN;
        }

        


        private void ScheduleNextArrival()
        {
            double interArrivalTime = rand.NextDouble() * (maxInterArrivalTime - minInterArrivalTime) + minInterArrivalTime;
            double arrivalTime = CurrentTime + interArrivalTime;

            EventQueue.AddEvent(arrivalTime, "arrival", () => GenerateRequest());
        }

        private void GenerateRequest()
        {
            if (AllRequests.Count >= MaxRequests)
                return;

            int sourceId = rand.Next(RefusalCounts.Length);
            string requestId = $"{sourceId + 1}.{AllRequests.Count + 1}";
            Request newRequest = new Request
            {
                SourceId = sourceId + 1,
                RequestId = requestId,
                ArrivalTime = CurrentTime
            };
            AllRequests.Add(newRequest);

            visualSimulator.AddRequest(requestId, sourceId);

            AssignRequest(newRequest);

            ScheduleNextArrival();
        }

        private void AssignRequest(Request request)
        {
            TotalRequestsTimes[request.SourceId - 1]++;

            Device availableDevice = FindFreeDevice();
            if (availableDevice != null)
            {
                SendToDevice(request, availableDevice);
            }
            else
            {
                Buffer bufferWithSpace = FindFreeBuffer();
                if (bufferWithSpace != null)
                {
                    lastBufferIndex = Buffers.IndexOf(bufferWithSpace);
                    bufferWithSpace.AddRequest(request);
                    visualSimulator.MoveRequestToBuffer(request.RequestId, bufferWithSpace.BufferId);
                }
                else
                {
                    Request oldestRequest = null;
                    Buffer bufferContainingOldest = null;

                    foreach (var buffer in Buffers)
                    {
                        if (buffer.Count > 0)
                        {
                            var firstRequest = buffer.Requests.Peek();
                            if (oldestRequest == null || firstRequest.ArrivalTime < oldestRequest.ArrivalTime)
                            {
                                oldestRequest = firstRequest;
                                bufferContainingOldest = buffer;
                            }
                        }
                    }

                    if (oldestRequest != null && bufferContainingOldest != null)
                    {
                        bufferContainingOldest.RemoveOldestRequest();
                        HandleRequestRefusal(oldestRequest);

                        bufferContainingOldest.AddRequest(request);
                        visualSimulator.MoveRequestToBuffer(request.RequestId, bufferContainingOldest.BufferId);
                    }
                    else
                    {
                        HandleRequestRefusal(request);
                    }
                }
            }
        }

        private void SendToDevice(Request request, Device device)
        {
            lastDeviceIndex = Devices.IndexOf(device);
            double serviceTime = Exponential(meanServiceTime);
            device.StartService(request, serviceTime);
            request.ServiceTime = serviceTime;
            request.DepartureTime = CurrentTime + serviceTime;

            ServiceTimes[request.SourceId - 1].Add(serviceTime);
            DeviceUsageTimes[device.DeviceId] += serviceTime;
            DeviceUsageTimesBySources[request.SourceId - 1] += serviceTime;

            double waitTime = CurrentTime - request.ArrivalTime;
            TotalWaitTimes[request.SourceId - 1] += waitTime;
            BufferWaitTimes[request.SourceId - 1].Add(waitTime);

            visualSimulator.MoveRequestToDevice(request.RequestId, device.DeviceId);

            EventQueue.AddEvent(CurrentTime + serviceTime, "departure", () => EndService(device));
        }

        private void HandleRequestRefusal(Request request)
        {
            RefusalCounts[request.SourceId - 1]++;
            visualSimulator.MoveRequestToRefuse(request.RequestId);
        }

        private void EndService(Device device)
        {
            device.Update();
            visualSimulator.ClearDevice(device.DeviceId);

            Request nextRequest = null;
            Buffer targetBuffer = null;

            foreach (var buffer in Buffers)
            {
                if (buffer.Count > 0)
                {
                    var lastRequest = buffer.Requests.Last();
                    if (nextRequest == null || lastRequest.ArrivalTime > nextRequest.ArrivalTime)
                    {
                        nextRequest = lastRequest;
                        targetBuffer = buffer;
                    }
                }
            }

            if (nextRequest != null && targetBuffer != null)
            {
                targetBuffer.RemoveLastRequest();
                SendToDevice(nextRequest, device);
                visualSimulator.ClearBuffer(targetBuffer.BufferId);
                visualSimulator.MoveRequestToDevice(nextRequest.RequestId, device.DeviceId);
            }
        }

        private Device? FindFreeDevice()
        {
            if (lastDeviceIndex == Devices.Count)
            {
                return Devices.FirstOrDefault(d => !d.IsBusy);
            }
            for (int i = lastDeviceIndex + 1; i < Devices.Count; i++)
            {
                if (!Devices[i].IsBusy)
                    return Devices[i];
            }
            for (int i = 0; i < lastDeviceIndex + 1; i++)
            {
                if (!Devices[i].IsBusy)
                    return Devices[i];
            }
            return null;
        }

        private Buffer? FindFreeBuffer()
        {
            if (lastBufferIndex == Buffers.Count)
            {
                return Buffers.FirstOrDefault(b => b.Count < b.Capacity);
            }
            for (int i = lastBufferIndex + 1; i < Buffers.Count; i++)
            {
                Buffer buffer = Buffers[i];
                if (buffer.Count < buffer.Capacity)
                    return buffer;
            }
            for (int i = 0; i < lastBufferIndex - 1; i++)
            {
                Buffer buffer = Buffers[i];
                if (buffer.Count < buffer.Capacity)
                    return buffer;
            }
            return null;
        }

        private double Exponential(double mean)
        {
            double u = rand.NextDouble();
            return -mean * Math.Log(1 - u);
        }

        public void Step()
        {
            if (AutoMode)
            {
                while (true)
                {
                    if (AllRequests.Count >= MaxRequests && !Devices.Any(d => d.IsBusy) && Buffers.All(b => b.Count == 0))
                    {
                        ShowResults();
                        return;
                    }

                    Event nextEvent = EventQueue.PopEvent();

                    if (nextEvent != null)
                    {
                        CurrentTime = nextEvent.Time;
                        nextEvent.Callback();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                if (AllRequests.Count >= MaxRequests && !Devices.Any(d => d.IsBusy) && Buffers.All(b => b.Count == 0))
                {
                    ShowResults();
                    return;
                }

                Event nextEvent = EventQueue.PopEvent();
                if (nextEvent != null)
                {
                    CurrentTime = nextEvent.Time;
                    nextEvent.Callback();
                }
                else
                {
                    return;
                }
            }
        }

        public void RunAuto()
        {
            AutoMode = true;
            Step();
        }

        public void ShowResults()
        {
            SimulationResultsForm resultsForm = new SimulationResultsForm(this);
            resultsForm.Show();
        }
    }



}
