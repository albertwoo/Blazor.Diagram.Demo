using System.Text.Json;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagram.Demo.Client.Pages.Dashboard.Links;
using Blazor.Diagram.Demo.Client.Pages.Dashboard.Nodes;

namespace Blazor.Diagram.Demo.Client.Pages.Dashboard;

public class DashboardLayout
{
    public required IEnumerable<NodeModelBase> Nodes { get; init; }
    public required IEnumerable<LinkContext> Links { get; init; }


    public string SerializeAsJson()
    {
        return JsonSerializer.Serialize(this, jsonSerializerOptions);
    }

    public void ApplyToDiagram(Diagrams.Core.Diagram diagram, Action<Model>? StyledLinkModel_Changed = null, Func<NodeModelBase, NodeModelBase>? mapNode = null, Func<BaseLinkModel, BaseLinkModel>? mapLink = null)
    {
        diagram.Nodes.Add(mapNode == null ? Nodes : Nodes.Select(mapNode));

        foreach (var link in Links)
        {
            var sourceNode = diagram.Nodes.Where(x => x is NodeModelBase baseNode && baseNode.RefId == link.SourceId).FirstOrDefault();
            var targetNode = diagram.Nodes.Where(x => x is NodeModelBase baseNode && baseNode.RefId == link.TargetId).FirstOrDefault();
            if (sourceNode != null && targetNode != null)
            {
                var sourcePort = sourceNode.GetPort(link.SourceAlignment);
                var targetPort = targetNode.GetPort(link.TargetAlignment);
                if (sourcePort != null && targetPort != null)
                {
                    var linkModel = new StyledLinkModel(sourcePort, targetPort)
                    {
                        Dash = link.LineDash,
                        Color = link.LineColor,
                        Animated = link.LineAnimated,
                    };

                    if (StyledLinkModel_Changed != null)
                    {
                        linkModel.Changed += StyledLinkModel_Changed;
                    }

                    if (link.LineShape.HasValue)
                    {
                        switch (link.LineShape.Value)
                        {
                            case LineShape.Curve:
                                linkModel.Router = new NormalRouter();
                                linkModel.PathGenerator = new SmoothPathGenerator();
                                break;
                            case LineShape.Orthogonal:
                                linkModel.Router = new OrthogonalRouter();
                                linkModel.PathGenerator = new StraightPathGenerator();
                                break;
                            default:
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(link.SourceMarkerPath))
                    {
                        linkModel.SourceMarker = new LinkMarker(link.SourceMarkerPath, link.SourceMarkerWidth);
                    }
                    if (!string.IsNullOrEmpty(link.TargetMarkerPath))
                    {
                        linkModel.TargetMarker = new LinkMarker(link.TargetMarkerPath, link.TargetMarkerWidth);
                    }

                    diagram.Links.Add(mapLink == null ? linkModel : mapLink(linkModel));
                }
            }
        }
    }


    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
    };

    public static DashboardLayout FromModels(IEnumerable<NodeModel> nodeModels, IEnumerable<BaseLinkModel> linkModels)
    {
        var nodes = new List<NodeModelBase>();
        var links = new List<LinkContext>();

        foreach (var link in linkModels)
        {
            if (link.Source.Model is PortModel sourcePort && sourcePort.Parent is NodeModelBase sourceNode &&
                link.Target.Model is PortModel targetPort && targetPort.Parent is NodeModelBase targetNode)
            {
                var linkContext = new LinkContext()
                {
                    SourceId = sourceNode.RefId,
                    SourceAlignment = sourcePort.Alignment,
                    TargetId = targetNode.RefId,
                    TargetAlignment = targetPort.Alignment,
                    LineShape = link.Router switch
                    {
                        NormalRouter _ => LineShape.Curve,
                        OrthogonalRouter _ => LineShape.Orthogonal,
                        _ => null,
                    },
                    SourceMarkerPath = link.SourceMarker?.Path,
                    SourceMarkerWidth = link.SourceMarker?.Width ?? 10,
                    TargetMarkerPath = link.TargetMarker?.Path,
                    TargetMarkerWidth = link.TargetMarker?.Width ?? 10,
                };

                if (link is StyledLinkModel styledLinkModel)
                {
                    linkContext.LineDash = styledLinkModel.Dash;
                    linkContext.LineColor = styledLinkModel.Color;
                    linkContext.LineAnimated = styledLinkModel.Animated;
                }

                links.Add(linkContext);
            }
        }

        foreach (var node in nodeModels)
        {
            if (node is NodeModelBase nodeModelBase)
            {
                if (nodeModelBase.RefId == Guid.Empty)
                {
                    nodeModelBase.RefId = Guid.NewGuid();
                }
                nodes.Add(nodeModelBase);
            }
        }

        return new DashboardLayout
        {
            Nodes = nodes,
            Links = links
        };
    }

    public static DashboardLayout FromJson(string json) => JsonSerializer.Deserialize<DashboardLayout>(json, jsonSerializerOptions)!;
}

public class LinkContext
{
    public required Guid SourceId { get; set; }
    public required Guid TargetId { get; set; }
    public PortAlignment SourceAlignment { get; init; }
    public PortAlignment TargetAlignment { get; init; }
    public LineShape? LineShape { get; set; }
    public int LineDash { get; set; }
    public bool LineAnimated { get; set; }
    public string? LineColor { get; set; }
    public string? SourceMarkerPath { get; set; }
    public double SourceMarkerWidth { get; set; } = 10;
    public string? TargetMarkerPath { get; set; }
    public double TargetMarkerWidth { get; set; } = 10;
}

public enum LineShape
{
    Curve,
    Orthogonal
}