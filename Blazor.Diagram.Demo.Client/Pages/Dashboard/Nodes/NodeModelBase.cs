using System.Text.Json.Serialization;
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;

namespace Blazor.Diagram.Demo.Client.Pages.Dashboard.Nodes;

[JsonDerivedType(typeof(TextNodeModel), typeDiscriminator: nameof(TextNodeModel))]
[JsonDerivedType(typeof(UrlNodeModel), typeDiscriminator: nameof(UrlNodeModel))]
[JsonDerivedType(typeof(ImageNodeModel), typeDiscriminator: nameof(ImageNodeModel))]
[JsonDerivedType(typeof(EmbedNodeModel), typeDiscriminator: nameof(EmbedNodeModel))]
public class NodeModelBase : NodeModel
{
    public Guid RefId { get; set; } = Guid.NewGuid();

    public static void RegisterAllDerivedModels(BlazorDiagram diagram)
    {
        diagram.RegisterComponent<TextNodeModel, TextNode>();
        diagram.RegisterComponent<UrlNodeModel, UrlNode>();
        diagram.RegisterComponent<ImageNodeModel, ImageNode>();
        diagram.RegisterComponent<EmbedNodeModel, EmbedNode>();
    }
}
