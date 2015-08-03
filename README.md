# CalcRx
Allows you to run runtime text expressions on IObservable&lt;T>

Suppose you have the following observable

```cs
IObservable<double> temperature = SomeMethodToGetTemperatureChanges();
```
You have have a text field in your user interface that allows them to type expressions like this

``` 
_ + 273.15
```

And you want to apply that expression to an existing observable, you can do this.

```cs
IObservable<double> kelvinTemperature = CalcRx.ChangeObservable("_ + 273.15", temperature);
```

## Parser
The parser is writteh using Coco/R compiler generator. The ATG file is included in the FormulaParser directory.

## Current status
Currently the project is under develpment and I am adding features as I get time. If you would like to join in, feel free to fork it and send me the pull requests. If you need any support in compiling, or adding more features drop me a line.
