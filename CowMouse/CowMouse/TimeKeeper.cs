using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CowMouse
{
    public class TimeKeeper
    {
        #region Time Keeping
        private int framesSinceMidnight;
        private int FramesSinceMidnight
        {
            get { return framesSinceMidnight; }
            set
            {
                framesSinceMidnight = value;
                Hours = (framesSinceMidnight * 24) / FramesPerDay;
                Minutes = ((framesSinceMidnight * 24 * 60) / FramesPerDay) % 60;

                setUpCutoffs();
            }
        }
        public int Hours
        {
            get;
            private set;
        }

        public int Minutes
        {
            get;
            private set;
        }
        #endregion

        #region Frame Counting
        private void SetFramesPerDay(int frames)
        {
            this.FramesPerDay = frames;
        }
        private int FramesPerDay { get; set; }
        #endregion

        /// <summary>
        /// Constructs a new TimeKeeper.
        /// </summary>
        /// <param name="realMinutesPerGameDay">The amount of (real-world) minutes for a whole in-game day to pass.</param>
        public TimeKeeper(int realMinutesPerGameDay)
        {
            //1 real minute is 60 seconds, 1 real second is 60 frames
            //so 1 real minute per game day is 60*60 real frames per game day
            SetFramesPerDay(realMinutesPerGameDay * 60 * 60);
            this.FramesSinceMidnight = 0;
        }

        public void Update(GameTime gameTime)
        {
            FramesSinceMidnight++;
            if (FramesSinceMidnight >= FramesPerDay)
                FramesSinceMidnight = 0;
        }

        #region Times of Day
        Dictionary<TimeOfDay, int> startTimes;
        Dictionary<TimeOfDay, int> ranges;
        
        /// <summary>
        /// Sets the start times and ranges for each time of day
        /// </summary>
        private void setUpCutoffs()
        {
            startTimes = new Dictionary<TimeOfDay, int>();
            ranges = new Dictionary<TimeOfDay, int>();

            //night_2 starts at midnight (second watch)
            startTimes[TimeOfDay.NIGHT_2] = 0;

            //sunrise starts at 5 am, and ends the night(2)
            startTimes[TimeOfDay.SUNRISE] = (5 * FramesPerDay) / 24;
            ranges[TimeOfDay.NIGHT_2] = startTimes[TimeOfDay.SUNRISE] - startTimes[TimeOfDay.NIGHT_2];

            //morning starts at 6 am, and ends the sunrise
            startTimes[TimeOfDay.MORNING] = (6 * FramesPerDay) / 24;
            ranges[TimeOfDay.SUNRISE] = startTimes[TimeOfDay.MORNING] - startTimes[TimeOfDay.SUNRISE];

            //afternoon starts at noon, and ends the morning
            startTimes[TimeOfDay.AFTERNOON] = (12 * FramesPerDay) / 24;
            ranges[TimeOfDay.MORNING] = startTimes[TimeOfDay.AFTERNOON] - startTimes[TimeOfDay.MORNING];

            //sunset starts at 7 pm, and ends the afternoon
            startTimes[TimeOfDay.SUNDOWN] = (19 * FramesPerDay) / 24;
            ranges[TimeOfDay.AFTERNOON] = startTimes[TimeOfDay.SUNDOWN] - startTimes[TimeOfDay.AFTERNOON];

            //night_1 (first watch) starts at 8 pm, and ends the sunset
            startTimes[TimeOfDay.NIGHT_1] = (20 * FramesPerDay) / 24;
            ranges[TimeOfDay.SUNDOWN] = startTimes[TimeOfDay.NIGHT_1] - startTimes[TimeOfDay.SUNDOWN];

            //one more cleanup to do
            ranges[TimeOfDay.NIGHT_1] = FramesPerDay - startTimes[TimeOfDay.NIGHT_1];
        }

        public TimeOfDay DayTime
        {
            get
            {
                foreach (TimeOfDay time in startTimes.Keys)
                {
                    if (FramesSinceMidnight >= startTimes[time] && FramesSinceMidnight - ranges[time] <= startTimes[time])
                        return time;
                }

                throw new ArithmeticException("Some kind of integer roundoff issue ...");
            }
        }

        public float PercentageThroughCurrentPhase()
        {
            return (framesSinceMidnight - (float)startTimes[DayTime]) / ranges[DayTime];
        }
        #endregion

        /// <summary>
        /// A sample string for measurement/sizing purposes.
        /// </summary>
        public static String TestString
        {
            get { return "Current Time: 12:12"; }
        }

        public override String ToString()
        {
            return "Current Time: " + Hours.ToString("00") + ":" + Minutes.ToString("00");
        }
    }

    public enum TimeOfDay
    {
        NIGHT_1,
        SUNRISE,
        MORNING,
        AFTERNOON,
        SUNDOWN,
        NIGHT_2
    }
}
