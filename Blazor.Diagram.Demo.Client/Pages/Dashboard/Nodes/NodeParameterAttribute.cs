namespace Blazor.Diagram.Demo.Client.Pages.Dashboard.Nodes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
public class NodeParameterAttribute(string? name = null, Type? fieldRenderType = null, bool textarea = false) : Attribute
{
    public string? Name => name;
    public Type? FieldRenderType => fieldRenderType;
    public bool Textarea => textarea;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
public class NodeParameterAttribute<TComponent, TValue>(string? name = null) : NodeParameterAttribute(name, typeof(TComponent))
    where TComponent : NodeParameterComponentBase<TValue>;

public class NodeParameterComponentBase<T> : ComponentBase
{
    [Parameter] public required T Value { get; set; }
    [Parameter] public Action<object?>? ValueChanged { get; set; }
}
