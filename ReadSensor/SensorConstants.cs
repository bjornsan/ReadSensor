using System;
using System.Collections.Generic;
using System.Text;

namespace ReadSensor
{
    public static class SensorConstants
    {
        /***
         * This class contains constant values regarding the properties of the sensor.
         * Theese values do not change and as such I thought it unesseccary to include them
         * into the sensor class itself.
         * 
         *  A 12 bit ADC has a value range between 0 and 4095.
         *  
         *  Sanpling rate is specified to every 100 ms,
         *  with a full measurement ocurring every 2 minutes.
         *  
         *  With the given sampling time this results in a measurement cycle of:
         *      
         *          100 ms * 10 * 60 * 2 = 120 000 ms
         * 
         * From investigating temperature.txt I found out that it contains 767 rows of data.
         * 
         * The sample size is the number of samples in each "measurement cycle of 2 minutes".
         * 
         *          10 (samples / second ) * 60 (seconds / minute ) * 2 ( minutes )  = 1200 samples over 2 minutes. 
         */
        public static readonly short ADC_MIN = 0;
        public static readonly short ADC_MAX = 4095;
        public static readonly short TEMP_MIN = -50;
        public static readonly short TEMP_MAX = 50;
        public static readonly byte SAMPLING_RATE_MS = 100;
        public static readonly int MEASUREMENT_CYCLE_MS = 120000;
        public static readonly short SAMPLE_SIZE = 1200;
        public static readonly short ROWS_IN_CSV = 767;
    }
}
