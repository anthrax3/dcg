@param from: int
@param to: int
@global
void GenerateNumberList(int from, int to)
{
    for (int i = from; i <= to; i++)
    {
        Dcg.Write(i);
        if (i < to)
        {
            Dcg.Write(", ");
        }
    }
}
@end_global
<p>@(Dcg.FileInfo.FullName)</p>
<p>
@! GenerateNumberList(from, to);
</p>