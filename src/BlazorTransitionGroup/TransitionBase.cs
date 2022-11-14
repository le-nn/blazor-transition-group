using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTransitionGroup;

/// <summary>
/// An abstract class to implement transition.
/// </summary>
public abstract class TransitionBase : ComponentBase {
    /// <summary>
    /// Gets or sets the key property to detect that should play animation.
    /// </summary>
    [Parameter]
    public object? Key { get; set; }
}
