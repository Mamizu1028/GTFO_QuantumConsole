namespace Hikaria.QC;

public enum LogOperationKind
{
    AddHistory,
    BeginHistoryStream,
    AddLiveStatus,
    AddInteractive,
    Update,
    Append,
    Complete,
    Commit,
    Remove,
    Clear
}
