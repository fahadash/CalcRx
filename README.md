# CalcRx
Allows you to run runtime text expressions on IObservable&lt;T>

Suppose you have the following observable

```csharp
IObservable<double> temperature = SomeMethodToGetTemperatureChanges();
```
You have have a text field in your user interface that allows them to type expressions like this

``` 
 ( _ - 32 ) * 5/9
```

And you want to apply that expression to an existing observable, you can do this.

```csharp
IObservable<double> celciusTemperature = CalcRx.Evalutate<double>("( _ - 32 ) * 5/9", temperature);
```

Now suppose you have a observable that gives you back the Stock Trades

```csharp
IObservable<Trade> myTrades = SomeWebService.MyTrades();
```

Now your user interface allows users to write Profit calculator like this 'SellPrice - BuyPrice' . (SellPrice and BuyPrice are properties of Trade object).

CalcRx can access properties of objects yielded by observables. So you could do

```csharp
IObservable<double> profits = CalcRx.Evaluate<double>("SellPrice - BuyPrice", myTrades);
```

## Parser
The parser is written using Coco/R compiler generator. The ATG grammar file is included in the FormulaParser directory.

## Current status
Currently the project is under development and I am adding features as I get time. If you would like to join in, feel free to fork it and send me the pull requests. If you need any support in compiling, or adding more features drop me a line.
