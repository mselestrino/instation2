using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace Instation2;

public sealed class MainWindow : Window
{
    private readonly string[] _categories = ["Jogos", "Favoritos", "Recentes", "Emulador", "Sistema"];
    private readonly string[] _games = ["Biblioteca de jogos", "Adicionar pasta de jogos", "Verificar arquivos", "Configurações"];
    private readonly TextBlock _categoryText;
    private readonly TextBlock _selectionText;
    private readonly TextBlock _hintText;
    private int _category;
    private int _selection;

    public MainWindow()
    {
        Title = "Instation 2";
        WindowState = WindowState.FullScreen;
        SystemDecorations = SystemDecorations.None;
        Background = Brushes.Black;
        KeyDown += HandleKeyDown;

        var root = new Grid();
        root.Children.Add(new XmbBackground());

        var content = new Grid { Margin = new Thickness(76, 48, 76, 38) };
        content.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        content.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
        content.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        var header = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 34 };
        header.Children.Add(new TextBlock
        {
            Text = "instation",
            FontSize = 30,
            FontWeight = FontWeight.Light,
            Foreground = Brushes.White,
            Opacity = 0.92
        });
        _categoryText = new TextBlock
        {
            Text = _categories[0].ToUpperInvariant(),
            FontSize = 15,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromArgb(220, 225, 240, 255)),
            LetterSpacing = 3
        };
        header.Children.Add(_categoryText);
        content.Children.Add(header);

        var center = new StackPanel
        {
            Spacing = 18,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, -10, 0, 0)
        };
        var eyebrow = new TextBlock
        {
            Text = "SUA COLEÇÃO",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(175, 220, 235, 255)),
            LetterSpacing = 4
        };
        center.Children.Add(eyebrow);

        _selectionText = new TextBlock
        {
            Text = _games[0],
            FontSize =  thirtySix,
            FontWeight = FontWeight.Light,
            Foreground = Brushes.White
        };
        center.Children.Add(_selectionText);
        var subtitle = new TextBlock
        {
            Text = "Organize e inicie seus jogos de PlayStation 2.",
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.FromArgb(190, 225, 235, 255))
        };
        center.Children.Add(subtitle);
        Grid.SetRow(center, 1);
        content.Children.Add(center);

        _hintText = new TextBlock
        {
            Text = "← →  CATEGORIAS       ↑ ↓  NAVEGAR       ENTER  SELECIONAR       ESC  SAIR",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 225, 235, 255)),
            LetterSpacing = 1.5
        };
        Grid.SetRow(_hintText, 2);
        content.Children.Add(_hintText);

        root.Children.Add(content);
        Content = root;
    }

    private const double thirtySix = 36;

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                _category = (_category - 1 + _categories.Length) % _categories.Length;
                _selection = 0;
                UpdateView();
                break;
            case Key.Right:
                _category = (_category + 1) % _categories.Length;
                _selection = 0;
                UpdateView();
                break;
            case Key.Up:
                _selection = Math.Max(0, _selection - 1);
                UpdateView();
                break;
            case Key.Down:
                _selection = Math.Min(_games.Length - 1, _selection + 1);
                UpdateView();
                break;
            case Key.Escape:
                Close();
                break;
            case Key.Enter:
                _hintText.Text = _selection == 0
                    ? "A BIBLIOTECA SERÁ CONECTADA AO PCSX2 NA PRÓXIMA ETAPA."
                    : "ESTA FUNÇÃO SERÁ CONFIGURADA NA PRÓXIMA ETAPA.";
                break;
        }
    }

    private void UpdateView()
    {
        _categoryText.Text = _categories[_category].ToUpperInvariant();
        _selectionText.Text = _games[_selection];
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
