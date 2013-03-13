@# This is a number listing template for demostration.
@# You can render this template to see its output.
@param count: int
@code
for (int i = 1; i <= count; i++)
{
    @text
    @(i)
    @end_text
}
@end_code