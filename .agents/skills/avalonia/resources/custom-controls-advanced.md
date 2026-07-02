# Custom Controls and Advanced Techniques

## Custom TemplatedControl

```csharp
public class RatingControl : TemplatedControl
{
    public static readonly StyledProperty<int> RatingProperty =
        AvaloniaProperty.Register<RatingControl, int>(nameof(Rating), defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public int Rating
    {
        get => GetValue(RatingProperty);
        set => SetValue(RatingProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var grid = e.NameScope.Get<ItemsControl>("PART_ItemsControl");
    }
}
```

```xml
<Style Selector="local|RatingControl">
    <Setter Property="Template">
        <ControlTemplate>
            <StackPanel Orientation="Horizontal" Spacing="2">
                <ItemsControl x:Name="PART_ItemsControl" />
            </StackPanel>
        </ControlTemplate>
    </Setter>
</Style>
```

## User Control (Composite)

```csharp
public partial class UserCard : UserControl
{
    public static readonly StyledProperty<string> NameProperty =
        AvaloniaProperty.Register<UserCard, string>(nameof(Name));
    public string Name { get => GetValue(NameProperty); set => SetValue(NameProperty, value); }

    public UserCard() => InitializeComponent();
}
```

## Performance: Compiled Bindings

```xml
<Window x:DataType="vm:MainViewModel">
    <TextBlock Text="{Binding Name}" />                          <!-- Fast: compiled -->
    <TextBlock Text="{ReflectionBinding Name}" />                <!-- Slow: reflection -->
    <TextBlock Text="{Binding Name, Mode=OneTime}" />            <!-- Fastest -->
</Window>
```

## Performance: Virtualization

```xml
<ListBox ItemsSource="{Binding LargeCollection}" VirtualizationMode="Simple" />
<DataGrid ItemsSource="{Binding LargeDataSet}" VirtualizingPanel.ScrollUnit="Item" />
```

## Performance: Lazy Loading

```csharp
public void LoadNextPage()
{
    Task.Run(async () =>
    {
        var newItems = await _dataService.GetItemsAsync(page * pageSize, pageSize);
        foreach (var item in newItems) Items.Add(item);
        page++;
    });
}
```

## Render Transforms

```xml
<Border RenderTransform="translate(10 20)" />
<Border RenderTransform="scale(1.5 0.8)" />
<Border RenderTransform="rotate(45)" />
<Border RenderTransform="skew(10 20)" />
<Border RenderTransform="translate(10 20) rotate(45) scale(1.2)" />
```

```csharp
element.RenderTransform = new TranslateTransform { X = 10, Y = 20 };
element.RenderTransform = new RotateTransform { Angle = 45, CenterX = 50, CenterY = 50 };
```

## Drawing (Code-Behind)

```csharp
public override void Render(DrawingContext context)
{
    var rect = new Rect(10, 10, 100, 50);
    context.DrawRectangle(new SolidColorBrush(Colors.Blue), null, rect);
    var pen = new Pen(new SolidColorBrush(Colors.Black), 2);
    context.DrawLine(pen, new Point(0, 0), new Point(100, 100));
}
```

## Testing Custom Controls

```csharp
[AvaloniaFact]
public void Rating_CanBeSet()
{
    var control = new RatingControl { Rating = 3 };
    Assert.Equal(3, control.Rating);
}
```

## Best Practices

1. Use `TemplatedControl` for complex controls, `UserControl` for composition
2. Use attached properties for control customization
3. Virtualize large collections
4. Use compiled bindings (`x:DataType`) for performance
5. Use `Avalonia.Headless` for UI testing
