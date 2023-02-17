using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTransitionGroup.Test.Views.Comopnents;

public class FakeTransitionGroup : ComponentBase {
    /// <summary>
    /// The render fragment for ChildContent.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder) {
        base.BuildRenderTree(builder);
        builder.AddContent(0, ChildContent);
    }
}
