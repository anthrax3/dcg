@# This template demostrates how to use one template
@# to generate multiple outputs.
@! int count = 5;
@# The following for statement renders in the main output.
@code
for (int i = 0; i < count; i++)
{
    @text
    @(i)
    @end_text
}
@end_code
@# The following renders in "output2" output.
@output output2
@code
for (int i = 0; i < count; i++)
{
    @text
    @(i)
    @end_text
}
@end_code
@end_output