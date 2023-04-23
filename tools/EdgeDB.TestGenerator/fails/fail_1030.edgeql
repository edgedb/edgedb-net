System.AggregateException: One or more errors occurred. (value is not a dictionary) (value is not a dictionary) (value is not a dictionary) (value is not a dictionary)
 ---> System.InvalidOperationException: value is not a dictionary
   at EdgeDB.TestGenerator.ValueFormatter.Format(Object edgedbValue, Object generatedValue, Action progress, IValueProvider provider, IWrappingValueProvider parent) in /Users/quinch/Documents/GitHub/EdgeDB.Net/tools/EdgeDB.TestGenerator/ValueFormatter.cs:line 48
   at EdgeDB.TestGenerator.ValueFormatter.<>c__DisplayClass0_6.<Format>b__5(Int32 i) in /Users/quinch/Documents/GitHub/EdgeDB.Net/tools/EdgeDB.TestGenerator/ValueFormatter.cs:line 183
   at System.Threading.Tasks.Parallel.<>c__DisplayClass19_0`1.<ForWorker>b__1(RangeWorker& currentWorker, Int32 timeout, Boolean& replicationDelegateYieldedBeforeCompletion)
--- End of stack trace from previous location ---
   at System.Threading.Tasks.Parallel.<>c__DisplayClass19_0`1.<ForWorker>b__1(RangeWorker& currentWorker, Int32 timeout, Boolean& replicationDelegateYieldedBeforeCompletion)
   at System.Threading.Tasks.TaskReplicator.Replica.Execute()
   --- End of inner exception stack trace ---
   at System.Threading.Tasks.TaskReplicator.Run[TState](ReplicatableUserAction`1 action, ParallelOptions options, Boolean stopOnFirstFailure)
   at System.Threading.Tasks.Parallel.ForWorker[TLocal](Int32 fromInclusive, Int32 toExclusive, ParallelOptions parallelOptions, Action`1 body, Action`2 bodyWithState, Func`4 bodyWithLocal, Func`1 localInit, Action`1 localFinally)
--- End of stack trace from previous location ---
   at System.Threading.Tasks.Parallel.ForWorker[TLocal](Int32 fromInclusive, Int32 toExclusive, ParallelOptions parallelOptions, Action`1 body, Action`2 bodyWithState, Func`4 bodyWithLocal, Func`1 localInit, Action`1 localFinally)
   at System.Threading.Tasks.Parallel.For(Int32 fromInclusive, Int32 toExclusive, Action`1 body)
   at EdgeDB.TestGenerator.ValueFormatter.Format(Object edgedbValue, Object generatedValue, Action progress, IValueProvider provider, IWrappingValueProvider parent) in /Users/quinch/Documents/GitHub/EdgeDB.Net/tools/EdgeDB.TestGenerator/ValueFormatter.cs:line 181
   at EdgeDB.TestGenerator.Generators.TestGenerator.<>c__DisplayClass11_0.<<GenerateAsync>b__0>d.MoveNext() in /Users/quinch/Documents/GitHub/EdgeDB.Net/tools/EdgeDB.TestGenerator/Generators/TestGenerator.cs:line 110
 ---> (Inner Exception #1) System.InvalidOperationException: value is not a dictionary
   at EdgeDB.TestGenerator.ValueFormatter.Format(Object edgedbValue, Object generatedValue, Action progress, IValueProvider provider, IWrappingValueProvider parent) in /Users/quinch/Documents/GitHub/EdgeDB.Net/tools/EdgeDB.TestGenerator/ValueFormatter.cs:line 48
   at EdgeDB.TestGenerator.ValueFormatter.<>c__DisplayClass0_6.<Format>b__5(Int32 i) in /Users/quinch/Documents/GitHub/EdgeDB.Net/tools/EdgeDB.TestGenerator/ValueFormatter.cs:line 183
   at System.Threading.Tasks.Parallel.<>c__DisplayClass19_0`1.<ForWorker>b__1(RangeWorker& currentWorker, Int32 timeout, Boolean& replicationDelegateYieldedBeforeCompletion)
--- End of stack trace from previous location ---
   at System.Threading.Tasks.Parallel.<>c__DisplayClass19_0`1.<ForWorker>b__1(RangeWorker& currentWorker, Int32 timeout, Boolean& replicationDelegateYieldedBeforeCompletion)
   at System.Threading.Tasks.TaskReplicator.Replica.Execute()<---

 ---> (Inner Exception #2) System.InvalidOperationException: value is not a dictionary
   at EdgeDB.TestGenerator.ValueFormatter.Format(Object edgedbValue, Object generatedValue, Action progress, IValueProvider provider, IWrappingValueProvider parent) in /Users/quinch/Documents/GitHub/EdgeDB.Net/tools/EdgeDB.TestGenerator/ValueFormatter.cs:line 48
   at EdgeDB.TestGenerator.ValueFormatter.<>c__DisplayClass0_6.<Format>b__5(Int32 i) in /Users/quinch/Documents/GitHub/EdgeDB.Net/tools/EdgeDB.TestGenerator/ValueFormatter.cs:line 183
   at System.Threading.Tasks.Parallel.<>c__DisplayClass19_0`1.<ForWorker>b__1(RangeWorker& currentWorker, Int32 timeout, Boolean& replicationDelegateYieldedBeforeCompletion)
--- End of stack trace from previous location ---
   at System.Threading.Tasks.Parallel.<>c__DisplayClass19_0`1.<ForWorker>b__1(RangeWorker& currentWorker, Int32 timeout, Boolean& replicationDelegateYieldedBeforeCompletion)
   at System.Threading.Tasks.TaskReplicator.Replica.Execute()<---

 ---> (Inner Exception #3) System.InvalidOperationException: value is not a dictionary
   at EdgeDB.TestGenerator.ValueFormatter.Format(Object edgedbValue, Object generatedValue, Action progress, IValueProvider provider, IWrappingValueProvider parent) in /Users/quinch/Documents/GitHub/EdgeDB.Net/tools/EdgeDB.TestGenerator/ValueFormatter.cs:line 48
   at EdgeDB.TestGenerator.ValueFormatter.<>c__DisplayClass0_6.<Format>b__5(Int32 i) in /Users/quinch/Documents/GitHub/EdgeDB.Net/tools/EdgeDB.TestGenerator/ValueFormatter.cs:line 183
   at System.Threading.Tasks.Parallel.<>c__DisplayClass19_0`1.<ForWorker>b__1(RangeWorker& currentWorker, Int32 timeout, Boolean& replicationDelegateYieldedBeforeCompletion)
--- End of stack trace from previous location ---
   at System.Threading.Tasks.Parallel.<>c__DisplayClass19_0`1.<ForWorker>b__1(RangeWorker& currentWorker, Int32 timeout, Boolean& replicationDelegateYieldedBeforeCompletion)
   at System.Threading.Tasks.TaskReplicator.Replica.Execute()<---


select { (GZYZMJIYJTKDNNF := <float32>45.662083n, UIXZALRELUFTEBH := <int16>20881), (DUXSRFEHFFJXPDV := <float32>9.276653n, WNMDGUQPPDLMDLT := <int16>28780), (LMFKOVNZMNGAXZT := <float32>-17.479986n, NKVRTWMPKFLRRXF := <int16>10317), (KDGSYUIKPUQMJEG := <float32>-12.971614n, VFDLCVREMAGGVED := <int16>30131) }

select {
  (
    GZYZMJIYJTKDNNF := <float32>45.662083n,
    UIXZALRELUFTEBH := <int16>20881
  ),
  (
    DUXSRFEHFFJXPDV := <float32>9.276653n,
    WNMDGUQPPDLMDLT := <int16>28780
  ),
  (
    LMFKOVNZMNGAXZT := <float32>-17.479986n,
    NKVRTWMPKFLRRXF := <int16>10317
  ),
  (
    KDGSYUIKPUQMJEG := <float32>-12.971614n,
    VFDLCVREMAGGVED := <int16>30131
  )
}


set
└── namedtuple
    ├── std::float32
    └── std::int16
                                                                                                                  