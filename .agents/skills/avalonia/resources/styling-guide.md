# Avalonia Styling Guide

## Style Selectors (CSS-like)

```xml
<Styles xmlns="https://github.com/avaloniaui">
    <Style Selector="Button">                              <!-- Type selector -->
        <Setter Property="Background" Value="Blue" />
    </Style>
    <Style Selector="Button.Primary">                      <!-- Class selector -->
        <Setter Property="Background" Value="Green" />
    </Style>
    <Style Selector="Button#SaveButton">                   <!-- Name selector -->
        <Setter Property="Background" Value="Red" />
    </Style>
    <Style Selector="StackPanel Button">                   <!-- Descendant -->
        <Setter Property="Margin" Value="5" />
    </Style>
    <Style Selector="StackPanel > Button">                 <!-- Child -->
        <Setter Property="Margin" Value="10" />
    </Style>
    <Style Selector="Button, TextBox">                     <!-- OR combination -->
        <Setter Property="Margin" Value="5" />
    </Style>
</Styles>
```

## Pseudo-Classes

```xml
<Style Selector="Button:pointerover"> <Setter Property="Background" Value="LightBlue" /> </Style>
<Style Selector="Button:pressed"> <Setter Property="Background" Value="DarkBlue" /> </Style>
<Style Selector="Button:disabled"> <Setter Property="Opacity" Value="0.5" /> </Style>
<Style Selector="TextBox:focus"> <Setter Property="BorderBrush" Value="Blue" /> </Style>
<Style Selector="ListBoxItem:selected"> <Setter Property="Background" Value="LightBlue" /> </Style>
<Style Selector="Button:pointerover:not(:disabled)"> <Setter Property="Background" Value="LightBlue" /> </Style>
```

## Resource Dictionaries

```xml
<Window.Resources>
    <SolidColorBrush x:Key="PrimaryBrush" Color="#007ACC" />
    <x:Double x:Key="StandardFontSize">14</x:Double>
    <Thickness x:Key="StandardMargin">10</Thickness>
</Window.Resources>

<!-- Merged dictionaries -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceInclude Source="/Styles/Colors.axaml" />
            <ResourceInclude Source="/Styles/Buttons.axaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## Control Templates

```xml
<Style Selector="Button.CustomButton">
    <Setter Property="Template">
        <ControlTemplate>
            <Border Background="{TemplateBinding Background}" CornerRadius="5" Padding="{TemplateBinding Padding}">
                <ContentPresenter Content="{TemplateBinding Content}" HorizontalAlignment="Center" VerticalAlignment="Center" />
            </Border>
        </ControlTemplate>
    </Setter>
</Style>
```

## Data Templates

```xml
<DataTemplate x:Key="PersonTemplate" x:DataType="models:Person">
    <StackPanel Orientation="Horizontal" Spacing="10">
        <Image Source="{Binding Avatar}" Width="32" Height="32" />
        <TextBlock Text="{Binding Name}" FontWeight="Bold" />
    </StackPanel>
</DataTemplate>
<ListBox ItemTemplate="{StaticResource PersonTemplate}" />
```

## Light/Dark Theme Support

```xml
<Style Selector="Window">
    <Setter Property="Background" Value="White" />
    <Setter Property="Foreground" Value="Black" />
</Style>
<Style Selector="Window[RequestedThemeVariant=Dark]">
    <Setter Property="Background" Value="#1E1E1E" />
    <Setter Property="Foreground" Value="White" />
</Style>
```

```csharp
Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
Application.Current.RequestedThemeVariant = ThemeVariant.Light;
Application.Current.RequestedThemeVariant = ThemeVariant.Default; // system
```

## Animations

```xml
<Style Selector="Button:pointerover">
    <Style.Animations>
        <Animation Duration="0:0:0.2" FillMode="Forward">
            <KeyFrame Cue="0%"><Setter Property="Background" Value="Blue" /></KeyFrame>
            <KeyFrame Cue="100%"><Setter Property="Background" Value="LightBlue" /></KeyFrame>
        </Animation>
    </Style.Animations>
</Style>
```

## Transitions

```xml
<Button>
    <Button.Transitions>
        <Transitions>
            <DoubleTransition Property="Opacity" Duration="0:0:0.3" />
            <TransformOperationsTransition Property="RenderTransform" Duration="0:0:0.3" />
        </Transitions>
    </Button.Transitions>
</Button>
```

## Best Practices

1. Use `StaticResource` over `DynamicResource` when values won't change
2. Define brushes as resources instead of inline
3. Separate styles into logical files (Colors, Buttons, etc.)
4. Prefer class selectors over direct styling for reusability
5. Test styles in both light and dark themes
