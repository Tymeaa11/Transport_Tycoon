using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.ViewModel;
public class ChunkViewModel(Chunk chunk) : ViewModelBase
{
    #region Fields
    public Chunk Chunk { get; init; } = chunk;

    private bool _isVisible = false;
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            OnPropertyChanged();
        }
    }

    public event EventHandler? Rerender;
    public void TriggerRerender() => Rerender?.Invoke(this, EventArgs.Empty);
    public void RemoveAllUpdateTriggers() { Rerender = null; }

    #endregion
}
