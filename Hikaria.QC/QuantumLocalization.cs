namespace Hikaria.QC
{
    public static class QuantumLocalization
    {
        public static string Loading => QuantumGlobal.Localization.Get(1);
        public static string ExecutingAsyncCommand => QuantumGlobal.Localization.Get(2);
        public static string EnterCommand => QuantumGlobal.Localization.Get(3);

        public static string CommandError => QuantumGlobal.Localization.Get(4);
        public static string ConsoleError => QuantumGlobal.Localization.Get(5);
        public static string MaxLogSizeExceeded => QuantumGlobal.Localization.Get(6);

        public static string InitializationProgress => QuantumGlobal.Localization.Get(7);

        public static string InitializationComplete => QuantumGlobal.Localization.Get(8);

        public static string SubmitButtonText => QuantumGlobal.Localization.Get(9);
        public static string ClearButtonText => QuantumGlobal.Localization.Get(10);
        public static string CloseButtonText => QuantumGlobal.Localization.Get(11);
    }
}
