namespace BlazorTransitionGroup.Internal;

internal class ThrottledExecutor<T> {
    volatile int LockFlag;
    DateTime LastInvokeTime;
    Timer? ThrottleTimer;

    readonly List<Action<T>> observers = new();
    readonly object locker = new();

    public ushort ThrottleWindowMs { get; private set; }

    public ThrottledExecutor() {
        LastInvokeTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(ushort.MaxValue);
    }

    public IDisposable Subscribe(Action<T> action) {
        lock (locker) {
            observers.Add(action);
        }

        return new Subscription(() => {
            lock (locker) {
                observers.Remove(action);
            }
        });
    }

    public void Invoke(T value, byte maximumInvokesPerSecond = 0) {
        ThrottleWindowMs = maximumInvokesPerSecond switch {
            0 => 0,
            _ => (ushort)(1000 / maximumInvokesPerSecond),
        };

        Invoke(value);
    }

    public void Invoke(T value) {
        // If no throttle window then bypass throttling
        if (ThrottleWindowMs is 0) {
            ExecuteThrottledAction(value);
        }
        else {
            LockAndExecuteOnlyIfNotAlreadyLocked(() => {
                // If waiting for a previously throttled notification to execute
                // then ignore this notification request
                //if (InvokingSuspended)
                //    return;

                var millisecondsSinceLastInvoke =
                    (int)(DateTime.UtcNow - LastInvokeTime).TotalMilliseconds;

                // If last execute was outside the throttle window then execute immediately
                if (millisecondsSinceLastInvoke >= ThrottleWindowMs) {
                    ExecuteThrottledAction(value);
                }
                else {
                    // This is exactly the second invoke within the time window,
                    // so set a timer that will trigger at the start of the next
                    // time window and prevent further invokes until
                    // the timer has triggered
                    ThrottleTimer?.Dispose();
                    ThrottleTimer = new Timer(
                        callback: _ => ExecuteThrottledAction(value),
                        state: null,
                        dueTime: ThrottleWindowMs - millisecondsSinceLastInvoke,
                        period: 0
                    );
                }
            });
        }
    }

    private void LockAndExecuteOnlyIfNotAlreadyLocked(Action action) {
        if (Interlocked.CompareExchange(ref LockFlag, 1, 0) is 0) {
            try {
                action();
            }
            finally {
                LockFlag = 0;
            }
        }
    }

    private void ExecuteThrottledAction(T value) {
        try {
            lock (locker) {
                foreach (var observer in observers) {
                    observer(value);
                }
            }
        }
        finally {
            ThrottleTimer?.Dispose();
            ThrottleTimer = null;
            LastInvokeTime = DateTime.UtcNow;
        }
    }
}