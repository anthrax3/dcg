@param name: string
@global
string Capitalize(string text)
{
    return char.ToUpper(text[0]) + text.Substring(1);
}
@end_global
public string @(Capitalize(name))
{
    get {return this.@(name);}
    set {this.@(name) = value;}
}