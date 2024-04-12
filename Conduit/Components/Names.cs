using Radix;

namespace Conduit.Components.Names.Attributes
{
    [Literal]
    public partial struct html : AttributeName { }
}

namespace Conduit.Components.Names.Elements
{
    [Literal]
    public partial struct html : ElementName { }

    [Literal(StringRepresentation = "!DOCTYPE")]
    public partial struct doctype : ElementName { }
}