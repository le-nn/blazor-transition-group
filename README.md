# Blazor Transition Group

Exposes simple components useful for defining entering and exiting transitions. 
it does not animate styles by itself. Instead it exposes transition stages, manages classes and group elements and manipulates the DOM in useful ways, making the implementation of actual visual transitions much easier.

This project is inspired from [React Transition Group](https://github.com/reactjs/react-transition-group).

<img src="./overview.gif" width="600"/>

Here is demo page.

https://le-nn.github.io/blazor-transition-group/

## Installation

```
dotnet add package BlazorTransitionGroup
```

## Supported platform

.NET 7 or higher

## Components

### Transition

The Transition component lets you describe a transition from one component
state to another over time with a simple declarative API. 
Most commonly it's used to animate the mounting and unmounting of a component,
but can also be used to describe in-place transition states as well.

By default the Transition component does not alter the behavior of the component it renders, it only tracks "enter" and "exit" states for the components.
It's up to you to give meaning and effect to those states. For example we can add styles to a component when it enters or exits:

There are 4 main states a Transition can be in:

* ```entering```
* ```entered```
* ```exiting```
* ```exited```

Here is example of how to use Transition component

[Sample code is here](./samples/BlazorTransitionGroup.Samples/Demo/GrowTransition.razor)

Inherits ```BlazorTransitionGroup.Transition``` and override razor template as BuildRenderTree method.

```razor

// GrowTransition.razor
@using BlazorTransitionGroup

@inherits Transition

<div style="@ActualStyle" class="@Class">
    @ChildContent?.Invoke(TransitionState)
</div>

@code {
    string ActualStyle => $"opacity: {Opacity};transform:scale({Size});transition:opacity {Duration / 2}ms ease-in-out,transform {Duration}ms ease-in-out;{Style}";

    string Opacity => TransitionState switch {
        TransitionState.Entering or TransitionState.Entered => "1",
        _ => "0",
    };

    string Size => TransitionState switch {
        TransitionState.Entering or TransitionState.Entered => "1",
        _ => "0",
    };

    double Duration => TransitionState switch {
        TransitionState.Entering or TransitionState.Entered => DurationEnter,
        _ => DurationExit,
    };

    string HeightStyle => TransitionState switch {
        TransitionState.Entering or TransitionState.Entered => "100%",
        _ => "0%",
    };

    [Parameter]
    public string Height { get; set; } = "auto";

    [Parameter]
    public string Width { get; set; } = "auto";

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public string? Class { get; set; }

    protected override void OnParametersSet() {
        base.OnParametersSet();
    }
}



```

### TransitionGroup

The ```<TransitionGroup>``` component manages a set of transition components
(```<Transition>```) in a list. 

Like with the transition components, ```<TransitionGroup>``` is a state machine for managing
the mounting and unmounting of components over time.

Consider the example below. 
As items are removed or added to the TodoList the
in prop is toggled automatically by the ```<TransitionGroup>```.

Note that ```<TransitionGroup>``` does not define any animation behavior!
Exactly how a list item animates is up to the individual transition component.
This means you can mix and match animations across different list items.

```TransitionGroup``` Component requires unique ```@key``` field.

[Sample code in Demo is here](./samples/BlazorTransitionGroup.Samples/Demo/TransitionDemo.razor)

```razor

@using BlazorTransitionGroup

<TransitionGroup>
    @foreach (var (i, text, id) in _items) {
        @if (i % 2 is 0) {
            <GrowTransition @key="@($"{text}-{id}")" Context="state">
                <div class="item d-flex p-3 align-items-center shadow mt-3 rounded-3 bg-white">
                    <button class="btn btn-danger" @onclick="@(() => Remove((i, text, id)))">
                        <i class="oi oi-trash" />
                    </button>
                    <div class="p-1 mx-3" style="width:100px;">@state</div>
                    <div class="p-1 mx-3">@text</div>
                </div>
            </GrowTransition>
        }
        else {
            <SlideTransition @key="@($"{text}-{id}")">
                <div class="item d-flex p-3 align-items-center shadow mt-3 rounded-3 bg-white">
                    <button class="btn btn-danger" @onclick="@(() => Remove((i, text, id)))">
                        <i class="oi oi-trash" />
                    </button>
                    <div class="p-1 mx-3" style="width:100px;"></div>
                    <div class="p-1 mx-3">@text</div>
                </div>
            </SlideTransition>
        }
    }
</TransitionGroup>

<div class="d-flex mt-4">
    <input @bind-value="_text " />
    <button class="btn btn-primary" @onclick="Add"> ADD</button>
</div>

@code {
    string _text = "";
    int _i = 3;

    List<(int Index, string Text, Guid Key)> _items = new() {
        (0, "item 1", Guid.NewGuid()),
        (1, "item 2", Guid.NewGuid()),
        (2, "item 3", Guid.NewGuid()),
    };

    void Add() {
        if (string.IsNullOrWhiteSpace(_text)) {
            return;
        }

        _items.Add((_i++, _text, Guid.NewGuid()));
        _text = "";
    }

    void Remove((int, string, Guid) text) {
        _items.Remove(text);
    }
}

```

### TransitionBase

Another option to implements transition is to inherit ```TransitionBase```.
Transition can be implemented with composition.

RenderFragment context provider stransition state.


[Sample code in Demo is here](./samples/BlazorTransitionGroup.Samples/Demo/SlideTransition.razor)

Here is example of how to use Transition component.
Don't forget to specify the Key as a Transition Attribute.

```razor

@using BlazorTransitionGroup

@inherits TransitionBase

<Transition Key="Key" Context="transitionState">
    <div style="@(GetActualStyle(transitionState))" class="@Class">
        @ChildContent
    </div>
</Transition>

@code {
    string GetActualStyle(TransitionState state) {
        var (x, opacity) = (GetX(state), GetOpacity(state));
        return $"opacity: {opacity};transform:translateX({x});transition:opacity {Duration / 2}ms ease-in-out,transform {Duration}ms ease-in-out;{Style}";
    }

    string GetOpacity(TransitionState state) {
        return state switch {
            TransitionState.Entering or TransitionState.Entered => "1",
            _ => "0",
        };
    }

    double Duration => 800;

    string GetX(TransitionState state) {
        return state switch {
            TransitionState.Entering or TransitionState.Entered => "0%",
            _ => "-50%",
        };
    }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}

```

# License
Designed with ♥ by le-nn. Licensed under the MIT License.
