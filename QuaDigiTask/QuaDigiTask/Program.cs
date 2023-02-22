using System;
using static QuaDigiTask.MeasurementType;

namespace QuaDigiTask
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Set the randomised DateTime objects to be today's date
            var randomisedData = GetInputData(out var startOfSampling);

            Console.WriteLine("INPUT:");
            foreach (var data in randomisedData)
                data.Print();

            var sortedData = MeasurementSampler.Sample(startOfSampling, randomisedData);
            Console.WriteLine("OUTPUT:");

            foreach (var dataType in sortedData)
            {
                foreach (var data in dataType.Value)
                {
                    data.Print();
                }
            }

        }

        private static List<Measurement> GetInputData(out DateTime startOfSampling)
        {
            /*
            Feel free to edit this method to test the solution on a variety of inputs
            This hardcoded list of inputs is the same as the example input in the 
            problem outline, with some additional heart rate data.
            */
            startOfSampling = new DateTime(2017, 1, 3, 10, 0, 0);

            return new List<Measurement>()
            {
                CreateMeasurement(4, 45, 35.79, Temp),
                CreateMeasurement(1, 18, 98.78, SPO2),
                CreateMeasurement(4, 39, 67, HR),
                CreateMeasurement(9, 7, 35.01, Temp),
                CreateMeasurement(3, 34, 96.49, SPO2),
                CreateMeasurement(2, 1, 35.82, Temp),
                CreateMeasurement(7, 23, 73, HR),
                CreateMeasurement(8, 5, 77, HR),
                CreateMeasurement(5, 0, 97.17, SPO2),
                CreateMeasurement(5, 1, 95.08, SPO2),
                CreateMeasurement(7, 25, 75, HR),
            };

            static Measurement CreateMeasurement(int minute, int second, double value, MeasurementType type)
                => new(new DateTime(2017, 1, 3, 10, minute, second), value, type);
        }
    }
}