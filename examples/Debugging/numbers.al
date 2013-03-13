@# This is a number listing template for demostration.
@# You can render this template to see its output.
@# @param count int
@param count:int
@code
for (int i = 1; i <= count; i++)
{
    // throw new InvalidOperationException("yeah");
    @text
    @# @(ii)
    @(i)
    @end_text
}
@end_code