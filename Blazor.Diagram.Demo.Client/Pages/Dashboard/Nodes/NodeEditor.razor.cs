using System.Reflection;

namespace Blazor.Diagram.Demo.Client.Pages.Dashboard.Nodes;

public partial class NodeEditor<T> where T : NodeModelBase
{
    [Parameter] public required T NodeModel { get; set; }

    private string title = "";
    private IEnumerable<(PropertyInfo Property, NodeParameterAttribute ParameterAttr)> properties = [];

    protected override void OnInitialized()
    {
        var modelType = NodeModel.GetType();
        var parameterAttr = modelType.GetCustomAttributes(typeof(NodeParameterAttribute), false).FirstOrDefault();

        title = ((NodeParameterAttribute?)parameterAttr)?.Name ?? modelType.Name;

        properties = modelType.GetProperties()
            .Select(p =>
            {
                var parameterAttr = p.GetCustomAttributes(typeof(NodeParameterAttribute), false).FirstOrDefault();
                return (p, (NodeParameterAttribute?)parameterAttr);
            })
            .Where(x => x.Item2 != null)
            .Select(x => (Property: x.p, ParameterAttr: x.Item2!))
            .ToList();
    }
}
