using System.Windows.Input;

namespace VolcanicTransport_WPF.ViewModel
{
    /// <summary>
    /// Általános parancs típusa.
    /// </summary>
    /// <remarks>
    /// Parancs létrehozása.
    /// </remarks>
    /// <param name="canExecute">Végrehajthatóság feltétele.</param>
    /// <param name="execute">Végrehajtandó tevékenység.</param>
    public class DelegateCommand(Predicate<Object?>? canExecute, Action<Object?> execute) : ICommand
    {
        private readonly Action<Object?> _execute = execute ?? throw new ArgumentNullException(nameof(execute)); // a tevékenységet végrehajtó lambda-kifejezés
        private readonly Predicate<Object?>? _canExecute = canExecute; // a tevékenység feltételét ellenőző lambda-kifejezés

        /// <summary>
        /// Végrehajthatóság változásának eseménye.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Parancs létrehozása.
        /// </summary>
        /// <param name="execute">Végrehajtandó tevékenység.</param>
        public DelegateCommand(Action<Object?> execute) : this(null, execute) { }

        /// <summary>
        /// Végrehajthatóság ellenőrzése
        /// </summary>
        /// <param name="parameter">A tevékenység paramétere.</param>
        /// <returns>Igaz, ha a tevékenység végrehajtható.</returns>
        public Boolean CanExecute(Object? parameter) => _canExecute == null || _canExecute(parameter);

        /// <summary>
        /// Tevékenység végrehajtása.
        /// </summary>
        /// <param name="parameter">A tevékenység paramétere.</param>
        public void Execute(Object? parameter) => _execute(parameter);
    }
}
