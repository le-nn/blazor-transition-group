using BlazorTransitionGroup.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlazorTransitionGroup;

/// <summary>
/// The <TransitionGroup> component manages a set of transition components <Transition> in a list.
/// </summary>
public class TransitionGroup : ComponentBase, IDisposable {
    RenderChildrenContext? _lastContext;
    TransitionGroupContext _animatableComponentContext = new();
    ThrottledExecutor<byte> _executor = new();
    IDisposable _subscription;

    /// <summary>
    /// The render fragment for ChildContent.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public TransitionGroup() {
        _animatableComponentContext.StateHasChanged += () => {
            //   _executor.Invoke(0, 5);
            StateHasChanged();
        };

        _subscription = _executor.Subscribe(_ => {
            //  StateHasChanged();
        });
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder) {
#if DEBUG
        var sw = Stopwatch.StartNew();
#endif

        int seq = 0;
        var subBuilder = new RenderTreeBuilder();
        subBuilder.AddContent(0, ChildContent);

        if (_lastContext is null) {
            _lastContext = BuildChildrenAsTree(subBuilder.GetFrames());
            BuildContent(builder, ref seq, childbuilder => {
                foreach (var child in _lastContext.Sequence) {
                    child.RenderChild(ref seq, childbuilder);
                }
            });
        }
        else {
            var currentContext = BuildChildrenAsTree(subBuilder.GetFrames());

            // Detect diff and insert removed element during animation into current tree.
            foreach (var (lastRenderedContextItem, i) in _lastContext.Sequence.WithIndex()) {
                // When the last element has been removed.
                if (
                    lastRenderedContextItem.Key is not null
                    && currentContext.Keys.ContainsKey(lastRenderedContextItem.Key) is false
                ) {
                    // Insert the removed element to current render tree.
                    // Because animation might be in progress.
                    currentContext.Insert(i, lastRenderedContextItem);

                    if (
                        _animatableComponentContext.AnimatingElements.Contains(lastRenderedContextItem.Key) is false
                        && lastRenderedContextItem.IsAnimationElement is false
                    ) {
                        // Make the element to animating state if the element is not animating
                        lastRenderedContextItem.IsAnimationElement = true;
                        _animatableComponentContext.RequestRemoveAnimation(lastRenderedContextItem.Key);
                    }
                }
            }

            BuildContent(builder, ref seq, childbuilder => {
                // Apply computed frames to RenderTree
                foreach (var child in currentContext.Sequence) {
                    if (currentContext.InsertedKeys.Contains(child.Key)) {
                        // If the child is the inserted element, execute when animating only.
                        if (_animatableComponentContext.AnimatingElements.Contains(child.Key)) {
                            child.RenderChild(ref seq, childbuilder);
                        }
                    }
                    else {
                        child.RenderChild(ref seq, childbuilder);
                    }
                }
            });

            _lastContext = currentContext;
        }

#if DEBUG
        // Show debug info to view
        sw.Stop();
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "style", "position:fixed;top:0;right:0;z-index:10000;background:rgba(0,0,0,0.5);color:white;padding:10px;");
        builder.AddContent(seq++, $"Render time: {sw.ElapsedMilliseconds} ms");
        builder.CloseElement();
#endif
    }

    void BuildContent(RenderTreeBuilder builder, ref int seq, RenderFragment childContent) {
        builder.OpenRegion(seq++);
        builder.OpenComponent<CascadingValue<TransitionGroupContext>>(seq++);
        builder.AddAttribute(
            seq++,
            "Value",
            _animatableComponentContext
        );
        builder.AddAttribute(3, "ChildContent", childContent);
        builder.CloseComponent();
        builder.CloseRegion();
    }

#pragma warning disable BL0006
    static RenderChildrenContext BuildChildrenAsTree(ArrayRange<RenderTreeFrame> frames) {
        var closeElementMap = new Dictionary<int, Queue<RenderTreeFrameType>>();
        var cacheSequence = new List<RenderFrameBuilder>();
        RenderFrameBuilder? currentRenderChild = null;
        int depth = 0;
        int cursor = 0;

        foreach (var frame in frames.Array.Take(frames.Count)) {
            if (closeElementMap.TryGetValue(cursor, out var frameTypes)) {
                closeElementMap.Remove(cursor);
                foreach (var frameType in frameTypes) {
                    switch (frameType) {
                        case RenderTreeFrameType.Element:
                            currentRenderChild!.Add(new() {
                                RenderFrame = RenderFrame.CloseElement,
                            });
                            break;
                        case RenderTreeFrameType.Component:
                            currentRenderChild!.Add(new() {
                                RenderFrame = RenderFrame.CloseComponent,
                            });
                            break;
                    }

                    depth--;
                }
            }

            switch (frame.FrameType) {
                case RenderTreeFrameType.Element: {
                        if (depth == 0) {
                            currentRenderChild = new(frame.ElementKey);
                            cacheSequence.Add(currentRenderChild);
                        }

                        int index = cursor + frame.ElementSubtreeLength;
                        if (closeElementMap.ContainsKey(index)) {
                            closeElementMap[index].Enqueue(RenderTreeFrameType.Element);
                        }
                        else {
                            var q = new Queue<RenderTreeFrameType>();
                            q.Enqueue(RenderTreeFrameType.Element);
                            closeElementMap.Add(index, q);
                        }

                        currentRenderChild!.Add(new() {
                            RenderFrame = RenderFrame.OpenElement,
                            Name = frame.ElementName,
                            Key = frame.ElementKey,
                        });

                        depth++;
                        break;
                    }
                case RenderTreeFrameType.Component: {
                        if (depth == 0) {
                            currentRenderChild = new(frame.ComponentKey);
                            cacheSequence.Add(currentRenderChild);
                        }

                        int index = cursor + frame.ComponentSubtreeLength;
                        if (closeElementMap.ContainsKey(index)) {
                            closeElementMap[index].Enqueue(RenderTreeFrameType.Component);
                        }
                        else {
                            var q = new Queue<RenderTreeFrameType>();
                            q.Enqueue(RenderTreeFrameType.Component);
                            closeElementMap.Add(index, q);
                        }

                        var isAppendKey = depth == 0 ;
                        currentRenderChild!.Add(new() {
                            RenderFrame = RenderFrame.OpenComponent,
                            Key = frame.ComponentKey,
                            Type = frame.ComponentType,
                            IsAppendKey = isAppendKey,
                        });

                        depth++;
                        break;
                    }
                case RenderTreeFrameType.Attribute:
                    currentRenderChild!.Add(new() {
                        RenderFrame = RenderFrame.Attribute,
                        RenderTreeFrame = frame,
                    });

                    break;

                case RenderTreeFrameType.Text:
                    currentRenderChild!.Add(new() {
                        RenderFrame = RenderFrame.Text,
                        Text = frame.TextContent,
                    });

                    break;

                case RenderTreeFrameType.ElementReferenceCapture:
                    currentRenderChild!.Add(new() {
                        RenderFrame = RenderFrame.ElementReferenceCapture,
                        ElementReferenceCaptureAction = frame.ElementReferenceCaptureAction,
                    });

                    break;

                case RenderTreeFrameType.ComponentReferenceCapture:
                    currentRenderChild!.Add(new() {
                        RenderFrame = RenderFrame.ComponentReferenceCapture,
                        ComponentReferenceCaptureAction = frame.ComponentReferenceCaptureAction,
                    });

                    break;
                case RenderTreeFrameType.Region:
                case RenderTreeFrameType.None:
                default:
                    break;
            }

            cursor++;
        }

        foreach (var item in closeElementMap.Values) {
            foreach (var frameType in item) {
                switch (frameType) {
                    case RenderTreeFrameType.Element:
                        currentRenderChild!.Add(new() {
                            RenderFrame = RenderFrame.CloseElement,
                        });
                        break;
                    case RenderTreeFrameType.Component:
                        currentRenderChild!.Add(new() {
                            RenderFrame = RenderFrame.CloseComponent,
                        });
                        break;
                }
            }
        }

        return new RenderChildrenContext(cacheSequence);
    }

    public void Dispose() {
        _subscription.Dispose();
    }
}
