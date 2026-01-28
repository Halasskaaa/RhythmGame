namespace wah.Chart.SSC;

internal readonly record struct NoteTime(ushort MeasureIndex, ushort Row, ushort RowCount)
{
    public float Beat => MeasureIndex + (float)Row / RowCount;
}
