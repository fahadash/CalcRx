# CalcRx
Allows you to run runtime text expressions on IObservable&lt;T>.

Suppose a few scientists are working on a project of monitoring temperatures inside Amazon Rainforest, their temperature probe reports to satellite and all the temperature readings are received by a computer running the .NET Program that you are maintaining. You receive the temperature changes through IObservable<double> (a Publish-Subscribe model)

```csharp
IObservable<double> temperatureInFahrenheit = SomeMethodToGetTemperatureChanges();
```
You are getting temperature in Fahrenheit, but scientists like Celcius better, you have a text field in your application that allows Scientists to write a formula f(t) so they can type experessions like the following.

``` 
 ( _ - 32 ) * 5/9
```
*Underscore _ references the object yielded by input observable. In this case is the temperature values in double*

You want to apply the above expression to an existing observable, CalcRx allows you to do the following.

```csharp
IObservable<double> temperatureInCelcius = temperatureInFahrenheit.Evaluate<double, double>("( _ - 32 ) * 5/9");
```

Now suppose you have a observable that gives you back the Stock Trades

```csharp
IObservable<Trade> myTrades = SomeWebService.MyTrades();
```

Traders that are using your application want to run a certain formula (like ones in Excel) on the Trades or Ticks that are coming in live. A formula such as **SellPrice - BuyPrice**

CalcRx can access properties of objects yielded by observables. So you could do

```csharp
IObservable<double> profits = myTrades.Evaluate<Trade, double>("SellPrice - BuyPrice");
```

Now, you want to have more fun and introduce functions

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
