using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace Instation2;

public sealed class MainWindow : Window
{
    private readonly AppSettings _settings = AppSettings.Load();
    private readonly TextBlock _categoryText;
    private readonly TextBlock _countText;
    private readonly TextBlock _detailsText;
    private readonly TextBlock _statusText;
    private readonly ListBox _gameList;
    private readonly string[] _categories = ["Jogos", "Favoritos", "Recentes", "Emulador", "Sistema"];
    private IReadOnlyList<GameEntry> _games = [];
    private int _category;

    public MainWindow()
    {
        Title = "Instation 2";
        WindowState = WindowState.FullScreen;
        SystemDecorations = SystemDecorations.None;
        Background = Brushes.Black;
        KeyDown += HandleKeyDown;

        var root = new Grid();
        root.Children.Add(new XmbBackground());

        var layout = new Grid { Margin = new Thickness(72, 42, 72, 34) };
        layout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        layout.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
        layout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        var header = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 30 };
        header.Children.Add(new TextBlock
        {
            Text = "instation",
            FontSize = 31,
            FontWeight = FontWeight.Light,
            Foreground = Brushes.White,
            Opacity = 0.96
        });
        _categoryText = new TextBlock
        {
            Text = _categories[0].ToUpperInvariant(),
            FontSize = 14,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromArgb(220, 225, 240, 255))
        };
        header.Children.Add(_categoryText);
        layout.Children.Add(header);

        var body = new Grid { ColumnSpacing = 46, Margin = new Thickness(0, 42, 0, 26) };
        body.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.15, GridUnitType.Star)));
        body.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.85, GridUnitType.Star)));

        var left = new StackPanel { Spacing = 15, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
        left.Children.Add(new TextBlock
        {
            Text = "SUA COLEÇÃO",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 220, 235, 255))
        });
        _countText = new TextBlock
        {
            Text = "Nenhum jogo encontrado",
            FontSize = 27,
            FontWeight = FontWeight.Light,
            Foreground = Brushes.White
        };
        left.Children.Add(_countText);

        _gameList = new ListBox
        {
            MinHeight = 280,
            Background = new SolidColorBrush(Color.FromArgb(38, 5, 15, 42)),
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            FontSize = 17,
            Padding = new Thickness(4, 8)
        };
        _gameList.SelectionChanged += (_, _) => UpdateSelectedGame();
        left.Children.Add(_gameList);

        var actions = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
        actions.Children.Add(MakeButton("Adicionar pasta", AddFolder));
        actions.Children.Add(MakeButton("Verificar", RefreshLibrary));
        actions.Children.Add(MakeButton("Emulador", ConfigureEmulator));
        left.Children.Add(actions);
        Grid.SetColumn(left, 0);
        body.Children.Add(left);

        var right = new StackPanel
        {
            Spacing = 18,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 30, 0, 0)
        };
        right.Children.Add(new TextBlock
        {
            Text = "DETALHES DO JOGO",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 220, 235, 255))
        });
        _detailsText = new TextBlock
        {
            Text = "Selecione um jogo na biblioteca",
            FontSize = 25,
            FontWeight = FontWeight.Light,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap
        };
        right.Children.Add(_detailsText);
        right.Children.Add(new TextBlock
        {
            Text = "Selecione um título e pressione Enter ou Start para iniciar no PCSX2.",
            FontSize = 15,
            Foreground = new SolidColorBrush(Color.FromArgb(190, 225, 235, 255)),
            TextWrapping = TextWrapping.Wrap
        });
        _statusText = new TextBlock
        {
            Text = "Adicione a pasta onde estão suas imagens de jogos.",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromArgb(210, 190, 215, 255)),
            TextWrapping = TextWrapping.Wrap
        };
        right.Children.Add(_statusText);
        Grid.SetColumn(right, 1);
        body.Children.Add(right);

        Grid.SetRow(body, 1);
        layout.Children.Add(body);

        var footer = new TextBlock
        {
            Text = "← → CATEGORIAS     ↑ ↓ NAVEGAR     ENTER INICIAR     A ADICIONAR PASTA     ESC SAIR",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 225, 235, 255))
        };
        Grid.SetRow(footer, 2);
        layout.Children.Add(footer);

        root.Children.Add(layout);
        Content = root;
        RefreshLibrary();
    }

    private static Button MakeButton(string label, Action action)
    {
        var button = new Button
        {
            Content = label,
            Padding = new Thickness(16, 10),
            Background = new SolidColorBrush(Color.FromArgb(70, 180, 215, 255)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(90, 220, 240, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private async void AddFolder()
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Escolha a pasta dos jogos de PlayStation 2",
            AllowMultiple = true
        });

        var added = false;
        foreach (var folder in folders)
        {
            var path = folder.TryGetLocalPath();
            if (!string.IsNullOrWhiteSpace(path) &&
                !_settings.GameFolders.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                _settings.GameFolders.Add(path);
                added = true;
            }
        }

        if (added)
        {
            _settings.Save();
            RefreshLibrary();
            _statusText.Text = "Pasta adicionada. Biblioteca atualizada.";
        }
    }

    private void RefreshLibrary()
    {
        _games = GameLibrary.Scan(_settings.GameFolders);
        _gameList.ItemsSource = _games.Select(game => $"{game.Name}    ·    {game.SizeLabel}").ToArray();
        _countText.Text = _games.Count == 1 ? "1 jogo encontrado" : $"{_games.Count} jogos encontrados";

        if (_games.Count > 0)
            _gameList.SelectedIndex = 0;
        else
        {
            _detailsText.Text = "Sua biblioteca está vazia";
            _statusText.Text = _settings.GameFolders.Count == 0
                ? "Use Adicionar pasta para escolher onde seus jogos estão armazenados."
                : "Nenhum arquivo compatível foi encontrado nas pastas cadastradas.";
        }
    }

    private void UpdateSelectedGame()
    {
        if (_gameList.SelectedIndex < 0 || _gameList.SelectedIndex >= _games.Count)
            return;

        var game = _games[_gameList.SelectedIndex];
        _detailsText.Text = game.Name;
        _statusText.Text = $"{game.SizeLabel}\n{game.FilePath}";
    }

    private async void ConfigureEmulator()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Selecione o executável do PCSX2",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("PCSX2") { Patterns = ["pcsx2-qt.exe", "pcsx2.exe"] },
                new FilePickerFileType("Executável") { Patterns = ["*.exe"] }
            ]
        });

        var path = files.FirstOrDefault()?.TryGetLocalPath();
        if (!string.IsNullOrWhiteSpace(path))
        {
            _settings.Pcsx2Path = path;
            _settings.Save();
            _statusText.Text = $"PCSX2 configurado: {path}";
        }
    }

    private void StartSelectedGame()
    {
        if (_gameList.SelectedIndex < 0 || _gameList.SelectedIndex >= _games.Count)
        {
            _statusText.Text = "Selecione um jogo primeiro.";
            return;
        }

        var emulator = FindEmulator();
        if (emulator is null)
        {
            _statusText.Text = "PCSX2 não encontrado. Use o botão Emulador para localizar pcsx2-qt.exe.";
            return;
        }

        var game = _games[_gameList.SelectedIndex];
        try
        {
            var start = new ProcessStartInfo
            {
                FileName = emulator,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(emulator)!
            };
            start.ArgumentList.Add(game.FilePath);
            Process.Start(start);
            _statusText.Text = $"Iniciando {game.Name} no PCSX2...";
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            _statusText.Text = $"Não foi possível iniciar o jogo: {ex.Message}";
        }
    }

    private string? FindEmulator()
    {
        if (!string.IsNullOrWhiteSpace(_settings.Pcsx2Path) && File.Exists(_settings.Pcsx2Path))
            return _settings.Pcsx2Path;

        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PCSX2", "pcsx2-qt.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "PCSX2", "pcsx2-qt.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "PCSX2", "pcsx2-qt.exe")
        };

        var found = candidates.FirstOrDefault(File.Exists);
        if (found is not null)
        {
            _settings.Pcsx2Path = found;
            _settings.Save();
        }
        return found;
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                _category = (_category - 1 + _categories.Length) % _categories.Length;
                _categoryText.Text = _categories[_category].ToUpperInvariant();
                break;
            case Key.Right:
                _category = (_category + 1) % _categories.Length;
                _categoryText.Text = _categories[_category].ToUpperInvariant();
                break;
            case Key.Up:
                _gameList.SelectedIndex = Math.Max(0, _gameList.SelectedIndex - 1);
                break;
            case Key.Down:
                _gameList.SelectedIndex = Math.Min(_games.Count - 1, _gameList.SelectedIndex + 1);
                break;
            case Key.Enter:
                StartSelectedGame();
                break;
            case Key.A:
                AddFolder();
                break;
            case Key.F5:
                RefreshLibrary();
                break;
            case Key.Escape:
                Close();
                break;
        }
    }
}

public sealed class XmbBackground : Control
{
    private double _phase;
    private readonly DispatcherTimer _timer;

    public XmbBackground()
    {
        IsHitTestVisible = false;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(40) };
        _timer.Tick += (_, _) => { _phase += 0.008; InvalidateVisual(); };
        _timer.Start();
        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        var gradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.Parse("#071A46"), 0),
                new GradientStop(Color.Parse("#123F83"), 0.48),
                new GradientStop(Color.Parse("#24145B"), 1)
            ]
        };
        context.FillRectangle(gradient, bounds);

        var width = bounds.Width;
        var height = bounds.Height;
        for (var i = 0; i < 5; i++)
        {
            var y = height * (0.30 + i * 0.105) + Math.Sin(_phase + i * 0.8) * 26;
            var x = width * (0.55 + i * 0.075) + Math.Cos(_phase * 0.7 + i) * 55;
            var size = 250 + i * 90;
            var glow = new RadialGradientBrush
            {
                GradientStops =
                [
                    new GradientStop(Color.FromArgb(30, 120, 190, 255), 0),
                    new GradientStop(Color.FromArgb(0, 100, 160, 255), 1)
                ]
            };
            context.DrawEllipse(glow, null, new Point(x, y), size, size * 0.42);
        }

        var wavePen = new Pen(new SolidColorBrush(Color.FromArgb(24, 210, 235, 255)), 1.2);
        for (var i = 0; i < 7; i++)
        {
            var geometry = new StreamGeometry();
            using (var g = geometry.Open())
            {
                var offset = i * 34.0;
                g.BeginFigure(new Point(-40, height * 0.72 + offset), false);
                g.CubicBezierTo(
                    new Point(width * 0.28, height * 0.52 + offset + Math.Sin(_phase + i) * 18),
                    new Point(width * 0.62, height * 0.92 + offset - Math.Cos(_phase + i) * 24),
                    new Point(width + 40, height * 0.64 + offset));
                g.EndFigure(false);
            }
            context.DrawGeometry(null, wavePen, geometry);
        }
    }
}