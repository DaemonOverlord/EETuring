# EETuring
Everybody Edits Turing Validator

Validates a path from A to B

Quick start guide:

[Get World Data Method](https://gist.github.com/ThyChief/12d08be2dabd2e5a3d68)

```csharp
Turing turingTester = new Turing(world);
turingTester.OnProgress += turingTester_OnProgress;
turingTester.OnComplete += turingTester_OnComplete;

turingTester.SearchAsync(new Point(1, 23), new Point(23, 23));
```

Download Binaries

[Latest Version](https://www.mediafire.com/?tz86bkrgkbpyn0f)
