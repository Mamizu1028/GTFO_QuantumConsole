namespace Hikaria.QC
{
    public static class QuantumLocalization
    {
        public static string Loading => QuantumGlobal.Localization.GetById(1, "Loading...");
        public static string ExecutingAsyncCommand => QuantumGlobal.Localization.GetById(2, "Executing async command...");
        public static string EnterCommand => QuantumGlobal.Localization.GetById(3, "Enter Command...");

        public static string CommandError => QuantumGlobal.Localization.GetById(4, "Error");
        public static string ConsoleError => QuantumGlobal.Localization.GetById(5, "Quantum Processor Error");
        public static string MaxLogSizeExceeded => QuantumGlobal.Localization.GetById(6, "Log of size {0} exceeded the maximum log size of {1}");

        public static string InitializationProgress => QuantumGlobal.Localization.GetById(7, 
            "Q:\\>Quantum Console Processor is initializing\nQ:\\>Table generation under progress\nQ:\\>{0} commands have been loaded");

        public static string InitializationComplete => QuantumGlobal.Localization.GetById(8, "Q:\\>Quantum Console Processor ready");

        public static string SubmitButtonText => QuantumGlobal.Localization.GetById(9, "Submit");
        public static string ClearButtonText => QuantumGlobal.Localization.GetById(10, "Clear");
        public static string CloseButtonText => QuantumGlobal.Localization.GetById(11, "Close");
    }
}
