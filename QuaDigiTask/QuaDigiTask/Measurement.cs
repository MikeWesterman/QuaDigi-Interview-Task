using System.Collections.Generic;
using System.Globalization;

namespace QuaDigiTask
{
    /// <summary>
    /// Types of Measurement which may be taken.
    /// </summary>
    enum MeasurementType
    {
        Temp,
        HR,
        SPO2,
    }

    /// <summary>
    /// Class to store information about a single Measurement.
    /// </summary>
    class Measurement
    {
        internal DateTime MeasurementTime { get; init; }
        internal double MeasurementValue { get; init; }
        internal MeasurementType Type { get; init; }

        public Measurement(DateTime time, double value, MeasurementType type)
        {
            // Prevent nonsensical values.
            // I'm no biologist so these values could be better refined,
            // but at least some validation is helpful/worthwhile e.g. prevent negative values.
            switch (type)
            {
                case MeasurementType.Temp:
                    if (value < 30 || value > 45) throw new ArgumentOutOfRangeException("Body temperature cannot exceed the bounds of 30-45.");
                    break;
                case MeasurementType.HR:
                    if (value < 0 || value > 250) throw new ArgumentOutOfRangeException("Heart rate cannot exceed the bounds of 0-250.");
                    break;
                case MeasurementType.SPO2:
                    if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("SPO2 cannot exceed the bounds of 0-100.");
                    break;

            }

            MeasurementTime = time;
            MeasurementValue = value;
            Type = type;
        }

        public override string ToString() => $"{{{MeasurementTime}, {Type}, {MeasurementValue}}}\n";

        /// <summary>
        /// Helper method to write out the properties of a Measurement in  well formatted manner.
        /// </summary>
        public void Print() => Console.Write(this.ToString());
    }

    /// <summary>
    /// Class to handle sampling of multiple Measurements. Note that this class may be better suited to its own
    /// .cs file as this would be a better layout when considering extendability. However this class has been
    /// kept in the same .cs file for the sake of keeping things concise and readable for this interview task.
    /// </summary>
    static class MeasurementSampler
    {
        /// <summary>
        /// Sample measurements according to the following requirements:
        /// - Each type of measurement is sampled separately
        /// - From each 5 minute interval, only the last measurement is taken
        /// - If a measurement timestamp exactly matches a 5-minute interval border, it is used
        /// for the current interval
        /// - The input values are not sorted by time
        /// - The output is sorted by time ascending
        /// </summary>
        /// <param name="startOfSampling">The DateTime at which sampling began</param>
        /// <param name="unsampledMeasurements">The set of Measurements to be sampled</param>
        /// <returns>
        /// A dictionary of sampled Measurements according to the rules in the method summary,
        /// where keys are the type of measurement and values are the list of sampled Measurements.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If any Measurement was taken at a time preceeding the given DateTime at which sampling began.
        /// </exception>
        internal static Dictionary<MeasurementType, List<Measurement>> Sample(DateTime startOfSampling, List<Measurement> unsampledMeasurements)
        {
            if (unsampledMeasurements.Any(m => m.MeasurementTime < startOfSampling))
                throw new ArgumentException("Input measurements contained one or more measurement that preceeded the given start time.");

            var sampledMeasurements = new Dictionary<MeasurementType, List<Measurement>>();
            // Split Measurement samples by their type
            var groups = GroupMeasurementsByType(unsampledMeasurements);
            // For each type, sample in intervals of 5 minutes
            foreach (var group in groups)
                sampledMeasurements.Add(group.Key, SampleMeasurements(startOfSampling, group.Value));

            return sampledMeasurements;
        }

        /// <summary>
        /// Separate and group input Measurements by their MeasurementType
        /// </summary>
        /// <param name="measurements">List of unordered and ungrouped Measurements</param>
        /// <returns>
        /// Grouped Measurements by MeasurementType, with keys as MeasurementType, values as lists of Measurements of the corresponding type.
        /// </returns>
        private static Dictionary<MeasurementType, List<Measurement>> GroupMeasurementsByType(List<Measurement> measurements)
        {
            var groups = new Dictionary<MeasurementType, List<Measurement>>();

            foreach (var measurement in measurements)
            {
                if (!groups.ContainsKey(measurement.Type))
                    groups.Add(measurement.Type, new List<Measurement>());

                groups[measurement.Type].Add(measurement);
            }

            return groups;
        }

        /// <summary>
        /// Sample a list of Measurements by their time, selecting the last Measurement from each interval of 5 minutes.
        /// 
        /// Note that the requirement of "if a measurement timestamp will exactly match a 5-minute interval border, it shall be used
        /// for the current interval" was interpreted to mean that, if a measurement at time 10:05 sits on the border between
        /// intervals 10:00-10:05 and 10:05-10:10, the 10:05 measurement will be used for the 10:00-10:05 interval.
        /// Although a little ambiguous, this was chosen as the 10:05 measurement is the last or most recent Measurement from
        /// that interval, whih aligns wih the first rule.
        /// </summary>
        /// <param name="startOfSampling">The DateTime at which sampling began.</param>
        /// <param name="unorderedMeasurements">The list of unordered measurements to be sampled.</param>
        /// <returns>A list of ordered measurements fitting the criteria described in the summary.</returns>
        private static List<Measurement> SampleMeasurements(DateTime startOfSampling, List<Measurement> unorderedMeasurements)
        {
            // Keys are the nth interval (multiple of 5 minutes since the start of sampling)
            // Values are the last recorded Measurement in that interval
            var intervals = new Dictionary<int, Measurement>();

            foreach (var measurement in unorderedMeasurements)
            {
                var timeSinceStart = measurement.MeasurementTime.Subtract(startOfSampling);
                // Use integer division to find how many intervals of 5 minutes from the start of sampling to the measurement
                var numIntervalsSinceStart = timeSinceStart.Minutes / 5;
                // If the measurement sits on the boundary between two intervals, use the measurement for the lower of the two intervals.
                if (timeSinceStart != TimeSpan.Zero && timeSinceStart.Seconds == 0 && timeSinceStart.Minutes % 5 == 0)
                    numIntervalsSinceStart--;

                // Set the value at the nth interval to be the measurement if it is more recent than the current value
                if (!intervals.ContainsKey(numIntervalsSinceStart))
                    intervals[numIntervalsSinceStart] = measurement;
                else if (intervals[numIntervalsSinceStart].MeasurementTime < measurement.MeasurementTime)
                    intervals[numIntervalsSinceStart] = measurement;
            }

            // Values are inherently sorted by time ascending by nature of how the dictionary was setup, no need to sort
            return intervals.Values.ToList();
        }
    }
}
