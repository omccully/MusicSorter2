using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSorterTest
{
    class ObjectNotFoundException : Exception
    {
        public ObjectNotFoundException(string msg=null) : base(msg)
        {
        }
    }

    class EventNotExpectedException : Exception
    {
        public EventNotExpectedException(string msg= null) : base(msg)
        { 
        }
    }

    class EventOccurredTooManyTimesException : Exception
    {
        public EventOccurredTooManyTimesException(string msg= null) : base(msg)
        {
        }
    }

    /// <summary>
    /// Tests to make sure events happen an expected amount of times
    /// </summary>
    /// <typeparam name="T">Event args type</typeparam>
    class EventTracker<T>
    {
        T[] ExpectedObjects;
        bool[] ObjectSeen { get; set; }

        public delegate bool EventComparator(T a, T b);
        EventComparator comparator { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ExpectedObjectsIE">Expected objects</param>
        /// <param name="comparator">Delegate to compare objects of type T for equality</param>
        public EventTracker(IEnumerable<T> ExpectedObjectsIE, EventComparator comparator = null)
        {
            this.ExpectedObjects = ExpectedObjectsIE.ToArray();
            this.ObjectSeen = new bool[this.ExpectedObjects.Length];
            this.comparator = comparator;
        }

        /// <summary>
        /// Register that a certain event has happened.
        /// throws EventNotExpectedException if event should not have occurred
        /// throws EventOccurredTooManyTimesException if event occurred too many times
        /// </summary>
        /// <param name="obj">Event object</param>
        public void TrackEvent(T obj)
        {
            int index = -1;

            try
            {
                while (ObjectSeen[index = IndexOf(obj, index + 1)]) ;

                // (!ObjectSeen[index]) and (ExpectedObjects[index] equals obj) now
                this.ObjectSeen[index] = true;
            }
            catch (ObjectNotFoundException)
            {
                if (index == -1)
                    throw new EventNotExpectedException(obj.ToString());
                throw new EventOccurredTooManyTimesException(obj.ToString());
            }

        }

        /// <summary>
        /// Determines if all expected objects were tracked
        /// </summary>
        /// <returns></returns>
        public bool WasSuccessful()
        {
            return this.ObjectSeen.Count(b => !b) == 0;
        }

        public T[] ObjectsSeen()
        {
            return ObjectsThatWere(true);
        }

        public T[] ObjectsNotSeen()
        {
            return ObjectsThatWere(false);
        }

        /// <summary>
        /// Returns ToString() values for all objects not seen by this tracker
        /// </summary>
        /// <returns></returns>
        public string Debug()
        {
            T[] not_seen = ObjectsNotSeen();
            return "Objects not seen:\n" + String.Join("\n", not_seen);
        }

        T[] ObjectsThatWere(bool Seen)
        {
            List<T> list = new List<T>();

            for (int i = 0; i < ExpectedObjects.Length; i++)
            {
                if (ObjectSeen[i] == Seen)
                {
                    list.Add(ExpectedObjects[i]);
                }
            }

            return list.ToArray();
        }

        int IndexOf(T obj, int StartPosition = 0)
        {
            for (int i = StartPosition; i < ExpectedObjects.Count(); i++)
            {
                if (Compare(obj, ExpectedObjects[i]))
                {
                    return i;
                }
            }

            throw new ObjectNotFoundException();
        }

        bool Compare(T a, T b)
        {
            if (comparator == null)
            {
                // use default comparer
                return EqualityComparer<T>.Default.Equals(a, b);
            }
            // use user's comparator
            return comparator(a, b);

        }
    }
}
