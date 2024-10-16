using Blazor.Diagrams.Core.Models;

namespace Blazor.Diagram.Demo.Client.Pages.Dashboard.Nodes;

public partial class ImageNode
{
    [Parameter] public ImageNodeModel Node { get; set; } = null!;
}

[NodeParameter("Image Node")]
public class ImageNodeModel : NodeModelBase
{
    public ImageNodeModel()
    {
        AddPort(PortAlignment.Top);
        AddPort(PortAlignment.Right);
        AddPort(PortAlignment.Bottom);
        AddPort(PortAlignment.Left);
    }

    [NodeParameter<ImageBase64Uploader, string>("Image(<5MB)")]
    public string? ImageBase64 { get; set; }
    [NodeParameter]
    public int Width { get; set; } = 200;
    [NodeParameter]
    public int Height { get; set; } = 200;
}
