namespace Blazor.Diagram.Demo.Client.Pages.Dashboard.Nodes;

public partial class UrlNode
{
    [Parameter] public UrlNodeModel Node { get; set; } = null!;
}

[NodeParameter("Url Node")]
public class UrlNodeModel : TextNodeModel
{
    public UrlNodeModel()
    {
        FontSize = 18;
        FontWeight = 600;
        FontColor = "blue";
        Title = "External App";
    }

    [NodeParameter("Url")]
    public string? Url { get; set; }
}
