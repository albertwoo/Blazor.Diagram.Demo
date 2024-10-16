using Blazor.Diagram.Demo.Client.Pages.Dashboard.Behaviors;
using Blazor.Diagram.Demo.Client.Pages.Dashboard.Links;
using Blazor.Diagram.Demo.Client.Pages.Dashboard.Nodes;
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Behaviors;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagrams.Options;
using BlazorContextMenu;
using Blazored.LocalStorage;

namespace Blazor.Diagram.Demo.Client.Pages.Dashboard;

public partial class Dashboard
{
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required ILocalStorageService LocalStorageService { get; set; }

    private readonly Dictionary<string, string> customizedLinkStyle = [];

    private string? name { get; set; }
    private string? descrition { get; set; }
    private bool isLoading = true;
    private bool showGrid;
    private bool isPointerReleased = true;
    private BlazorDiagram diagram = null!;
    private Model? editingModel;

    protected override void OnInitialized()
    {
        var options = new BlazorDiagramOptions
        {
            AllowMultiSelection = true,
            AllowPanning = true,
            GridSnapToCenter = true,
            GridSize = 4,
            Zoom =
            {
                Enabled = true,
            },
            Links =
            {
                DefaultRouter = new NormalRouter(),
                DefaultPathGenerator = new SmoothPathGenerator()
            },
        };

        diagram = new BlazorDiagram(options);

        NodeModelBase.RegisterAllDerivedModels(diagram);

        diagram.UnregisterBehavior<SelectionBehavior>();
        diagram.UnregisterBehavior<DragMovablesBehavior>();
        diagram.RegisterBehavior(new SelectionBehavior2(diagram));
        diagram.RegisterBehavior(new DragMovablesBehavior(diagram));

        diagram.SelectionChanged += Diagram_SelectionChanged;
        diagram.PointerDown += Diagram_PointerDown;
        diagram.PointerUp += Diagram_PointerUp;

        diagram.Links.Added += Links_Added;
        diagram.Links.Removed += Links_Removed;
        diagram.Nodes.Removed += Nodes_Removed;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(10); // Let the diagram layout to be fully ready
            await LoadDashboard();
        }
    }

    private void Diagram_SelectionChanged(SelectableModel model)
    {
        if (model.Selected)
        {
            editingModel = model;
        }
        else
        {
            editingModel?.Refresh();
            editingModel = null;
        }

        StateHasChanged();
    }

    private void Diagram_PointerDown(Model? arg1, Blazor.Diagrams.Core.Events.PointerEventArgs arg2)
    {
        isPointerReleased = false;
        StateHasChanged();
    }

    private void Diagram_PointerUp(Model? arg1, Blazor.Diagrams.Core.Events.PointerEventArgs arg2)
    {
        isPointerReleased = true;
        StateHasChanged();
    }

    private void Links_Added(BaseLinkModel link)
    {
        if (link is not StyledLinkModel linkModel)
        {
            link.Changed += Link_Changed;
        }
    }

    private void Links_Removed(BaseLinkModel obj)
    {
        if (obj is StyledLinkModel linkModel)
        {
            linkModel.Changed -= StyledLinkModel_Changed;
        }
        StateHasChanged();
    }

    private void Nodes_Removed(NodeModel obj)
    {
    }

    private void Link_Changed(Model link)
    {
        // Relapace default LinkModel with StyledLinkModel
        if (link is LinkModel linkModel && linkModel.IsAttached)
        {
            link.Changed -= Link_Changed;

            var styledLinkModel = new StyledLinkModel(linkModel.Source, linkModel.Target);

            diagram.Links.Remove(linkModel);
            diagram.Links.Add(styledLinkModel);
            diagram.SelectModel(styledLinkModel, true);

            styledLinkModel.Changed += StyledLinkModel_Changed;
            StyledLinkModel_Changed(styledLinkModel);
        }
    }

    private void StyledLinkModel_Changed(Model model)
    {
        if (model is StyledLinkModel styledLinkModel)
        {
            if (styledLinkModel.Dash != 0 || styledLinkModel.Color != StyledLinkModel.DefaultColor)
            {
                customizedLinkStyle[styledLinkModel.Id] = styledLinkModel.MakeStyleClass();
            }
            else
            {
                customizedLinkStyle.Remove(styledLinkModel.Id);
            }
            StateHasChanged();
        }
    }

    private void AddNode(ItemClickEventArgs e, NodeModel node)
    {
        node.Position = new Point(e.MouseEvent.ClientX - e.MouseEvent.OffsetX - diagram.Pan.X, e.MouseEvent.ClientY - e.MouseEvent.OffsetY - 80 - diagram.Pan.Y);
        diagram.Nodes.Add(node);
        diagram.SelectModel(node, true);
        StateHasChanged();
    }

    private async Task Save()
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            var layout = DashboardLayout.FromModels(diagram.Nodes, diagram.Links).SerializeAsJson();
            await LocalStorageService.SetItemAsync($"dashboard_0", new DashboardModel
            {
                Name = name,
                Description = descrition,
                Layout = layout
            });
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadDashboard()
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            var dashboard = await LocalStorageService.GetItemAsync<DashboardModel>($"dashboard_0");
            if (dashboard == null) return;

            diagram.Nodes.Clear();
            diagram.Links.Clear();

            name = dashboard.Name;
            descrition = dashboard.Description;
            DashboardLayout.FromJson(dashboard.Layout).ApplyToDiagram(diagram, StyledLinkModel_Changed);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private void CloseModelEditor()
    {
        editingModel?.Refresh();
        editingModel = null;
    }

    private void PasteSelectedItems(ItemClickEventArgs e)
    {
        var selectedItems = diagram.GetSelectedModels();
        if (selectedItems.Any())
        {
            var nodes = selectedItems.Where(x => x is NodeModelBase).Select(x => (x as NodeModelBase)!).ToList();
            var links = selectedItems.Where(x => x is BaseLinkModel).Select(x => (x as BaseLinkModel)!).ToHashSet()!;

            // If link is attached between selected nodes, we should also include it for copying
            foreach (var node in nodes)
            {
                var tryAddLink = (BaseLinkModel? link) =>
                {
                    if (link?.Source.Model is PortModel sourcePortModel && nodes.Contains(sourcePortModel.Parent) &&
                        link?.Target.Model is PortModel targetPortModel && nodes.Contains(targetPortModel.Parent))
                    {
                        links.Add(link);
                    }
                };

                foreach (var link in node.Links)
                {
                    tryAddLink(link);
                }

                foreach (var link in node.PortLinks)
                {
                    tryAddLink(link);
                }
            }

            var json = DashboardLayout.FromModels(nodes, links).SerializeAsJson();

            var firstNode = nodes.FirstOrDefault();

            // TODO: The delta needs to be optimized
            var deltaX = e.MouseEvent.ClientX - e.MouseEvent.OffsetX - diagram.Pan.X - firstNode?.Position.X;
            var deltaY = e.MouseEvent.ClientY - e.MouseEvent.OffsetY - diagram.Pan.Y - firstNode?.Position.Y;

            var layout = DashboardLayout.FromJson(json);
            foreach (var node in layout.Nodes)
            {
                var oldRefId = node.RefId;
                var newPosition = node.Position.Add(deltaX ?? 0, deltaY ?? 0);

                node.RefId = Guid.NewGuid();
                node.SetPosition(newPosition.X, newPosition.Y);

                // Replace with new RefId
                foreach (var link in layout.Links)
                {
                    if (link.SourceId == oldRefId) link.SourceId = node.RefId;
                    if (link.TargetId == oldRefId) link.TargetId = node.RefId;
                }
            }

            // Unselect all first, so we can select pasted items later
            diagram.UnselectAll();

            layout.ApplyToDiagram(
                diagram,
                mapNode: node =>
                {
                    diagram.SelectModel(node, false);
                    return node;
                },
                mapLink: link =>
                {
                    diagram.SelectModel(link, false);
                    return link;
                }
            );
        }
    }

    private void DeleteSelectedItems()
    {
        foreach (var item in diagram.GetSelectedModels())
        {
            if (item is NodeModel node) diagram.Nodes.Remove(node);
            else if (item is LinkModel link) diagram.Links.Remove(link);
        }
    }

    private void AlignToTop()
    {
        var nodes = diagram.GetSelectedModels().Where(x => x is NodeModelBase).Select(x => (NodeModelBase)x).ToList();
        if (nodes.Count > 1)
        {
            var y = nodes.Select(x => x.Position.Y).Min();
            foreach (var node in nodes)
            {
                node.SetPosition(node.Position.X, y);
            }
        }
    }

    private void AlignToBottom()
    {
        var nodes = diagram.GetSelectedModels().Where(x => x is NodeModelBase).Select(x => (NodeModelBase)x).ToList();
        if (nodes.Count > 1)
        {
            var y = nodes.Select(x => x.Position.Y + x.Size?.Height).Max();
            foreach (var node in nodes)
            {
                node.SetPosition(node.Position.X, y - node.Size?.Height ?? 0);
            }
        }
    }

    private void AlignToLeft()
    {
        var nodes = diagram.GetSelectedModels().Where(x => x is NodeModelBase).Select(x => (NodeModelBase)x).ToList();
        if (nodes.Count > 1)
        {
            var x = nodes.Select(x => x.Position.X).Min();
            foreach (var node in nodes)
            {
                node.SetPosition(x, node.Position.Y);
            }
        }
    }

    private void AlignToRight()
    {
        var nodes = diagram.GetSelectedModels().Where(x => x is NodeModelBase).Select(x => (NodeModelBase)x).ToList();
        if (nodes.Count > 1)
        {
            var x = nodes.Select(x => x.Position.X + x.Size?.Width).Max();
            foreach (var node in nodes)
            {
                node.SetPosition(x - node.Size?.Width ?? 0, node.Position.Y);
            }
        }
    }
}
