using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTransitionGroup.Internal;

class TransitionGroupContext {
    public HashSet<object> AnimatingElements { get; } = new();

    public event Action<object>? RemoveAnimationRequested;

    public event Action? StateHasChanged;

    public TransitionGroupContext() {
    }

    public IDisposable SubscribeAnimationRequested(object elementkey, Action action) {
        void HandleRequested(object key) {
            if (key.Equals(elementkey)) {
                action();
            }
        }

        RemoveAnimationRequested += HandleRequested;
        return new Subscription(() => {
            RemoveAnimationRequested -= HandleRequested;
        });
    }

    public void BeginAnimation(object key) {
        if (AnimatingElements.Contains(key)) {
            return;
        }

        AnimatingElements.Add(key);
    }

    public void EndAnimation(object key) {
        if (AnimatingElements.Contains(key) is false) {
            return;
        }

        AnimatingElements.Remove(key);
    }

    public void RequestRemoveAnimation(object key) => RemoveAnimationRequested?.Invoke(key);

    public void RequestTransitionGroupStateHasChanged() => StateHasChanged?.Invoke();
}