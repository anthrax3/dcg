@# A template showing how to define global members
@# including methods and variables.
@global
string globalName = "cavingdeep";

string Capitalize(string text)
{
    return char.ToUpper(text[0]) + text.Substring(1);
}
@end_global
Capitalized name is @(Capitalize(globalName)).