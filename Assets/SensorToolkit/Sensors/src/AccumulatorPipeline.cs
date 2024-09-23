using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    [Serializable]
    public abstract class AccumulatorPipeline<REF, T> : ISerializationCallbackReceiver
        where REF : UnityEngine.Object
        where T : IAccumulated<REF, T> {

        TargetsEnumerable targetsEnumerable;
        public TargetsEnumerable OutputTargets {
            get {
                if (targetsEnumerable == null) {
                    targetsEnumerable = new TargetsEnumerable(this);
                }
                return targetsEnumerable;
            }
        }

        OutputsEnumerable outputsEnumerable;
        public OutputsEnumerable Outputs {
            get {
                if (outputsEnumerable == null) {
                    outputsEnumerable = new OutputsEnumerable(this);
                }
                return outputsEnumerable;
            }
        }

        public delegate bool SignalProcessor(in T input, out T processed);

        public SignalProcessor SignalProcessorCallback;

        public event Action<T> OnAdd;
        public event Action<T, T> OnChange;
        public event Action<T> OnRemove;
        public event Action OnSome;
        public event Action OnNone;

        Dictionary<REF, int> inputToMap = new Dictionary<REF, int>();
        Dictionary<REF, int> outputToMap = new Dictionary<REF, int>();
        [NonSerialized] Accumulator<REF, T>[] accumulators = new Accumulator<REF, T>[32];
        [NonSerialized] int accumulatorCount = 0;
        
        [NonSerialized] HashSet<REF> toRemove = new HashSet<REF>();
        [NonSerialized] HashSet<Accumulator<REF, T>> changed = new HashSet<Accumulator<REF, T>>();
        [NonSerialized] List<T> removed = new List<T>();

        struct Event {
            public enum Type { Add, Change, Remove }
            public Type type;
            public T target;
        }
        [NonSerialized] List<Event> eventQueue = new List<Event>();

        [SerializeField] int prevSignalCount;
        int timestamp = 0;
        bool isPlayingEvents = false;
        
        public T GetOutput(REF go) {
            return accumulators[outputToMap[go]].Output;
        }
        
        public bool TryGetOutput(REF go, out T output) {
            if (outputToMap.TryGetValue(go, out var i)) {
                output = accumulators[i].Output;
                return true;
            }
            output = default;
            return false;
        }

        public List<REF> GetInputObjects(REF go, List<REF> storeIn) {
            if (outputToMap.TryGetValue(go, out var i)) {
                foreach (var input in accumulators[i].InputTargets) {
                    storeIn.Add(input);
                }
            }
            return storeIn;
        }

        public bool ContainsOutput(REF go) {
            return outputToMap.ContainsKey(go);
        }

        public void UpdateAllInputs(List<T> nextInputs) {
            if (!AssertRecursionLimitNotReached()) {
                return;
            }

            timestamp += 1;

            toRemove.Clear();
            foreach (var input in inputToMap) {
                toRemove.Add(input.Key);
            }

            foreach (var signal in nextInputs) {
                toRemove.Remove(signal.Object);
                UpdateInputInternal(signal);
            }

            foreach (var remaining in toRemove) {
                RemoveInputInternal(remaining);
            }

            SerializeEvents();
            if (!isPlayingEvents) {
                PlayEvents();
            }
        }

        public void UpdateInput(T signal) {
            if (!AssertRecursionLimitNotReached()) {
                return;
            }

            timestamp += 1;

            UpdateInputInternal(signal);
            SerializeEvents();
            if (!isPlayingEvents) {
                PlayEvents();
            }
        }
        
        public void RemoveInput(REF forObject) {
            if (!AssertRecursionLimitNotReached()) {
                return;
            }

            timestamp += 1;

            RemoveInputInternal(forObject);
            SerializeEvents();
            if (!isPlayingEvents) {
                PlayEvents();
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() {
            /*inputToMap.Clear();
            outputToMap.Clear();
            for (int i = 0; i < accumulatorCount; i++) {
                var acc = accumulators[i];
                outputToMap.Add(acc.OutputTarget, i);
                foreach (var input in acc.InputTargets) {
                    inputToMap.Add(input, i);
                }
            }*/
        }

        int recursionCount = 0;
        bool AssertRecursionLimitNotReached() {
            if (!isPlayingEvents) {
                recursionCount = 0;
                return true;
            }
            recursionCount += 1;
            if (recursionCount > 10) {
                Debug.LogError("It appears that a sensor is being pulsed recursively from a detection event handler. This will be ignored to avoid a stack overflow. To avoid this error make sure that any detection event handlers are not causing a sensor to change its list of detections, and therefore firing a new sequence of events.");
                return false;
            }
            return true;
        }
        
        void UpdateInputInternal(T signal) {
            if (ReferenceEquals(signal.Object, null)) {
                return;
            }
            var processed = signal;
            if (SignalProcessorCallback?.Invoke(in signal, out processed) ?? true) {
                UpdateProcessedInput(signal.Object, processed);
            } else {
                RemoveInputInternal(signal.Object);
            }
        }
        
        void RemoveInputInternal(REF forObject) {
            if (inputToMap.TryGetValue(forObject, out var accIndex)) {
                RemoveInputFromMap(forObject, accIndex);
                inputToMap.Remove(forObject);
            }
        }
        
        void UpdateProcessedInput(REF inputTarget, T processed) {
            if (inputToMap.TryGetValue(inputTarget, out var accIndex)) {
                if (ReferenceEquals(accumulators[accIndex].OutputTarget, processed.Object)) {
                    if (accumulators[accIndex].UpdateInput(inputTarget, processed, timestamp)) {
                        OnChangedEvent(accumulators[accIndex]);
                    }
                } else {
                    RemoveInputFromMap(inputTarget, accIndex);
                    NewProcessedInput(inputTarget, processed);
                }
            } else {
                NewProcessedInput(inputTarget, processed);
            }
        }
        
        void NewProcessedInput(REF inputTarget, T processed) {
            if (!outputToMap.TryGetValue(processed.Object, out var accIndex)) {
                var acc = accumulatorCache.Get();
                acc.Spawn(processed.Object, timestamp);

                // Add the accumulator to the array, resize if needed
                accIndex = accumulatorCount;
                if (accIndex == accumulators.Length) {
                    Array.Resize(ref accumulators, accumulators.Length * 2);
                }
                accumulators[accIndex] = acc;
                accumulatorCount += 1;

                outputToMap[acc.OutputTarget] = accIndex;
            }
            inputToMap[inputTarget] = accIndex;
            if (accumulators[accIndex].UpdateInput(inputTarget, processed, timestamp)) {
                OnChangedEvent(accumulators[accIndex]);
            }
        }
        
        void RemoveInputFromMap(REF inputObject, int accIndex) {
            if (accumulators[accIndex].RemoveInput(inputObject, timestamp)) {
                if (accumulators[accIndex].Inputs.Count > 0) {
                    OnChangedEvent(accumulators[accIndex]);
                } else {
                    OnRemovedEvent(accumulators[accIndex]);
                    outputToMap.Remove(accumulators[accIndex].OutputTarget);
                    accumulatorCache.Dispose(accumulators[accIndex]);

                    // Swap with last accumulator and remove
                    var lastIndex = accumulatorCount - 1;
                    if (accIndex != lastIndex) {
                        var lastAcc = accumulators[lastIndex];
                        accumulators[accIndex] = lastAcc;
                        outputToMap[lastAcc.OutputTarget] = accIndex;
                        foreach (var input in lastAcc.InputTargets) {
                            inputToMap[input] = accIndex;
                        }
                    }
                    accumulatorCount -= 1;
                }
            }
        }

        void OnChangedEvent(Accumulator<REF, T> signal) {
            changed.Add(signal);
        }

        void OnRemovedEvent(Accumulator<REF, T> signal) {
            changed.Remove(signal);
            removed.Add(signal.PreviousOutput);
        }

        void SerializeEvents() {
            foreach (var change in changed) {
                var previousOutput = change.PreviousOutput;
                if (ReferenceEquals(previousOutput.Object, null)) {
                    eventQueue.Add(new Event { type = Event.Type.Add, target = change.Output });
                } else {
                    eventQueue.Add(new Event { type = Event.Type.Change, target = previousOutput });
                    eventQueue.Add(new Event { type = Event.Type.Change, target = change.Output });
                }
            }
            foreach (var remove in removed) {
                eventQueue.Add(new Event { type = Event.Type.Remove, target = remove });
            }
            changed.Clear();
            removed.Clear();
        }

        void PlayEvents() {
            isPlayingEvents = true;
            try {
                var eventIndex = 0;
                while (eventIndex < eventQueue.Count) {
                    var evt = eventQueue[eventIndex];
                    switch (evt.type) {
                        case Event.Type.Add:
                            OnAdd?.Invoke(evt.target);
                            break;
                        case Event.Type.Change:
                            // We store the prevValue as current index. The next value in the next index.
                            eventIndex++;
                            var changeTo = eventQueue[eventIndex];
                            OnChange?.Invoke(evt.target, changeTo.target);
                            break;
                        case Event.Type.Remove:
                            OnRemove?.Invoke(evt.target);
                            break;
                    }
                    eventIndex += 1;
                }
                eventQueue.Clear();

                var signalCount = Outputs.Count;
                if (prevSignalCount == 0 && signalCount > 0) {
                    OnSome?.Invoke();
                } else if (prevSignalCount > 0 && signalCount == 0) {
                    OnNone?.Invoke();
                }
                prevSignalCount = signalCount;
            } finally {
                isPlayingEvents = false;
            }
        }

        AccumulatorCache accumulatorCache = new AccumulatorCache();

        class AccumulatorCache : ObjectCache<Accumulator<REF, T>> {
            public override void Dispose(Accumulator<REF, T> obj) {
                obj.Dispose();
                base.Dispose(obj);
            }
            protected override Accumulator<REF, T> create() {
                var inst = base.create();
                inst.Initialize();
                return inst;
            }
        }

        public class OutputsEnumerable : IEnumerable<T>, IEnumerable {
            AccumulatorPipeline<REF, T> source;
            public OutputsEnumerable(AccumulatorPipeline<REF, T> source) { this.source = source; }
            public Enumerator GetEnumerator() { return new Enumerator(source); }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
            IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

            public int Count {
                get {
                    int n = 0;
                    for (int i = 0; i < source.accumulatorCount; i++) {
                        if (source.accumulators[i].OutputTarget != null) {
                            n += 1;
                        }
                    }
                    return n;
                }
            }

            public struct Enumerator : IEnumerator<T>, IEnumerator {
                AccumulatorPipeline<REF, T> source;
                int index;
                public Enumerator(AccumulatorPipeline<REF, T> source) {
                    this.source = source;
                    index = -1;
                }
                public T Current { get { return source.accumulators[index].Output; } }
                object IEnumerator.Current => throw new NotImplementedException();
                public void Dispose() { }
                public bool MoveNext() {
                    index += 1;
                    if (index >= source.accumulatorCount) {
                        return false;
                    }
                    if (Current.Object == null) {
                        return MoveNext();
                    }
                    return true;
                }
                public void Reset() { index = -1; }
            }
        }

        public class TargetsEnumerable : IEnumerable<REF>, IEnumerable {
            AccumulatorPipeline<REF, T> source;
            public TargetsEnumerable(AccumulatorPipeline<REF, T> source) { this.source = source; }
            public Enumerator GetEnumerator() { return new Enumerator(source); }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
            IEnumerator<REF> IEnumerable<REF>.GetEnumerator() { return GetEnumerator(); }

            public int Count {
                get {
                    int n = 0;
                    for (int i = 0; i < source.accumulatorCount; i++) {
                        if (source.accumulators[i].OutputTarget != null) {
                            n += 1;
                        }
                    }
                    return n;
                }
            }

            public struct Enumerator : IEnumerator<REF>, IEnumerator {
                AccumulatorPipeline<REF, T> source;
                int index;
                public Enumerator(AccumulatorPipeline<REF, T> source) {
                    this.source = source;
                    index = -1;
                }
                public REF Current { get { return source.accumulators[index].OutputTarget; } }
                object IEnumerator.Current => throw new NotImplementedException();
                public void Dispose() { }
                public bool MoveNext() {
                    index += 1;
                    if (index >= source.accumulatorCount) {
                        return false;
                    }
                    if (Current == null) {
                        return MoveNext();
                    }
                    return true;
                }
                public void Reset() { index = -1; }
            }
        }

    }
}
