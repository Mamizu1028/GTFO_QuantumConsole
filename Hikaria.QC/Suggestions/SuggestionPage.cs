namespace Hikaria.QC;

internal readonly struct SuggestionPage
{
    public int StartIndex { get; }
    public int EndIndex { get; }
    public int Count => EndIndex - StartIndex;
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }

    private SuggestionPage(int startIndex, int endIndex, bool hasPreviousPage, bool hasNextPage)
    {
        StartIndex = startIndex;
        EndIndex = endIndex;
        HasPreviousPage = hasPreviousPage;
        HasNextPage = hasNextPage;
    }

    public static SuggestionPage Calculate(int suggestionCount, int selectionIndex, int maxDisplaySize)
    {
        if (suggestionCount <= 0)
            return new SuggestionPage(0, 0, false, false);

        if (maxDisplaySize <= 0 || suggestionCount <= maxDisplaySize)
            return new SuggestionPage(0, suggestionCount, false, false);

        int selectedIndex = Clamp(selectionIndex, 0, suggestionCount - 1);
        int startIndex = selectedIndex / maxDisplaySize * maxDisplaySize;
        int endIndex = startIndex + maxDisplaySize;
        if (endIndex > suggestionCount)
            endIndex = suggestionCount;

        return new SuggestionPage(startIndex, endIndex, startIndex > 0, endIndex < suggestionCount);
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
            return min;

        return value > max ? max : value;
    }
}
