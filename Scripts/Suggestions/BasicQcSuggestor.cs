namespace Hikaria.QC
{
    public abstract class BasicQcSuggestor<TItem> : IQcSuggestor
    {
        protected abstract bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options);

        protected abstract IQcSuggestion ItemToSuggestion(TItem item);

        protected abstract IEnumerable<TItem> GetItems(SuggestionContext context, SuggestorOptions options);

        protected virtual bool IsMatch(SuggestionContext context, IQcSuggestion suggestion, SuggestorOptions options)
        {
            return SuggestorUtilities.IsCompatible(context.Prompt, suggestion.PrimarySignature, options);
        }

        public IEnumerable<IQcSuggestion> GetSuggestions(SuggestionContext context, SuggestorOptions options)
        {
            if (!CanProvideSuggestions(context, options))
            {
                return Enumerable.Empty<IQcSuggestion>();
            }

            return GetItems(context, options)
                .Select(ItemToSuggestionCached)
                .Where(suggestion => IsMatch(context, suggestion, options));
        }

        private IQcSuggestion ItemToSuggestionCached(TItem item)
        {
            return ItemToSuggestion(item);
        }
    }
}
