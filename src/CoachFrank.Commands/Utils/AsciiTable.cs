using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CoachFrank.Commands.Utils;

//https://github.com/RPCS3/discord-bot/blob/master/CompatBot/Utils/AsciiTable.cs
public sealed class AsciiTable
{
    private readonly string[] _columns;
    private readonly bool[] _alignToRight;
    private readonly bool[] _disabled;
    private readonly int[] _maxWidth;
    private readonly int[] _width;
    private readonly List<string[]> _rows = new();

    public AsciiTable(params string[] columns)
    {
        if (columns == null)
            throw new ArgumentNullException(nameof(columns));

        if (columns.Length == 0)
            throw new ArgumentException("Expected at least one column", nameof(columns));

        this._columns = columns;
        _alignToRight = new bool[columns.Length];
        _disabled = new bool[columns.Length];
        _maxWidth = new int[columns.Length];
        _width = new int[columns.Length];
        for (var i = 0; i < columns.Length; i++)
        {
            _maxWidth[i] = 80;
            _width[i] = GetVisibleLength(columns[i]);
        }
    }

    public AsciiTable(params AsciiColumn[] columns)
    {
        if (columns == null)
            throw new ArgumentNullException(nameof(columns));

        if (columns.Length == 0)
            throw new ArgumentException("Expected at least one column", nameof(columns));

        this._columns = new string[columns.Length];
        _alignToRight = new bool[columns.Length];
        _disabled = new bool[columns.Length];
        _maxWidth = new int[columns.Length];
        _width = new int[columns.Length];
        for (var i = 0; i < columns.Length; i++)
        {
            this._columns[i] = columns[i].Name ?? "";
            _disabled[i] = columns[i].Disabled;
            _maxWidth[i] = columns[i].MaxWidth;
            _width[i] = GetVisibleLength(columns[i].Name);
            _alignToRight[i] = columns[i].AlignToRight;
        }
    }

    public void DisableColumn(int idx)
    {
        if (idx < 0 || idx > _columns.Length)
            throw new IndexOutOfRangeException();

        _disabled[idx] = true;
    }

    public void DisableColumn(string column)
    {
        var idx = column.IndexOf(column, StringComparison.InvariantCultureIgnoreCase);
        if (idx < 0)
            throw new ArgumentException($"There's no such column as '{column}'", nameof(column));

        DisableColumn(idx);
    }

    public void SetMaxWidth(int idx, int length)
    {
        if (idx < 0 || idx > _columns.Length)
            throw new IndexOutOfRangeException();

        _maxWidth[idx] = length;
    }

    public void SetMaxWidth(string column, int length)
    {
        var idx = column.IndexOf(column, StringComparison.InvariantCultureIgnoreCase);
        if (idx < 0)
            throw new ArgumentException($"There's no such column as '{column}'", nameof(column));

        SetMaxWidth(idx, length);
    }

    public void SetAlignment(int idx, bool toRight)
    {
        if (idx < 0 || idx > _columns.Length)
            throw new IndexOutOfRangeException();

        _alignToRight[idx] = toRight;
    }

    public void SetAlignment(string column, bool toRight)
    {
        var idx = column.IndexOf(column, StringComparison.InvariantCultureIgnoreCase);
        if (idx < 0)
            throw new ArgumentException($"There's no such column as '{column}'", nameof(column));

        SetAlignment(idx, toRight);
    }

    public void Add(params string[] row)
    {
        if (row == null)
            throw new ArgumentNullException(nameof(row));

        if (row.Length != _columns.Length)
            throw new ArgumentException($"Expected row with {_columns.Length} cells, but received row with {row.Length} cells");

        _rows.Add(row);
        for (var i = 0; i < row.Length; i++)
            _width[i] = Math.Max(_width[i], GetVisibleLength(row[i]));
    }

    public override string ToString() => ToString(true);

    public string ToString(bool wrapInCodeBlock)
    {
        for (var i = 0; i < _columns.Length; i++)
            _width[i] = Math.Min(_width[i], _maxWidth[i]);

        var result = new StringBuilder();
        if (wrapInCodeBlock)
            result.AppendLine("```");
        var firstIdx = Array.IndexOf(_disabled, false);
        if (firstIdx < 0)
            throw new InvalidOperationException("Can't format table as every column is disabled");

        // header
        result.Append(PadRightVisible(TrimVisible(_columns[firstIdx], _maxWidth[firstIdx]), _width[firstIdx]));
        for (var i = firstIdx + 1; i < _columns.Length; i++)
            if (!_disabled[i])
                result.Append(" │ ").Append(PadRightVisible(TrimVisible(_columns[i], _maxWidth[i]), _width[i])); // header is always aligned to the left
        result.AppendLine();
        //header separator
        result.Append("".PadRight(_width[firstIdx], '─'));
        for (var i = firstIdx + 1; i < _columns.Length; i++)
            if (!_disabled[i])
                result.Append("─┼─").Append("".PadRight(_width[i], '─'));
        result.AppendLine();
        //rows
        foreach (var row in _rows)
        {
            var cell = TrimVisible(row[firstIdx], _maxWidth[firstIdx]);
            result.Append(_alignToRight[firstIdx] ? PadLeftVisible(cell, _width[firstIdx]) : PadRightVisible(cell, _width[firstIdx]));
            for (var i = firstIdx + 1; i < row.Length; i++)
                if (!_disabled[i])
                {
                    cell = TrimVisible(row[i], _maxWidth[i]);
                    result.Append(" │ ").Append(_alignToRight[i] ? PadLeftVisible(cell, _width[i]) : PadRightVisible(cell, _width[i]));
                }
            result.AppendLine();
        }
        if (wrapInCodeBlock)
            result.Append("```");
        return result.ToString();
    }

    private static int GetVisibleLength(string s)
    {
        if (string.IsNullOrEmpty(s))
            return 0;

        var c = 0;
        var e = StringInfo.GetTextElementEnumerator(s.Normalize());
        while (e.MoveNext())
        {
            var strEl = e.GetTextElement();
            foreach (var chr in strEl)
            {
                var category = char.GetUnicodeCategory(chr);
                if (char.IsControl(chr)
                    || category == UnicodeCategory.Format
                    || category == UnicodeCategory.ModifierSymbol
                    || category == UnicodeCategory.NonSpacingMark
                    || char.IsHighSurrogate(chr))
                    continue;

                c++;
            }
        }
        return c;
    }

    private static string TrimVisible(string s, int maxLength)
    {
        if (maxLength < 1)
            throw new ArgumentException("Max length can't be less than 1", nameof(maxLength));

        if (s.Length <= maxLength)
            return s;

        var c = 0;
        var e = StringInfo.GetTextElementEnumerator(s.Normalize());
        var result = new StringBuilder();
        while (e.MoveNext() && c < maxLength - 1)
        {
            var strEl = e.GetTextElement();
            result.Append(strEl);
            if (char.IsControl(strEl[0]) || char.GetUnicodeCategory(strEl[0]) == UnicodeCategory.Format)
                continue;

            c++;
        }
        return result.Append('…').ToString();
    }

    private static string PadLeftVisible(string s, int totalWidth, char padding = ' ')
    {
        var valueWidth = GetVisibleLength(s);
        var diff = s.Length - valueWidth;
        totalWidth += diff;
        return s.PadLeft(totalWidth, padding);
    }

    private static string PadRightVisible(string s, int totalWidth, char padding = ' ')
    {
        var valueWidth = GetVisibleLength(s);
        var diff = s.Length - valueWidth;
        totalWidth += diff;
        return s.PadRight(totalWidth, padding);
    }
}