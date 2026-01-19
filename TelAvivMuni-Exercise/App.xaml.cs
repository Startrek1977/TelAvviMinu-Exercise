using System.Windows;
using TelAvivMuni_Exercise.Infrastructure;

namespace TelAvivMuni_Exercise;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static IUnitOfWork? _unitOfWork;

    public static IUnitOfWork UnitOfWork => _unitOfWork ??= new UnitOfWork();

    protected override void OnExit(ExitEventArgs e)
    {
        _unitOfWork?.Dispose();
        base.OnExit(e);
    }
}
