using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace ReadSensor
{  
    /// <summary>
    /// Models a sensor.
    /// </summary>
    public class Sensor
    {
        public string measurementFlePath { get; set; }
        public string errorLogFilePath { get; set; }
        public DataAccess dataAccess { get; set; } = new DataAccess();

        /// <summary>
        /// Get ADC value from the temperature sensor.
        /// </summary>
        /// <param name="index">The current row we are getting from file</param>
        /// <returns>The ADC value mapped to Celsisus degrees</returns>
        public double getTemperature(short index)
        {
            var adcValue = ReadSensorValueFromFile(index);
            var mappedToCelsius = MapToCelsius(adcValue, SensorConstants.ADC_MIN, SensorConstants.ADC_MAX, SensorConstants.TEMP_MIN, SensorConstants.TEMP_MAX);
            return mappedToCelsius;
        }

        /// <summary>
        /// Read a specific line from the file of sensor readings.
        /// </summary>
        /// <param name="index">The row we are reading from.</param>
        /// <returns>The sensor ADC value</returns>
        public double ReadSensorValueFromFile(int index)
        {
            double sensorValue = 0;
            using (StreamReader reader = File.OpenText(measurementFlePath))
            {
                for (int i = 0; i <= index; i++)
                {
                    if (i == index)
                    {
                        double.TryParse(reader.ReadLine(), out sensorValue);
                        break;
                    }
                    reader.ReadLine();
                }
            }
            return sensorValue;
        }

        /// <summary>
        /// Maps ADC value to Celsius degrees.
        /// </summary>
        /// <param name="x">Value to map</param>
        /// <param name="in_min">Minimum value recived from ADC</param>
        /// <param name="in_max">Maximum value recived from ADC</param>
        /// <param name="out_min">Minimum value of desired output</param>
        /// <param name="out_max">Maximum value of desired output</param>
        /// <returns>The ADC value mapped to Celsius degrees</returns>
        private static double MapToCelsius(double x, double in_min, double in_max, double out_min, double out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        /// <summary>
        /// Logging errors to file
        /// </summary>
        /// <param name="error">The error pasred as a string</param>
        /// <returns></returns>
        internal async Task LogError(string error)
        {
            await System.IO.File.AppendAllTextAsync(errorLogFilePath, error + Environment.NewLine);
        }
    }
}
