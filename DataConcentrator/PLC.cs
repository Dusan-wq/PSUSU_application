using PLCSimulator;

namespace DataConcentrator
{
    // Wrapper oko PLCSimulatorManager - singleton pristupna tacka za DataConcentrator.
    public static class PLC
    {
        private static PLCSimulatorManager instance;

        public static PLCSimulatorManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PLCSimulatorManager();
                    instance.StartPLCSimulator();
                }
                return instance;
            }
        }

        public static void StopSimulator()
        {
            instance?.Abort();
        }
    }
}
