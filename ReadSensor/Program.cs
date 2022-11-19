using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace ReadSensor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Path to temperature.txt
            string PATH_TO_MEASUREMENT_FILE = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\sensordata\\temperature.txt";
            // Path to errorlog.txt
            string PATH_TO_ERROR_LOG = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\sensordata\\errorlog.txt";
            // Initialize the sensor
            Sensor sensor = new Sensor
            {
                measurementFlePath = PATH_TO_MEASUREMENT_FILE,
                errorLogFilePath = PATH_TO_ERROR_LOG
            };


            // Time stamps used for the control loop
            long timeSinceLastSampled = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long timeSinceLastSendData = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Time stamp for the start of a measurement period
            var measurementStartTime = DateTime.UtcNow;

            // Indexes for keeping track of file end, and sample array
            short fileIndex = 0;
            short sampleIndex = 0;

            // Array for storing samples during a measurement time. 
            double[] sampledMeasurements = new double[SensorConstants.SAMPLE_SIZE];

            // Queue for storing measurements failed to send.
            Queue<TemperatureMeasurement> missingMeasurements = new Queue<TemperatureMeasurement>();

            // control for ending the main loop
            bool isRunning = true;

            // Main loop where reading and sending data is done.
            while (isRunning)
            {
                // Current time at start of loop
                var timeNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // Reading data
                // Check if 100 ms has passed since last sample
                if ((timeNow - timeSinceLastSampled) >= SensorConstants.SAMPLING_RATE_MS)
                {
                    // If filehandle is at the end of file, reset handle to 0
                    if (fileIndex >= SensorConstants.ROWS_IN_CSV)
                    {
                        fileIndex = 0;
                    }

                    // set timeSinceLastSampled to current time.
                    timeSinceLastSampled = timeNow;

                    // Getting data from sensor.
                    try
                    {
                        sampledMeasurements[sampleIndex] = sensor.getTemperature(fileIndex);
                    }
                    catch (IndexOutOfRangeException error)
                    {
                        // Log error to file and continue.
                        Console.WriteLine($"[{timeNow}][ERROR READING FROM SENSOR][LOGGING ERROR TO FILE]");
                        _ = sensor.LogError($"[{timeNow}][{error.ToString()}]");
                    }

                    // Increment both filehandle index and sampleindex.
                    fileIndex++;
                    sampleIndex++;
                }

                // Storing data
                // Check if two minutes have passed since last time API request was done.
                if ((timeNow - timeSinceLastSendData) >= SensorConstants.MEASUREMENT_CYCLE_MS)
                {
                    // If queue of falied sendings of measurements > 0
                    if (missingMeasurements.Count > 0)
                    {
                        Console.WriteLine($"[{timeNow}][TRYING TO STORE MISSING DATA IN NEW THREAD]");
                        new Thread(() =>
                        {
                            //var missingPostArray = new TemperatureMeasurement[missingMeasurements.Count];
                            var missingPostList = new List<TemperatureMeasurement>();
                            for (int i = 0; i < missingMeasurements.Count; i++)
                            {
                                //missingPostArray[i] = missingMeasurements.Dequeue();
                                missingPostList.Add(missingMeasurements.Dequeue());
                            }
                            var isMissingSuccess = sensor.dataAccess.PostMissingMeasurement(missingPostList);

                            if (isMissingSuccess.Result)
                            {
                                Console.WriteLine($"[{timeNow}][SUCCESSFULLY STORED MISSING MEASUREMENTS]");
                            }
                            else
                            {
                                Console.WriteLine($"[{timeNow}][COULDNT STORE MISSING MEASUREMENTS]");
                                Console.WriteLine($"[{timeNow}][PLACING THEM BACK IN QUEUE]");
                                foreach (var m in missingPostList)
                                {
                                    missingMeasurements.Enqueue(m);
                                }
                            }
                        }).Start();

                    }

                    // set timeSinceLastSendData to current time.
                    timeSinceLastSendData = timeNow;

                    // calculate min, max, and avgerage.
                    double min = 50, max = 0, sum = 0, avg = 0;

                    for (int i = 0; i < sampledMeasurements.Length; i++)
                    {
                        if (sampledMeasurements[i] < min)
                            min = sampledMeasurements[i];
                    }
                    min = Math.Round(min, 2);

                    for (int j = 0; j < sampledMeasurements.Length; j++)
                    {
                        if (sampledMeasurements[j] > max)
                            max = sampledMeasurements[j];
                    }
                    max = Math.Round(max, 2);

                    for (int k = 0; k < sampledMeasurements.Length; k++)
                    {
                        sum += sampledMeasurements[k];
                    }

                    if (sampledMeasurements.Length != 0)
                        avg = Math.Round( (sum / sampledMeasurements.Length), 2 );

                    var measurementEndTime = DateTime.UtcNow;

                    // Used for printing to console in specified format
                    // The controller expects a DateTime object and not a string, as such I have provided the proper formatted DateTime string below for
                    // debugging. But to the API is sent the actual DateTime object.
                    var endTimeToString = measurementEndTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ", CultureInfo.InvariantCulture);
                    var startTimeToString = measurementStartTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ", CultureInfo.InvariantCulture);

                    // Building a measurement object
                    var newMeasurement = new TemperatureMeasurement
                    {
                        Time = new TemperatureRange
                        {
                            Start = measurementStartTime,
                            End = measurementEndTime
                        },
                        Max = max,
                        Min = min,
                        Avg = avg
                    };

                  
                    // Logging to console the values from the current measurement.
                    Console.WriteLine($"[CURRENT MEASUREMENT][{startTimeToString}-{endTimeToString}] max: {Math.Round(max, 2)} min: {Math.Round(min)} avg: {Math.Round(avg, 2)}");
                    Console.WriteLine($"[{timeNow}][TRYING TO STORE CURRENT MEASUREMENT IN NEW THREAD]");
                    // Send data async to backend
                    new Thread(() =>
                    {
                        var isSuccess = sensor.dataAccess.PostMeasurement(newMeasurement);

                        if (isSuccess.Result)
                        {
                            Console.WriteLine($"[{timeNow}][CURRENT MEASUREMENT SUCCESFULLY STORED]");
                        }
                        else
                        {
                            Console.WriteLine($"[{timeNow}][FAILED TO STORE CURRENT MEASUREMENT]");
                            Console.WriteLine($"[{timeNow}][PLACING IN QUEUE]");

                            if (missingMeasurements.Count < 10)
                            {
                                missingMeasurements.Enqueue(newMeasurement);
                            }
                            else
                            {
                                Console.WriteLine($"[{timeNow}][QUEUE FULL, REMOVING OLDEST, ADDING NEW]");
                                missingMeasurements.Dequeue();
                                missingMeasurements.Enqueue(newMeasurement);
                            }
                        }
                    }).Start();

                    // If a full measurement cycle has completed, reset the sampleIndex
                    sampleIndex = 0;
                    // Empty sampled measurement array
                    Array.Clear(sampledMeasurements, 0, SensorConstants.SAMPLE_SIZE);
                    // Set the new start time of next mesaurment to be the current time.
                    measurementStartTime = DateTime.UtcNow;

                    Console.WriteLine($"[{timeNow}][STARTING NEW MEASUREMENT CYCLE]");
                }
            }
        }
    }
}