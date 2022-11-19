using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ReadSensor
{
    /***
     * 
     *  This class was borrowed from the backend service provided by NxTech.
     * 
     * I use this only to create an object which I then serialize into JSON.
     * 
     */
    public class TemperatureRange
    {
        [Required]
        [Timestamp]
        public DateTime? Start { get; set; }

        [Required]
        [Timestamp]
        public DateTime? End { get; set; }
    }

    public class TemperatureMeasurement
    {
        [Required]
        public TemperatureRange Time { get; set; }

        [Required]
        [Range(-50.0, 50.0)]
        public double? Max { get; set; }

        [Required]
        [Range(-50.0, 50.0)]
        public double? Min { get; set; }

        [Required]
        [Range(-50.0, 50.0)]
        public double? Avg { get; set; }

        public override string ToString()
        {
            return $"[{Time.Start.Value.ToString("o")}-{Time.End.Value.ToString("o")}] max: {Max} min: {Min} avg: {Avg}";
        }
        
}

}
