using System.Text;

namespace Hikaria.QC;

internal sealed class Log
{
    public LogHandle Id { get; }
    public LogKind Kind { get; private set; }
    public LogLevel Level { get; private set; }
    private string _text;
    private StringBuilder? _builder;
    private bool _textDirty;

    public string Text
    {
        get
        {
            if (_builder != null && _textDirty)
            {
                _text = _builder.ToString();
                _textDirty = false;
            }

            return _text;
        }
    }

    public bool IsDirty { get; set; } = true;
    internal bool IsMutable => Kind != LogKind.History;
    public float CellSize { get; set; }

    internal Log(LogHandle id, LogKind kind, string text, LogLevel level)
    {
        Id = id;
        Kind = kind;
        _text = text ?? string.Empty;
        Level = level;
    }

    internal void SetText(string text, LogLevel level)
    {
        _text = text ?? string.Empty;
        _builder = null;
        _textDirty = false;
        Level = level;
        IsDirty = true;
    }

    internal void AppendText(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _builder ??= new StringBuilder(_text);
            _builder.Append(text);
            _textDirty = true;
            IsDirty = true;
        }
    }

    internal void CompactText()
    {
        if (_builder == null)
            return;

        _text = Text;
        _builder = null;
        _textDirty = false;
    }

    internal void SetKind(LogKind kind)
    {
        Kind = kind;
        IsDirty = true;
    }
}
