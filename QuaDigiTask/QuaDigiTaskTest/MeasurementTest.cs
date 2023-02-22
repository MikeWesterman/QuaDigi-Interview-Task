using NUnit.Framework;
using NUnit.Framework.Constraints;
using QuaDigiTask;
using System.Collections.Generic;
using System.Linq;

namespace QuaDigiTaskTest
{
    [TestFixture]
    public class MeasurementTest
    {
        [Test]
        public void Measurement_InvalidTemperature()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => 
            {
                var _ = new Measurement(DateTime.Now, -1, MeasurementType.Temp);
            });
        }

        [Test]
        public void Measurement_InvalidHeartRate()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var _ = new Measurement(DateTime.Now, 300, MeasurementType.HR);
            });
        }

        [Test]
        public void Measurement_InvalidSPO2()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var _ = new Measurement(DateTime.Now, 101, MeasurementType.SPO2);
            });
        }

        [Test]
        public void Sample_EmptyInput_EmptyOutput()
        {
            var output = MeasurementSampler.Sample(DateTime.Now, new List<Measurement>());
            Assert.That(output.Count, Is.EqualTo(0));
        }

        [Test]
        public void Sample_GroupedInFiveMinuteIntervals()
        {
            var times = new List<DateTime>()
            {
                new DateTime(2023, 02, 21, 15, 0, 0),
                new DateTime(2023, 02, 21, 15, 1, 1),
                new DateTime(2023, 02, 21, 15, 2, 2),
                new DateTime(2023, 02, 21, 15, 3, 3),
                new DateTime(2023, 02, 21, 15, 4, 4),
                new DateTime(2023, 02, 21, 15, 6, 5),
                new DateTime(2023, 02, 21, 15, 11, 6),
            };

            var input = times.Select(t => new Measurement(t, 0, MeasurementType.HR)).ToList();

            var output = MeasurementSampler.Sample(times[0], input);
            var intervals = output[MeasurementType.HR];

            // Should have 3 values. The first from first 5 minutes,
            // second from 5-10 minutes, third from 10-15.
            Assert.That(intervals.Count, Is.EqualTo(3));
            Assert.That(intervals[0].MeasurementTime, Is.EqualTo(times[4]));
            Assert.That(intervals[1].MeasurementTime, Is.EqualTo(times[5]));
            Assert.That(intervals[2].MeasurementTime, Is.EqualTo(times[6]));
        }

        [Test]
        public void Sample_MeasurementsWidelyDistributed()
        {
            var startTime = new DateTime(2023, 02, 21, 15, 0, 0);
            var times = new List<DateTime>()
            {
                new DateTime(2023, 02, 21, 15, 6, 0),
                new DateTime(2023, 02, 21, 15, 12, 1),
                new DateTime(2023, 02, 21, 15, 17, 2),
                new DateTime(2023, 02, 21, 15, 27, 3),
                new DateTime(2023, 02, 21, 15, 37, 4),
                new DateTime(2023, 02, 21, 15, 41, 5),
                new DateTime(2023, 02, 21, 15, 43, 6),
                new DateTime(2023, 02, 21, 15, 45, 6),
                new DateTime(2023, 02, 21, 15, 46, 6),
            };

            var input = times.Select(t => new Measurement(t, 0, MeasurementType.HR)).ToList();

            var output = MeasurementSampler.Sample(startTime, input);
            var intervals = output[MeasurementType.HR];

            Assert.That(intervals.Count, Is.EqualTo(7));

        }

        [Test]
        public void Sample_UsesMeasurementOnIntervalBorder()
        {
            var times = new List<DateTime>()
            {
                new DateTime(2023, 02, 21, 15, 0, 0),
                new DateTime(2023, 02, 21, 15, 9, 59), // 1s before boundary
                new DateTime(2023, 02, 21, 15, 10, 0), // Exactly on boundary
                new DateTime(2023, 02, 21, 15, 10, 1), // 1s after boundary
            };

            var input = times.Select(t => new Measurement(t, 0, MeasurementType.HR)).ToList();

            var output = MeasurementSampler.Sample(times[0], input);
            var intervals = output[MeasurementType.HR];

            Assert.That(intervals.Count, Is.EqualTo(3));
            Assert.That(intervals[0].MeasurementTime, Is.EqualTo(times[0]));
            Assert.That(intervals[1].MeasurementTime, Is.EqualTo(times[2])); // Uses boundary
            Assert.That(intervals[2].MeasurementTime, Is.EqualTo(times[3]));
        }

        [Test]
        public void Sample_GroupsByMeasurementType()
        {
            var currentTime = DateTime.Now;
            var input = new List<Measurement>()
            {
                new Measurement(currentTime, 37, MeasurementType.Temp),
                new Measurement(currentTime.AddMinutes(6), 37.5, MeasurementType.Temp),

                new Measurement(currentTime, 60, MeasurementType.HR),
                new Measurement(currentTime.AddMinutes(6), 61, MeasurementType.HR),

                new Measurement(currentTime, 98, MeasurementType.SPO2),
                new Measurement(currentTime.AddMinutes(6), 99, MeasurementType.SPO2),
            };

            var output = MeasurementSampler.Sample(currentTime, input);
            Assert.That(output.Select(o => o.Key), Is.EquivalentTo(new List<MeasurementType>() { MeasurementType.Temp, MeasurementType.HR, MeasurementType.SPO2 }));
            Assert.That(output[MeasurementType.Temp].Count, Is.EqualTo(2));
            Assert.That(output[MeasurementType.HR].Count, Is.EqualTo(2));
            Assert.That(output[MeasurementType.SPO2].Count, Is.EqualTo(2));
        }

        // TODO: Add test for start time validation. Data times cannot preceed.
        [Test]
        public void Sample_StartTimeAfterMeasurements_ThrowsArgumentException()
        {
            var measurementStartTime = DateTime.Now;

            var input = new List<Measurement>()
            {
                new Measurement(measurementStartTime, 37, MeasurementType.Temp),
                new Measurement(measurementStartTime.AddMinutes(1), 37.5, MeasurementType.Temp),
            };

            Assert.Throws<ArgumentException>(() =>
            {
                MeasurementSampler.Sample(measurementStartTime.AddMinutes(2), input);
            });
        }
    }
}