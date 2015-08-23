# CalcRx
Allows you to run runtime text expressions on IObservable&lt;T>

Suppose you have the following observable

```csharp
IObservable<double> temperatureInFahrenheit = SomeMethodToGetTemperatureChanges();
```
You have have a text field in your user interface that allows them to type expressions like this

``` 
 ( _ - 32 ) * 5/9
```

Underscore _ references the object yielded by input observable.

And you want to apply that expression to an existing observable, you can do this.

```csharp
IObservable<double> temperatureInCelcius = temperatureInFahrenheit.Evalutate<double, double>("( _ - 32 ) * 5/9");
```

Now suppose you have a observable that gives you back the Stock Trades

```csharp
IObservable<Trade> myTrades = SomeWebService.MyTrades();
```

Now your user interface allows users to write Profit calculator like this 'SellPrice - BuyPrice' . (SellPrice and BuyPrice are properties of Trade object).

CalcRx can access properties of objects yielded by observables. So you could do

```csharp
IObservable<double> profits = myTrades.Evaluate<Trade, double>("SellPrice - BuyPrice");
```

Now, you want to have more fun and introduction functions

```csharp
IObservable<int> numbers = Observable.Range(0, 10);
IObservable<double> squareRoots = numbers.Evaluate<int, double>("SQRT(_)",
							new [] {new Function("SQRT", new Func<IObservable<int>, IObservable<double>>(
										numberObservable => numberObservable.Select(Math.Sqrt)))});
```

CalcRx has built-in Function Packs so you don't have to write some basic ones.

```csharp
IObservable<int> series = Observable.Range(0, 10);
IObservable<double> runningSum = numbers.Evaluate<int, double>("SUM(_)", FunctionPacks.BasicAggregationFunctionPack);
```

Similarly you can use a combination of function packs

```csharp
IObservable<Trade> = NYSEService.GetTrades();
IObservable<double> last5MovingAvg = numbers.Evaluate<Trade, double>("SUM(REF(_, 5)) / 5", 
									FunctionPacks.BasicAggregationFunctionPack
									.Concat(FunctionPacks.HistoryFunctionPack));
```

### Planned improvements

The next few releases will address these issues

* Better syntax and runtime error handling and reporting
* Support for multi-line expressions
* Support for building function packs out of expression (thats gotta be easy)

## Parser
The parser is written using Coco/R compiler generator. The ATG grammar file is included in the FormulaParser directory.

## Current status
Currently the project is under development and I am adding features as I get time. If you would like to join in, feel free to fork it and send me the pull requests. If you need any support in compiling, or adding more features drop me a line.
