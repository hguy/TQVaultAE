# Reactive Programming and Animations

## ReactiveUI Setup

```xml
<PackageReference Include="ReactiveUI" Version="19.*" />
<PackageReference Include="ReactiveUI.Avalonia" Version="19.*" />
<PackageReference Include="System.Reactive" Version="5.*" />
```

## Reactive Properties

```csharp
private string _searchText;
public string SearchText
{
    get => _searchText;
    set => this.RaiseAndSetIfChanged(ref _searchText, value);
}
```

## Reactive Commands

```csharp
// Sync command with CanExecute
var canSave = this.WhenAnyValue(x => x.Name, x => x.Email,
    (name, email) => !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email));
SaveCommand = ReactiveCommand.Create(Save, canSave);

// Async command
LoadCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);
LoadCommand.ThrownExceptions.Subscribe(ex => ErrorMessage = ex.Message);

// Async command with parameter
DeleteCommand = ReactiveCommand.CreateFromTask<Item>(DeleteItemAsync, canDelete);
```

## Observable Sequences

```csharp
// Throttle (rate-limit)
this.WhenAnyValue(x => x.SearchText)
    .Throttle(TimeSpan.FromMilliseconds(300))
    .Subscribe(text => PerformSearch(text));

// Debounce (wait for pause)
this.WhenAnyValue(x => x.SearchText)
    .Debounce(TimeSpan.FromMilliseconds(500))
    .DistinctUntilChanged()
    .Subscribe(text => SearchAsync(text));

// Combine observables
this.WhenAnyValue(x => x.Price)
    .Select(price => price * 1.1m)
    .Subscribe(adjusted => AdjustedPrice = adjusted);

// Skip initial value
this.WhenAnyValue(x => x.SearchText).Skip(1).Subscribe(text => Search(text));
```

## Memory Management

```csharp
public class DisposableViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    public DisposableViewModel()
    {
        this.WhenAnyValue(x => x.PropertyA)
            .Subscribe(value => OnChanged(value))
            .DisposeWith(_disposables);
    }

    public void Dispose() => _disposables?.Dispose();
}
```

## Animations

```xml
<!-- Fade in on hover -->
<Style Selector="Button:pointerover">
    <Style.Animations>
        <Animation Duration="0:0:0.2" FillMode="Forward">
            <KeyFrame Cue="0%"><Setter Property="Opacity" Value="1" /></KeyFrame>
            <KeyFrame Cue="100%"><Setter Property="Opacity" Value="0.8" /></KeyFrame>
        </Animation>
    </Style.Animations>
</Style>

<!-- Pulse animation (infinite) -->
<Style Selector="Border.Pulse">
    <Style.Animations>
        <Animation Duration="0:0:1" IterationCount="Infinite">
            <KeyFrame Cue="0%"><Setter Property="Opacity" Value="1" /></KeyFrame>
            <KeyFrame Cue="50%"><Setter Property="Opacity" Value="0.6" /></KeyFrame>
            <KeyFrame Cue="100%"><Setter Property="Opacity" Value="1" /></KeyFrame>
        </Animation>
    </Style.Animations>
</Style>

<!-- Slide-in -->
<Style Selector="Border.SlideIn">
    <Style.Animations>
        <Animation Duration="0:0:0.5" FillMode="Forward">
            <KeyFrame Cue="0%"><Setter Property="TranslateTransform.X" Value="-300" /><Setter Property="Opacity" Value="0" /></KeyFrame>
            <KeyFrame Cue="100%"><Setter Property="TranslateTransform.X" Value="0" /><Setter Property="Opacity" Value="1" /></KeyFrame>
        </Animation>
    </Style.Animations>
</Style>
```

## Programmatic Animation

```csharp
var animation = new Animation
{
    Duration = TimeSpan.FromSeconds(0.5),
    Easing = new CubicEaseInOut(),
    Children =
    {
        new KeyFrame { Cue = new Cue(0.0), Setters = { new Setter(OpacityProperty, 0.0) } },
        new KeyFrame { Cue = new Cue(1.0), Setters = { new Setter(OpacityProperty, 1.0) } }
    }
};
await animation.RunAsync(button);
```

## Easing Functions

`LinearEasing`, `QuadraticEaseIn/Out/InOut`, `CubicEaseIn/Out/InOut`, `SineEaseIn/Out/InOut`, `BounceEaseOut`, `ElasticEaseIn/Out/InOut`, `BackEaseIn/Out/InOut`

## Search with Debounce Pattern

```csharp
this.WhenAnyValue(x => x.SearchText)
    .Where(text => !string.IsNullOrWhiteSpace(text))
    .Debounce(TimeSpan.FromMilliseconds(500))
    .DistinctUntilChanged()
    .ObserveOn(RxApp.MainThreadScheduler)
    .Subscribe(async text => await PerformSearch(text));
```

## Form Validation Pattern

```csharp
this.WhenAnyValue(x => x.Email)
    .Select(email => string.IsNullOrWhiteSpace(email) ? "Required"
        : !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ? "Invalid"
        : null)
    .Subscribe(error => EmailError = error);
```

## Best Practices

1. Throttle/debounce rapid updates to prevent performance issues
2. Use `CompositeDisposable` to clean up subscriptions
3. Subscribe to `ThrownExceptions` on all commands
4. Use `RxApp.MainThreadScheduler` for UI updates
5. Avoid nested subscriptions - use `CombineLatest` or `SelectMany`
