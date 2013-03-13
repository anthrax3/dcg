class TemplateAst
{
    Head Head { get; }
    Body Body { get; }
}

class Head
{
    IList<Reference> References { get; }
    IList<Import> Imports { get; }
    IList<Parameter> Parameters { get; }
    StringBuilder Global { get; }
}

class Reference
{
    string Value { get; set; }
}

class Import
{
    string Value { get; set; }
}

class Parameter
{
    string Name { get; set; }
    string Type { get; set; }
}

interface IDirective
{
    int LeadingWhiteCount { get; set; }
    char LeadingWhiteChar { get; set; }
}

class BaseDirective : IDirective
{
    private int leadingWhiteCount = 0;
    public int LeadingWhiteCount
    {
        get
        {
            return this.leadingWhiteCount;
        }
        set
        {
            this.leadingWhiteCount = value;
        }
    }

    private char leadingWhiteChar = ' ';
    public int LeadingWhiteChar
    {
        get
        {
            return this.leadingWhiteChar;
        }
        set
        {
            this.leadingWhiteChar = value;
        }
    }
}

class Body
{
    IList<IDirective> Directives { get; }
}

class StaticText : BaseDirective
{
    string Value { get; set; }
    bool IsNewLine { get; set; }

    void Escape();
}

class DynamicText : BaseDirective
{
    string Value { get; set; }
}

class Execution : BaseDirective
{
    string Statement { get; set; }
}

class Evaluation : BaseDirective
{
    string Expression { get; set; }
}

class MultiLineEvaluation : BaseDirective
{
    string Expression { get; set; }
}

class Code : BaseDirective
{
    IList<IDirective> Directives { get; }
}

class Text : BaseDirective
{
    IList<IDirective> Directives { get; }
}

class Between : BaseDirective
{
    IList<IDirective> Directives { get; }
}

class Output : BaseDirective
{
    string Key { get; }
    IList<IDirective> Directives { get; }
}

class SectionReference : BaseDirective
{
    string Value { get; set; }
}

class SectionDefinition : BaseDirective
{
    string Name { get; set; }
    IList<IDirective> Directives { get; }
}
