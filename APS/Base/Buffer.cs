namespace APS.Base
{
    public class Buffer
    {
        public int BufferId { get; set; }
        public Queue<Request> Requests { get; private set; }
        public int Capacity { get; private set; }

        public Buffer(int id, int capacity)
        {
            BufferId = id;
            Requests = new Queue<Request>();
            Capacity = capacity;
        }

        public void AddRequest(Request request)
        {
            if (Requests.Count < Capacity)
            {
                Requests.Enqueue(request);
            }
            else
            {
                throw new InvalidOperationException("Buffer is full.");
            }
        }

        public Request RemoveRequest()
        {
            if (Requests.Count > 0)
                return Requests.Dequeue();
            return null;
        }

        public Request RemoveOldestRequest()
        {
            return RemoveRequest();
        }

        public Request RemoveLastRequest()
        {
            if (Requests.Count == 0)
                return null;

            var list = Requests.ToList();
            var last = list.Last();
            list.RemoveAt(list.Count - 1);
            Requests = new Queue<Request>(list);
            return last;
        }

        public int Count => Requests.Count;
    }



}
