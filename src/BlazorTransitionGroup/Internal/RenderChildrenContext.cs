namespace BlazorTransitionGroup.Internal;

class RenderChildrenContext {
    public Dictionary<object, int> Keys = new();

    public HashSet<object> InsertedKeys = new();

    public List<RenderFrameBuilder> Sequence { get; }

    public RenderChildrenContext(List<RenderFrameBuilder> sequence) {
        Sequence = sequence;
        UpdateKeys();
    }

    public void Insert(int i, RenderFrameBuilder child) {
        if (Keys.ContainsKey(child.Key)) {
            throw new Exception("key is already exists");
        }

        InsertedKeys.Add(child.Key);
        Sequence.Insert(i, child);
        UpdateKeys();
    }

    void UpdateKeys() {
        var keys = new Dictionary<object, int>();
        var i = 0;
        foreach (var item in Sequence) {
            if (item.Key is not null)
                keys.TryAdd(item.Key, i++);
        }
        Keys = keys;
    }
}