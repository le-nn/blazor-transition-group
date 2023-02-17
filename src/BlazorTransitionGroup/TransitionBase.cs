using Microsoft.AspNetCore.Components;

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