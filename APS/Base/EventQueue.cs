namespace APS.Base
{
    public class EventQueue
    {
        private List<Event> events;

        public EventQueue()
        {
            events = new List<Event>();
        }

        public void AddEvent(double time, string type, Action callback)
        {
            events.Add(new Event { Time = time, Type = type, Callback = callback });
            events.Sort();
        }

        public Event PopEvent()
        {
            if (events.Count > 0)
            {
                Event nextEvent = events[0];
                events.RemoveAt(0);
                return nextEvent;
            }
            return null;
        }

        public double NextEventTime()
        {
            if (events.Count > 0)
                return events[0].Time;
            return double.PositiveInfinity;
        }

        public int Count => events.Count;
    }


}
