# Avalonia Controls Reference

## Layout Controls

```xml
<Grid ColumnDefinitions="100,*,Auto" RowDefinitions="Auto,*,50">
    <TextBlock Grid.Row="0" Grid.Column="0" Text="Top-Left" />
</Grid>

<StackPanel Orientation="Vertical" Spacing="10">
    <Button Content="Button 1" /> <Button Content="Button 2" />
</StackPanel>

<DockPanel LastChildFill="True">
    <Menu DockPanel.Dock="Top" /> <StatusBar DockPanel.Dock="Bottom" />
</DockPanel>

<WrapPanel Orientation="Horizontal" ItemWidth="100" />
<UniformGrid Columns="3" Rows="2" />
<ScrollViewer HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Auto" />
<Border BorderBrush="Gray" BorderThickness="1" CornerRadius="5" Padding="10" Background="White" />
<Viewbox Stretch="Uniform" />
```

## Input Controls

```xml
<TextBox Text="{Binding Name}" Watermark="Enter name" />
<TextBox Text="{Binding Description}" AcceptsReturn="True" TextWrapping="Wrap" Height="100" />
<Button Content="Click Me" Command="{Binding ClickCommand}" />
<CheckBox IsChecked="{Binding IsEnabled}" Content="Enable" />
<RadioButton GroupName="Size" IsChecked="{Binding IsSmall}" Content="Small" />
<ComboBox ItemsSource="{Binding Countries}" SelectedItem="{Binding SelectedCountry}" />
<Slider Value="{Binding Volume}" Minimum="0" Maximum="100" />
<NumericUpDown Value="{Binding Age}" Minimum="0" Maximum="120" />
<DatePicker SelectedDate="{Binding BirthDate}" />
<TimePicker SelectedTime="{Binding AppointmentTime}" />
<CalendarDatePicker SelectedDate="{Binding EventDate}" />
<ToggleSwitch IsChecked="{Binding IsEnabled}" OnContent="On" OffContent="Off" />
<AutoCompleteBox ItemsSource="{Binding Suggestions}" Text="{Binding SearchText}" FilterMode="Contains" />
<MaskedTextBox Mask="(000) 000-0000" />
```

## Display Controls

```xml
<TextBlock Text="{Binding Title}" FontSize="24" FontWeight="Bold" TextWrapping="Wrap" />
<Image Source="/Assets/logo.png" Width="200" Stretch="Uniform" />
<ProgressBar Value="{Binding Progress}" Minimum="0" Maximum="100" />
<ProgressBar IsIndeterminate="True" />
```

## Collection Controls

```xml
<ListBox ItemsSource="{Binding Items}" SelectedItem="{Binding SelectedItem}" SelectionMode="Multiple">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}" />
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>

<TreeView ItemsSource="{Binding RootNodes}">
    <TreeView.ItemTemplate>
        <TreeDataTemplate ItemsSource="{Binding Children}">
            <TextBlock Text="{Binding Name}" />
        </TreeDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>

<DataGrid ItemsSource="{Binding Users}" AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Name" Binding="{Binding Name}" />
        <DataGridCheckBoxColumn Header="Active" Binding="{Binding IsActive}" />
    </DataGrid.Columns>
</DataGrid>

<ItemsControl ItemsSource="{Binding Tags}" />
<Carousel ItemsSource="{Binding Images}" SelectedIndex="{Binding CurrentIndex}" />
```

## Menu & Navigation

```xml
<Menu>
    <MenuItem Header="File">
        <MenuItem Header="New" Command="{Binding NewCommand}" InputGesture="Ctrl+N" />
        <MenuItem Header="Exit" Command="{Binding ExitCommand}" />
    </MenuItem>
</Menu>

<TabControl>
    <TabItem Header="Home"><views:HomeView /></TabItem>
    <TabItem Header="Settings"><views:SettingsView /></TabItem>
</TabControl>

<Expander Header="Advanced Options" IsExpanded="False" />
<SplitView IsPaneOpen="{Binding IsPaneOpen}" DisplayMode="CompactInline" OpenPaneLength="250" />
```

## Dialogs & Popups

```xml
<ToolTip.Tip><TextBlock Text="Tooltip" /></ToolTip.Tip>
<Button.Flyout><Flyout><TextBlock Text="Flyout" /></Flyout></Button.Flyout>
```

## Shapes

```xml
<Rectangle Width="100" Height="50" Fill="Blue" Stroke="Black" StrokeThickness="2" />
<Ellipse Width="100" Height="100" Fill="Red" />
<Line StartPoint="0,0" EndPoint="100,100" Stroke="Black" StrokeThickness="2" />
<Polygon Points="50,0 100,50 75,100 25,100 0,50" Fill="Orange" />
<Path Fill="Purple" Data="M 10,50 L 50,10 A 40,40 0 0,1 90,50 L 50,90" />
```
