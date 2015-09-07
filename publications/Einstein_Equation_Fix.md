###Abstract
This is about a bug in CalcRx which was causing it to compute the wrong results for Einstein equations or expressions with similar properties. The resolution, and future improvement possibility are also discussed.

###The issue
There is nothing peculiar about Einstein equations in this scenario that causes this, it is just how CalcRx was coded was throwing it off. Anything with `Base` expression involved twice across one or more binary operations would fit the scenario. But in my case, I was writing tests for Einstein Equations and failed it at the beginning. To understand the issue better, consider this scenario. You have an application that is monitoring the velocities of an object moving relative to your probe, you want to apply Lorentz Transformation. The following equation is not true Lorentz Factor because of the missing square root in the denominator, we will see later why.

```
_/(1 - (_ / 299792458) ^ 2)
```

That huge number is speed of light in a vacuum in meters per seconds. Before the issue was fixed, it was generating an [Expression][expressions] like this

```csharp
__main.CombineLatest(__main, __main.Select(a0 => a0/299792458).Select(a1 => Math.Pow(a1, 2)))
```

Pretty ugly right? not only above expression looks ugly it will produce undefined results because of the way simultaneous operations are timed. To understand this better, we need to look into [CombineLatest]. CombineLatest does not care how the data is coming in, as soon as one of the observables in the combination yields an output, [CombineLatest] recomputes the expression. So if an expression has to wait for all variables involved to be updated before it could be recomputed will fail. The other option is to use [Zip], but [CombineLatest] is thrown in there for a purpose, there are cases where you need not to wait for all sides to post an update to recompute the formula. So what is the option? The answer is to wrap everything cohesive around in one [Select].

The above example would look pretty if CalcRx generated the following expression

```csharp
__main.Select(a0 => a0 / (1 - Math.Pow(a0 / 299792458)));
```

But the way expression tree was being built was, every time an `IObservable<T>` was combined in a binary operation with a number, it would do [Select] which turns the expression into IObservable<T>, then all subsequent binary operations will do Select on Select. And if you try to combine two Select-s that would try to join them with CombineLatest.


###The fix
To fix the issue, I implemented the following.

1. Everytime you run into a binary computation between a [Select]-or and a number, re-write that [Select]-or to include that select inside.
2. Everytime two observables that are results of Select-s are combined in a binary operation, discard one Select and combine the selectors of both in one Select.


###New issues
The fix helped me generate a very clean and acceptable [Expression][expressions]. I have ended my fix there, but there are still two issues left.

1. The fix is a burden on Parsing because it regenerates the whole Select by prying the existing one open, excavating the selectors out of it and combining it with the other operand into another [Select].
2. Einstein Equation will still fail

To understand 1, consider the scenario where parsing is underway for the following expression

```
_ + 3 * _
```

Now the first step would be to generate the Expression for `3 * _` which would generate

```csharp
__main.Select(a0 => 3 * a0);
```

Perfect so far right? Now the system has to combine a `_ +`, the new fix would rip the Expression, remove `3 * a0` which is the selector out, then generate new expression and a new Select call [Expression][4]. Imagine how crazy would it be if your expression involve 20 binary operations? The troubles don't end there consider the following expression

```
(_ + 1) * (_ - 1)
```

In the above case, you will end up with having to combine two __main.Select Expressions with binary multiply operation. Before the fix, it would combine them using [CombineLatest], but thanks to the fix, now it will convert both of them into one Select. But to do that, it would excavate the selectors of both, get the Parameter expression of one, and recursively walk the other selector tree to replace its own parameter expression with the one of the other side. To understand it better you are having to combine the following two

```csharp
__main.Select(a0 => a0 + 1) * __main.Select(a1 => a1 - 1)
```

Notice that the first selector is referencing a0, and second is a1. In the new Select, there would be only one parameter, either a0 or a1. In order to do that, I have done something I am not too happy about. I am walking the whole tree on the other side, regenerating every bit of it because Expression trees are non-mutable and replacing a1 with a0.


Now about 2, The Einstein Equation failure. Consider the easier equation that would fail this scenario. How about we divide the number by its square root?

```
_ / sqrt ( _ )
```

Now in order to accomplish the above, the square root function or `sqrt` has to come from either the Function Packs or has to be included in the call to `Evaluate`, when underscore (_) is an argument to a function, the current architecute expects to find a function that takes observable for that argument and will pass `__main` to that parameter. A sane implementation however should find a proper overload and do something like following

```csharp
__main.Select(a0 => a0 / sqrt( a0 ))
```

That is indeed not expecting too much. For the above I have opened the issue#3 and the fix to that will finish the Einstein Equation fix.

###Conclusion
So in a nutshell, the issue was the crazy [Expressions][expressions] being generated, I implemented a fix which although one-time but does add an overhead on the parsing and [Expression][expressions] generation part and Einstein Equation or similar would still fail because of the way function calling is implemented.

[CombineLatest]: https://msdn.microsoft.com/en-us/library/Hh211991(v=VS.103).aspx
[Zip]: https://msdn.microsoft.com/en-us/library/hh244275(v=vs.103).aspx
[Select]: https://msdn.microsoft.com/en-us/library/Hh244306(v=VS.103).aspx
[expression_call]: https://msdn.microsoft.com/en-us/library/bb349020(v=vs.110).aspx
[expressions]: https://msdn.microsoft.com/en-us/library/system.linq.expressions(v=vs.110).aspx
