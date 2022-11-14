namespace BlazorTransitionGroup.Internal;

class Subscription : IDisposable {
    private readonly Action Action;
    private bool IsDisposed;

    public Subscription(Action action) {
        Action = action;
    }

    public void Dispose() {
        if (IsDisposed) {
            throw new ObjectDisposedException(
                nameof(Subscription),
                $"Attempt to call {nameof(Dispose)} twice on {nameof(Subscription)}."
            );
        }

        IsDisposed = true;
        GC.SuppressFinalize(this);
        Action();
    }

    ~Subscription() {
        if (!IsDisposed)
            throw new InvalidOperationException($"{nameof(Subscription)} was not disposed. ");
    }
}