using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Models;

namespace Blazor.Diagram.Demo.Client.Pages.Dashboard.Links;

public class StyledLinkModel : LinkModel
{
    public const string DefaultColor = "grey";

    public StyledLinkModel(PortModel sourcePort, PortModel targetPort) : base(sourcePort, targetPort)
    {
        Color = DefaultColor;
    }

    public StyledLinkModel(Anchor source, Anchor target) : base(source, target)
    {
        Color = DefaultColor;
    }

    public int Dash { get; set; } = 0;
    public bool Animated { get; set; }

    public static string MakeDefaultStyleClass()
    {
        return $$"""
            g.diagram-link > path:not(.selection-helper) {
                stroke-dasharray: 0;
            }

            @keyframes diagram-link-dash-animation {
                100% {
                    stroke-dashoffset: -10;
                }
            }
            """;
    }

    public string MakeStyleClass()
    {
        var animatedCss = Animated ? "animation: diagram-link-dash-animation .5s linear infinite;" : "";
        return $$"""
            g.diagram-link[data-link-id="{{Id}}"] > path:not(.selection-helper) {
                stroke-dasharray: {{Dash}};
                {{animatedCss}}
            }
            """;
    }
}
