using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Api.Rendering;

public class ComponentRenderer
{
    private readonly HtmlRenderer _renderer;

    public ComponentRenderer(HtmlRenderer renderer)
    {
        _renderer = renderer;
    }

    public async Task<string> RenderAsync<TComponent>()
        where TComponent : IComponent
    {
        return await RenderAsync<TComponent>(ParameterView.Empty);
    }

    public async Task<string> RenderAsync<TComponent>(
        Action<ComponentParameterBuilder<TComponent>> configure)
        where TComponent : IComponent
    {
        var builder = new ComponentParameterBuilder<TComponent>();
        configure(builder);
        return await RenderAsync<TComponent>(builder.Build());
    }

    private async Task<string> RenderAsync<TComponent>(ParameterView parameters)
        where TComponent : IComponent
    {
        var output = await _renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await _renderer.RenderComponentAsync<TComponent>(parameters);
            return result.ToHtmlString();
        });

        return output;
    }


    public async Task<string> RenderLayoutAsync<TLayout>()
        where TLayout : LayoutComponentBase
    {
        return await RenderAsync<TLayout>(ParameterView.Empty);
    }

    public async Task<string> RenderLayoutAsync<TLayout>(
    string bodyContent)
    where TLayout : LayoutComponentBase
    {
        var builder = new ComponentParameterBuilder<TLayout>();

        // inyectamos el contenido del body como un RenderFragment
        builder.Add(c => c.Body, b =>
        {
            b.AddMarkupContent(0, bodyContent);
        });

        return await RenderAsync<TLayout>(builder.Build());
    }

    public async Task<string> RenderLayoutAsync<TLayout>(
        string bodyContent,
        Action<ComponentParameterBuilder<TLayout>> configure)
        where TLayout : LayoutComponentBase
    {
        var builder = new ComponentParameterBuilder<TLayout>();
        configure(builder);

        // inyectamos el contenido del body como un RenderFragment
        builder.Add(c => c.Body, b =>
        {
            b.AddMarkupContent(0, bodyContent);
        });

        return await RenderAsync<TLayout>(builder.Build());
    }


    public class ComponentParameterBuilder<TComponent> where TComponent : IComponent
    {
        private readonly Dictionary<string, object?> _parameters = new();

        /// <summary>
        /// Agrega un parámetro al componente usando una expresión fuertemente tipada.
        /// Solo acepta propiedades marcadas con [Parameter].
        /// </summary>
        public ComponentParameterBuilder<TComponent> Add<TValue>(
            Expression<Func<TComponent, TValue>> parameterExpression,
            TValue value)
        {
            if (parameterExpression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException(
                    "La expresión debe ser una propiedad del componente",
                    nameof(parameterExpression));
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException(
                    "La expresión debe referenciar una propiedad",
                    nameof(parameterExpression));
            }

            // Validar que la propiedad tenga el atributo [Parameter]
            if (!propertyInfo.GetCustomAttributes<ParameterAttribute>().Any())
            {
                throw new InvalidOperationException(
                    $"La propiedad '{propertyInfo.Name}' no está marcada con [Parameter]");
            }

            _parameters[propertyInfo.Name] = value;
            return this;
        }

        public ParameterView Build()
        {
            return ParameterView.FromDictionary(_parameters);
        }
    }

}


