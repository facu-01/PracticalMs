using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Api.Rendering;

public class ComponentRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
{
    public async Task<string> RenderAsync<TComponent>(
        Action<ComponentParameterBuilder<TComponent>>? parameterBuilder = null,
        IDictionary<string, object?>? layoutParameters = null,
        bool isPartial = false) where TComponent : IComponent
    {
        await using var htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);

        var parameters = new ComponentParameterBuilder<TComponent>();
        parameterBuilder?.Invoke(parameters);
        var dict = parameters.Build();

        var output = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            return await htmlRenderer.RenderComponentWithLayoutAsync<TComponent>(
                layoutParameters: layoutParameters,
                componentParameters: dict,
                isPartial: isPartial);
        });

        return output;
    }
}

public class ComponentParameterBuilder<TComponent>
{
    private readonly Dictionary<string, object?> _parameters = new();

    public void Add<TProperty>(Expression<Func<TComponent, TProperty>> propertySelector, TProperty value)
    {
        if (propertySelector.Body is MemberExpression memberExpression)
        {
            _parameters[memberExpression.Member.Name] = value;
        }
        else
        {
            throw new ArgumentException("Expression must be a member expression");
        }
    }

    public Dictionary<string, object?> Build() => _parameters;
}

public static class HtmlRendererExtensions
{
    public static async Task<string> RenderComponentWithLayoutAsync<T>(
        this HtmlRenderer renderer,
        IDictionary<string, object?>? layoutParameters = null,
        IDictionary<string, object?>? componentParameters = null,
        bool isPartial = false) where T : IComponent
    {
        var componentType = typeof(T);
        var layoutAttribute = componentType.GetCustomAttribute<LayoutAttribute>();

        var compParams = componentParameters != null
            ? ParameterView.FromDictionary(componentParameters)
            : ParameterView.Empty;

        if (layoutAttribute != null && !isPartial)
        {
            var layoutType = layoutAttribute.LayoutType;

            var layParams = layoutParameters != null
                ? new Dictionary<string, object?>(layoutParameters)
                : new Dictionary<string, object?>();

            layParams["Body"] = (RenderFragment)(builder =>
            {
                builder.OpenComponent(0, componentType);

                // Add parameters to the component
                if (componentParameters != null)
                {
                    foreach (var param in componentParameters)
                    {
                        builder.AddAttribute(1, param.Key, param.Value);
                    }
                }

                builder.CloseComponent();
            });

            var layout = await renderer.RenderComponentAsync(layoutType, ParameterView.FromDictionary(layParams));
            return layout.ToHtmlString();
        }

        var output = await renderer.RenderComponentAsync<T>(compParams);
        return output.ToHtmlString();
    }
}
