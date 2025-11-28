# .NET MAUI XAML Tag Cheat Sheet

A comprehensive reference for common **.NET MAUI XAML tags**, their **functions**, **key properties**, and **usage examples**.

---

## 🧱 1. Layout Containers

| Tag | Function | Common Properties | Example |
|------|-----------|------------------|----------|
| `<StackLayout>` | Arranges child elements in a single line (vertical or horizontal). | `Orientation`, `Spacing`, `Padding`, `Margin` | ```xml
<StackLayout Orientation="Vertical" Spacing="10">
  <Label Text="Name:"/>
  <Entry Placeholder="Enter name"/>
</StackLayout>
``` |
| `<Grid>` | Defines a table-like layout with rows and columns. | `RowDefinitions`, `ColumnDefinitions`, `RowSpacing`, `ColumnSpacing` | ```xml
<Grid>
  <Grid.RowDefinitions>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="*"/>
  </Grid.RowDefinitions>
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="*"/>
    <ColumnDefinition Width="2*"/>
  </Grid.ColumnDefinitions>
  <Label Text="Name:" Grid.Row="0" Grid.Column="0"/>
  <Entry Grid.Row="0" Grid.Column="1"/>
</Grid>
``` |
| `<HorizontalStackLayout>` | Like `StackLayout`, but fixed horizontally. | `Spacing`, `Padding`, `Margin` | ```xml
<HorizontalStackLayout Spacing="5">
  <Button Text="Yes"/>
  <Button Text="No"/>
</HorizontalStackLayout>
``` |
| `<VerticalStackLayout>` | Like `StackLayout`, but vertical. | `Spacing`, `Padding`, `Margin` | ```xml
<VerticalStackLayout>
  <Label Text="Title"/>
  <Label Text="Subtitle"/>
</VerticalStackLayout>
``` |
| `<FlexLayout>` | Advanced layout similar to CSS flexbox. | `Direction`, `Wrap`, `JustifyContent`, `AlignItems` | ```xml
<FlexLayout Wrap="Wrap" JustifyContent="SpaceBetween">
  <Button Text="1"/>
  <Button Text="2"/>
  <Button Text="3"/>
</FlexLayout>
``` |
| `<AbsoluteLayout>` | Places children at absolute or relative positions. | `LayoutBounds`, `LayoutFlags` | ```xml
<AbsoluteLayout>
  <Label Text="Hello" AbsoluteLayout.LayoutBounds="0.1,0.1,100,30" AbsoluteLayout.LayoutFlags="None"/>
</AbsoluteLayout>
``` |

---

## 🎛️ 2. Basic Controls (Inputs & Buttons)

| Tag | Function | Common Properties | Example |
|------|-----------|------------------|----------|
| `<Button>` | Clickable button triggering an event. | `Text`, `Clicked`, `BackgroundColor`, `TextColor`, `CornerRadius` | ```xml
<Button Text="Submit" BackgroundColor="Blue" Clicked="OnSubmitClicked"/>
``` |
| `<Entry>` | Single-line text input. | `Text`, `Placeholder`, `IsPassword`, `Keyboard` | ```xml
<Entry Placeholder="Enter email" Keyboard="Email"/>
``` |
| `<Editor>` | Multi-line text input. | `Text`, `Placeholder`, `AutoSize`, `MaxLength` | ```xml
<Editor Placeholder="Enter description..." AutoSize="TextChanges"/>
``` |
| `<CheckBox>` | Toggleable checkbox. | `IsChecked`, `Color`, `CheckedChanged` | ```xml
<CheckBox IsChecked="True" CheckedChanged="OnCheck"/>
``` |
| `<Switch>` | On/off toggle switch. | `IsToggled`, `Toggled`, `OnColor`, `ThumbColor` | ```xml
<Switch IsToggled="False" Toggled="OnToggleChanged"/>
``` |
| `<Slider>` | Selects a numeric value by sliding. | `Minimum`, `Maximum`, `Value`, `ValueChanged` | ```xml
<Slider Minimum="0" Maximum="100" Value="50"/>
``` |
| `<Stepper>` | Increments/decrements numeric values. | `Minimum`, `Maximum`, `Increment`, `ValueChanged` | ```xml
<Stepper Minimum="0" Maximum="10" Increment="1"/>
``` |

---

## 🖼️ 3. Display Elements

| Tag | Function | Common Properties | Example |
|------|-----------|------------------|----------|
| `<Label>` | Displays non-editable text. | `Text`, `TextColor`, `FontSize`, `FontAttributes` | ```xml
<Label Text="Hello, World!" FontSize="20" FontAttributes="Bold"/>
``` |
| `<Image>` | Displays an image from file, resource, or URL. | `Source`, `Aspect`, `WidthRequest`, `HeightRequest` | ```xml
<Image Source="dotnet_bot.png" Aspect="AspectFit"/>
``` |
| `<ProgressBar>` | Shows progress visually (0–1). | `Progress`, `ProgressColor` | ```xml
<ProgressBar Progress="0.6" ProgressColor="Green"/>
``` |
| `<ActivityIndicator>` | Animated spinner for background activity. | `IsRunning`, `Color`, `IsVisible` | ```xml
<ActivityIndicator IsRunning="True" Color="Blue"/>
``` |
| `<BoxView>` | Simple colored box. | `Color`, `CornerRadius`, `WidthRequest`, `HeightRequest` | ```xml
<BoxView Color="Gray" HeightRequest="1"/>
``` |
| `<Border>` | Adds a border or rounded container. | `Stroke`, `StrokeThickness`, `StrokeShape`, `Background` | ```xml
<Border Stroke="Black" StrokeThickness="2" Background="LightGray">
  <Label Text="Bordered label"/>
</Border>
``` |

---

## 🧭 4. Navigation and Structure

| Tag | Function | Common Properties | Example |
|------|-----------|------------------|----------|
| `<ContentPage>` | Defines a single page. | `Title`, `BackgroundColor`, `Content` | ```xml
<ContentPage Title="Home">
  <VerticalStackLayout>
    <Label Text="Welcome!"/>
  </VerticalStackLayout>
</ContentPage>
``` |
| `<NavigationPage>` | Manages navigation stack. | `BarBackgroundColor`, `BarTextColor` | ```xml
<NavigationPage>
  <x:Arguments>
    <local:MainPage />
  </x:Arguments>
</NavigationPage>
``` |
| `<TabbedPage>` | Multi-tab page. | `Children`, `BarBackgroundColor`, `SelectedTabColor` | ```xml
<TabbedPage>
  <local:HomePage Title="Home"/>
  <local:SettingsPage Title="Settings"/>
</TabbedPage>
``` |
| `<Shell>` | High-level app container. | `Title`, `FlyoutBehavior`, `ContentTemplate` | ```xml
<Shell>
  <ShellContent Title="Main" ContentTemplate="{DataTemplate local:MainPage}" />
</Shell>
``` |

---

## 🧮 5. Data and Binding Controls

| Tag | Function | Common Properties | Example |
|------|-----------|------------------|----------|
| `<CollectionView>` | Displays lists of items (modern). | `ItemsSource`, `ItemTemplate`, `SelectionMode` | ```xml
<CollectionView ItemsSource="{Binding Items}">
  <CollectionView.ItemTemplate>
    <DataTemplate>
      <Label Text="{Binding Name}"/>
    </DataTemplate>
  </CollectionView.ItemTemplate>
</CollectionView>
``` |
| `<ListView>` | Legacy list of items. | `ItemsSource`, `ItemTemplate`, `HasUnevenRows` | ```xml
<ListView ItemsSource="{Binding People}">
  <ListView.ItemTemplate>
    <DataTemplate>
      <TextCell Text="{Binding Name}" Detail="{Binding Age}"/>
    </DataTemplate>
  </ListView.ItemTemplate>
</ListView>
``` |
| `<Picker>` | Drop-down selection list. | `ItemsSource`, `SelectedIndex`, `SelectedItem` | ```xml
<Picker ItemsSource="{Binding Cities}" SelectedIndex="0"/>
``` |
| `<SearchBar>` | Search input box. | `Text`, `Placeholder`, `SearchButtonPressed` | ```xml
<SearchBar Placeholder="Search..." SearchButtonPressed="OnSearch"/>
``` |

---

## 🧩 6. Miscellaneous & Advanced

| Tag | Function | Common Properties | Example |
|------|-----------|------------------|----------|
| `<ScrollView>` | Makes content scrollable. | `Orientation`, `Content` | ```xml
<ScrollView>
  <VerticalStackLayout>
    <Label Text="Lots of content..."/>
  </VerticalStackLayout>
</ScrollView>
``` |
| `<Frame>` | Bordered, shadowed container. | `CornerRadius`, `HasShadow`, `BorderColor` | ```xml
<Frame BorderColor="Gray" CornerRadius="10" HasShadow="True">
  <Label Text="Inside Frame"/>
</Frame>
``` |
| `<WebView>` | Displays web pages or HTML. | `Source`, `Navigating`, `Navigated` | ```xml
<WebView Source="https://dotnet.microsoft.com" />
``` |
| `<Map>` | Interactive map (needs permission). | `MapType`, `IsShowingUser`, `Pins` | ```xml
<maps:Map MapType="Street" IsShowingUser="True"/>
``` |

---

## 🧠 Notes

- Use `x:` namespace for XAML identifiers (e.g., `x:Name="MyButton"`).
- Binding syntax: `{Binding PropertyName}` links UI to ViewModel data.
- Common resources and styles go in `ResourceDictionary` or `App.xaml`.

---

**Author:** ChatGPT (.NET MAUI Reference 2025)  
**License:** Free to use for learning and development
