using UnityEngine;

namespace Hikaria.QC
{
    public class QuantumLocalization : ScriptableObject
    {
        public string Loading = "Loading...";
        public string ExecutingAsyncCommand = "Executing async command...";
        public string EnterCommand = "Enter Command...";

        public string CommandError = "Error";
        public string ConsoleError = "Quantum Processor Error";
        public string MaxLogSizeExceeded = "Log of size {0} exceeded the maximum log size of {1}";

        public string InitializationProgress =
            "Q:\\>Quantum Console Processor is initializing\n" +
            "Q:\\>Table generation under progress\n" +
            "Q:\\>{0} commands have been loaded";

        public string InitializationComplete = "Q:\\>Quantum Console Processor ready";
    }
}
