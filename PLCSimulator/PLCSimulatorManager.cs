using System;
using System.Collections.Generic;
using System.Threading;

namespace PLCSimulator
{
    /// <summary>
    /// PLC Simulator
    /// 
    /// 4 x ANALOG INPUT : ADDR001 - ADDR004
    /// 4 x ANALOG OUTPUT: ADDR005 - ADDR008
    /// 4 x DIGITAL INPUT: ADDR009, ADDR011 - ADDR013
    /// 4 x DIGITAL OUTPUT: ADDR010, ADDR014 - ADDR016
    /// </summary>
    public class PLCSimulatorManager
    {
        private readonly Dictionary<string, double> addressValues;
        private readonly object locker = new object();
        private Thread t1;
        private Thread t2;
        private volatile bool running;

        public PLCSimulatorManager()
        {
            addressValues = new Dictionary<string, double>();

            // AI
            addressValues.Add("ADDR001", 0);
            addressValues.Add("ADDR002", 0);
            addressValues.Add("ADDR003", 0);
            addressValues.Add("ADDR004", 0);

            // AO
            addressValues.Add("ADDR005", 0);
            addressValues.Add("ADDR006", 0);
            addressValues.Add("ADDR007", 0);
            addressValues.Add("ADDR008", 0);

            // DI
            addressValues.Add("ADDR009", 0);
            addressValues.Add("ADDR011", 0);
            addressValues.Add("ADDR012", 0);
            addressValues.Add("ADDR013", 0);

            // DO
            addressValues.Add("ADDR010", 0);
            addressValues.Add("ADDR014", 0);
            addressValues.Add("ADDR015", 0);
            addressValues.Add("ADDR016", 0);
        }

        public void StartPLCSimulator()
        {
            running = true;

            t1 = new Thread(GeneratingAnalogInputs) { IsBackground = true };
            t1.Start();

            t2 = new Thread(GeneratingDigitalInputs) { IsBackground = true };
            t2.Start();
        }

        private void GeneratingAnalogInputs()
        {
            while (running)
            {
                Thread.Sleep(100);

                lock (locker)
                {
                    addressValues["ADDR001"] = 100 * Math.Sin((double)DateTime.Now.Second / 60 * Math.PI); //SINE
                    addressValues["ADDR002"] = 100 * DateTime.Now.Second / 60; //RAMP
                    addressValues["ADDR003"] = 50 * Math.Cos((double)DateTime.Now.Second / 60 * Math.PI); //COS
                    addressValues["ADDR004"] = RandomNumberBetween(0, 50);  //rand
                }
            }
        }

        private void GeneratingDigitalInputs()
        {
            while (running)
            {
                Thread.Sleep(1000);

                lock (locker)
                {
                    addressValues["ADDR009"] = addressValues["ADDR009"] == 0 ? 1 : 0;
                    addressValues["ADDR011"] = addressValues["ADDR011"] == 0 ? 1 : 0;
                    addressValues["ADDR012"] = addressValues["ADDR012"] == 0 ? 1 : 0;
                    addressValues["ADDR013"] = addressValues["ADDR013"] == 0 ? 1 : 0;
                }
            }
        }

        // Citanje/pisanje je zakljucano (lock) da bi se izbegli race-conditioni
        // izmedju niti za generisanje ulaza i niti koje citaju/pisu iz GUI-ja.
        public double GetAnalogValue(string address)
        {
            lock (locker)
            {
                if (addressValues.ContainsKey(address))
                {
                    return addressValues[address];
                }
                return -1;
            }
        }

        public void SetAnalogValue(string address, double value)
        {
            lock (locker)
            {
                if (addressValues.ContainsKey(address))
                {
                    addressValues[address] = value;
                }
            }
        }

        public void SetDigitalValue(string address, double value)
        {
            lock (locker)
            {
                if (addressValues.ContainsKey(address))
                {
                    addressValues[address] = value;
                }
            }
        }

        private static double RandomNumberBetween(double minValue, double maxValue)
        {
            Random random = new Random();
            var next = random.NextDouble();
            return minValue + (next * (maxValue - minValue));
        }

        // Kooperativno gasenje niti (bez Thread.Abort, koji je nepouzdan/deprecated).
        public void Abort()
        {
            running = false;
        }
    }
}
