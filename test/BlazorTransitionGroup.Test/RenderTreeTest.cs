using BlazorTransitionGroup.Test.Views;
using Bunit;
using Bunit.Extensions;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorTransitionGroup.Test;

public class RenderTreeTest {
    [Fact]
    public void Ensure_RenderTree_is_Correct() {
        using var fake = new TestContext();
        var fakeRendered = fake.RenderComponent<FakeTransitionGroupCounter>();

        using var actual = new TestContext();
        var actualRendered = actual.RenderComponent<TransitionGroupCounter>();

        using var counterContext = new TestContext();
        var counterContextRendered = actual.RenderComponent<Counter>();

        Assert.NotEqual(fakeRendered.Markup, counterContextRendered.Markup);
    }
}