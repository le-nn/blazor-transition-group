using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTransitionGroup;

public class TransitionBase : ComponentBase {
    [Parameter]
    public object? Key { get; set; }
}
