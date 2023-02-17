using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Runtime.InteropServices;

namespace BlazorTransitionGroup.Internal;

#pragma warning disable BL0006

enum RenderFrame {
    OpenElement,
    CloseElement,
    OpenComponent,
    CloseComponent,
    Attribute,
    Text,
    ElementReferenceCapture,
    ComponentReferenceCapture,
}

[StructLayout(LayoutKind.Explicit, Pack = 4)]
struct Frame {
    [FieldOffset(0)] RenderFrame _renderFrame;

    // ref
    [FieldOffset(4)] Action<ElementReference> _lementReferenceCaptureAction;
    [FieldOffset(4)] Action<object> _componentReferenceCaptureAction;

    // component and element
    [FieldOffset(4)] object? _key;

    // component
    [FieldOffset(12)] Type _type;
    [FieldOffset(20)] bool _isAppendKey;

    // element
    [FieldOffset(12)] string _name;

    // attribute
    [FieldOffset(20)] RenderTreeFrame _renderTreeFrame;

    // text content
    [FieldOffset(4)] string _text;

    public RenderFrame RenderFrame {
        get => _renderFrame;
        set => _renderFrame = value;
    }

    public object? Key {
        get => _key;
        set => _key = value;
    }

    public Type Type {
        get => _type;
        set => _type = value;
    }

    public bool IsAppendKey {
        get => _isAppendKey;
        set => _isAppendKey = value;
    }

    public string Name {
        get => _name;
        set => _name = value;
    }

    public string Text {
        get => _text;
        set => _text = value;
    }

    public RenderTreeFrame RenderTreeFrame {
        get => _renderTreeFrame;
        set => _renderTreeFrame = value;
    }

    public Action<ElementReference> ElementReferenceCaptureAction {
        get => _lementReferenceCaptureAction;
        set => _lementReferenceCaptureAction = value;
    }

    public Action<object> ComponentReferenceCaptureAction {
        get => _componentReferenceCaptureAction;
        set => _componentReferenceCaptureAction = value;
    }
};

class RenderFrameBuilder {
    readonly List<Frame> frames = new();

    public object Key { get; }

    public bool IsAnimationElement { get; set; }

    public RenderFrameBuilder(object key) {
        Key = key;
    }

    public void Add(Frame frame) {
        frames.Add(frame);
    }

    public void RenderChild(ref int sequence, RenderTreeBuilder builder) {
        foreach (var frame in frames) {
            switch (frame.RenderFrame) {
                case RenderFrame.OpenElement: {
                        builder.OpenElement(sequence++, frame.Name);
                        if (frame.Key != null) {
                            builder.SetKey(frame.Name);
                        }
                        break;
                    }
                case RenderFrame.OpenComponent: {
                        builder.OpenComponent(sequence++, frame.Type);
                        if (frame.Key != null) {
                            builder.SetKey(frame.Key);
                        }

                        if (frame.IsAppendKey) {
                            builder.AddAttribute(sequence++, "Key", frame.Key);
                        }
                        break;
                    }
                case RenderFrame.Attribute: {
                        builder.AddAttribute(sequence++, frame.RenderTreeFrame);
                        break;
                    }
                case RenderFrame.Text: {
                        builder.AddContent(sequence++, frame.Text);
                        break;
                    }
                case RenderFrame.ElementReferenceCapture: {
                        builder.AddElementReferenceCapture(sequence++, frame.ElementReferenceCaptureAction);
                        break;
                    }
                case RenderFrame.ComponentReferenceCapture: {
                        builder.AddComponentReferenceCapture(sequence++, frame.ComponentReferenceCaptureAction);
                        break;
                    }
                case RenderFrame.CloseComponent: {
                        builder.CloseComponent();
                        break;
                    }
                case RenderFrame.CloseElement: {
                        builder.CloseElement();
                        break;
                    }
            }
        }
    }
}

public static class WithIndexExtension {
    public static IEnumerable<(T Value, int Index)> WithIndex<T>(this IEnumerable<T> sequence) {
        return sequence.Select((x, i) => (x, i));
    }
}