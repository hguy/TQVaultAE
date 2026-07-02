# MVVM Architecture and Data Binding

Core patterns for implementing Model-View-ViewModel architecture and data bindings in Avalonia applications.

## ViewModel Base Class (ReactiveUI)

```csharp
using ReactiveUI;
using System.Reactive;
using System.Collections.ObjectModel;

public class MainViewModel : ReactiveObject
{
    private string _name;
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private ObservableCollection<User> _users;
    public ObservableCollection<User> Users
    {
        get => _users;
        set => this.RaiseAndSetIfChanged(ref _users, value);
    }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }

    public MainViewModel()
    {
        SaveCommand = ReactiveCommand.Create(Save);
        LoadCommand = ReactiveCommand.Create(Load);
        Users = new ObservableCollection<User>();
    }
}
```

## INotifyPropertyChanged (without ReactiveUI)

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class MainViewModel : INotifyPropertyChanged
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
        }
    }

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

## Data Binding Modes

```xml
<TextBlock Text="{Binding Name}" />                                          <!-- OneWay (default for TextBlock) -->
<TextBox Text="{Binding Name, Mode=TwoWay}" />                              <!-- TwoWay (default for TextBox) -->
<TextBlock Text="{Binding Name, Mode=OneTime}" />                           <!-- OneTime -->
<Slider Value="{Binding Volume, Mode=OneWayToSource}" />                    <!-- OneWayToSource -->
```

## Binding Paths

```xml
<TextBlock Text="{Binding Name}" />
<TextBlock Text="{Binding User.Name}" />
<TextBlock Text="{Binding Items[0].Name}" />
<TextBlock Text="{Binding Path=DataContext.Title, RelativeSource={RelativeSource AncestorType=Window}}" />
<Button Content="{Binding Path=(Button.Content), RelativeSource={RelativeSource Self}}" />
```

## Commands

```xml
<Button Content="Save" Command="{Binding SaveCommand}" />
<Button Content="Delete" Command="{Binding DeleteCommand}" CommandParameter="{Binding SelectedItem}" />
```

## Multi-Binding

```xml
<TextBlock>
    <TextBlock.Text>
        <MultiBinding StringFormat="{}{0} - {1}">
            <Binding Path="FirstName" />
            <Binding Path="LastName" />
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

## Value Converters

```csharp
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
            return visibility == Visibility.Visible;
        return false;
    }
}
```

```xml
<Window.Resources>
    <converters:BoolToVisibilityConverter x:Key="BoolToVisibility" />
</Window.Resources>
<TextBlock Text="{Binding Status, Converter={StaticResource BoolToVisibility}}" />
```

## Dependency Injection

```csharp
public override void OnFrameworkInitializationCompleted()
{
    var services = new ServiceCollection();
    services.AddSingleton<IDataService, DataService>();
    services.AddTransient<MainViewModel>();
    services.AddSingleton<MainWindow>();
    var provider = services.BuildServiceProvider();

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        desktop.MainWindow = provider.GetRequiredService<MainWindow>();

    base.OnFrameworkInitializationCompleted();
}
```

## Collections

```xml
<ListBox ItemsSource="{Binding Users}" SelectedItem="{Binding SelectedUser, Mode=TwoWay}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal" Spacing="10">
                <Image Source="{Binding Avatar}" Width="32" Height="32" />
                <TextBlock Text="{Binding Name}" FontWeight="Bold" />
            </StackPanel>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

## Design-Time Data

```xml
<Window xmlns:vm="using:MyApp.ViewModels">
    <Design.DataContext>
        <vm:MainViewModel />
    </Design.DataContext>
</Window>
```

```csharp
public MainViewModel()
{
    if (Design.IsDesignMode)
    {
        Title = "Sample Title";
        Users = new ObservableCollection<User> { new User { Name = "John Doe" } };
    }
}
```

## Master-Detail Pattern

```xml
<Grid ColumnDefinitions="200,*">
    <ListBox Grid.Column="0" ItemsSource="{Binding Items}" SelectedItem="{Binding SelectedItem, Mode=TwoWay}" />
    <ContentControl Grid.Column="1" Content="{Binding SelectedItem}">
        <ContentControl.ContentTemplate>
            <DataTemplate>
                <TextBlock Text="{Binding Name}" FontSize="20" FontWeight="Bold" />
            </DataTemplate>
        </ContentControl.ContentTemplate>
    </ContentControl>
</Grid>
```
