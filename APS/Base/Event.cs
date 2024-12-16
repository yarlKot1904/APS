namespace APS.Base
{
    using System;

    public class Event : IComparable<Event>
    {
        public double Time { get; set; }
        public string Type { get; set; }
        public Action Callback { get; set; }

        public int CompareTo(Event other)
        {
            return Time.CompareTo(other.Time);
        }
    }


}
