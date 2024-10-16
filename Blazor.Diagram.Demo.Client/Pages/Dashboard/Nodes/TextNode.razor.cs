using Blazor.Diagrams.Core.Models;

namespace Blazor.Diagram.Demo.Client.Pages.Dashboard.Nodes;

public partial class TextNode
{
    [Parameter] public TextNodeModel Node { get; set; } = null!;
}

[NodeParameter("Text Node")]
public class TextNodeModel : NodeModelBase
{
    public TextNodeModel()
    {
        Title = "Text";
        AddPort(PortAlignment.Top);
        AddPort(PortAlignment.Right);
        AddPort(PortAlignment.Bottom);
        AddPort(PortAlignment.Left);
    }

    [NodeParameter("Title")]
    public new string Title { get; set; }
    [NodeParameter("Font Size")]
    public int FontSize { get; set; } = 20;
    [NodeParameter("Font Weight")]
    public int FontWeight { get; set; } = 400;
    [NodeParameter("Font Color")]
    public string FontColor { get; set; } = "black";
    [NodeParameter("Extra Css Style")]
    public string? ExtraStyle { get; set; }
}
