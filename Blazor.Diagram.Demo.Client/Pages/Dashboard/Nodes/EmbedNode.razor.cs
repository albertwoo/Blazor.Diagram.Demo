namespace Blazor.Diagram.Demo.Client.Pages.Dashboard.Nodes;

public partial class EmbedNode
{
    [Parameter] public EmbedNodeModel Node { get; set; } = null!;
}

[NodeParameter("Embed Node")]
public class EmbedNodeModel : TextNodeModel
{
    public EmbedNodeModel()
    {
        FontSize = 18;
        FontWeight = 600;
        FontColor = "#3873ff";
        Title = "Embed View";
    }

    [NodeParameter("Embed")]
    public string? EmbedContent { get; set; }

    [NodeParameter("Inline")]
    public bool Inline { get; set; }

    public bool IsOpen { get; set; }
}
