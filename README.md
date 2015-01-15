# EETuring
Everybody Edits Turing Validator

Validates a path from A to B

Quick start guide:
(function to get world data can be found in Program.cs)

```csharp
Turing turingTester = new Turing(world);
turingTester.OnProgress += turingTester_OnProgress;
turingTester.OnComplete += turingTester_OnComplete;

turingTester.SearchAsync(new Point(1, 23), new Point(23, 23));
```
