using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ReadSensor
{
    /// <summary>
    /// Provides data access to the "remote" api for storing sensor data.
    /// </summary>
    public class DataAccess
    {
        public static readonly string API_ENDPOINT_STANDARD = "http://localhost:5000/api/temperature";
        public static readonly string API_ENDPOINT_MISSING = "http://localhost:5000/api/temperature/missing";
        private readonly string DATA_ERROR_PATH = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\sensordata\\dataErrorLog.txt";

        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Sends a temperature measurement to teh API Endpoint "Standard".
        /// </summary>
        /// <param name="measurement"></param>
        /// <returns>If successfull</returns>
        internal async Task<bool> PostMeasurement(TemperatureMeasurement measurement)
        {
            var jsonData = JsonConvert.SerializeObject(measurement);
            HttpResponseMessage result;
            try
            {
                result = await _httpClient.PostAsync(API_ENDPOINT_STANDARD, new StringContent(jsonData, Encoding.UTF8, "application/json"));
                result.EnsureSuccessStatusCode();
                return result.IsSuccessStatusCode;
            }
            catch (HttpRequestException error)
            {
                // log error
                _ = LogError(error.Message);
                return false;
            }
        }

        /// <summary>
        /// Sends a temperature measurement to teh API Endpoint "Missing".
        /// </summary>
        /// <param name="measurement"></param>
        /// <returns>If successfull</returns>
        internal async Task<bool> PostMissingMeasurement(List<TemperatureMeasurement> measurements)
        {
            var jsonData = JsonConvert.SerializeObject(measurements);
            HttpResponseMessage result;
            try
            {
                result = await _httpClient.PostAsync(API_ENDPOINT_MISSING, new StringContent(jsonData, Encoding.UTF8, "application/json"));
                result.EnsureSuccessStatusCode();
                return result.IsSuccessStatusCode;
            }
            catch (HttpRequestException error)
            {
                // log error
                _ = LogError(error.Message);
                return false;
            }
        }
            
        /// <summary>
        /// Log errors to file.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        internal async Task LogError(string error)
        {
            await System.IO.File.AppendAllTextAsync(DATA_ERROR_PATH, error + Environment.NewLine);
        }
    }
}
