using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VolcanicTransport.Model;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport_WPF.ViewModel;

namespace VolcanicTransport_WPF.View
{
    public class MiniVisualChunk : FrameworkElement
    {
        private const int Dpi = 24; // contains magic

        #region Fields
        private static readonly Rect MiniChunkBoundries = new(0, 0, GameSettings.MiniChunkSizeInPixels, GameSettings.MiniChunkSizeInPixels);
        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index) => _visual;

        private readonly DrawingVisual _visual;
        #endregion

        #region Constructor
        public MiniVisualChunk()
        {
            _visual = new DrawingVisual();
            AddVisualChild(_visual);

            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            DataContextChanged += (s, e) =>
            {
                if (e.OldValue is ChunkViewModel oldCvm)
                {
                    oldCvm.Rerender -= OnChunkDataChanged;
                }

                if (e.NewValue is ChunkViewModel cvm)
                {
                    cvm.Rerender += OnChunkDataChanged;
                    PreRender(cvm.Chunk);
                }
            };
        }
        #endregion

        #region Events
        private void OnChunkDataChanged(object? sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
                ExecuteRerender();
            else
                Dispatcher.BeginInvoke(ExecuteRerender);
        }

        private void ExecuteRerender()
        {
            if (DataContext is ChunkViewModel cvm)
            {
                PreRender(cvm.Chunk);
            }
        }

        #endregion

        #region Methods

        private void PreRender(Chunk chunkData)
        {

            System.Diagnostics.Debug.WriteLine($"Generating minimap prerender for {chunkData.Coordinate}");


            RenderTargetBitmap bakedMap = new(
                GameSettings.MiniChunkSizeInPixels, GameSettings.MiniChunkSizeInPixels, Dpi, Dpi, PixelFormats.Pbgra32
            );

            DrawingVisual dv = new();
            using (DrawingContext dc = dv.RenderOpen())
            {
                chunkData.FieldMatrix.ReadEach((x, y, f) =>
                {
                    // Draw the tile based on FieldType / Surface
                    Brush brush = f.Surface != null ? SurfaceBrushProvider.GetBrush(f.Surface.GetType()) : FieldBrushProvider.GetBrush(f.Type);
                   
                    double fieldX = x * GameSettings.MiniFieldSize;
                    double fieldY = y * GameSettings.MiniFieldSize;

                    Rect rectangle = new(fieldX, fieldY, GameSettings.MiniFieldSize, GameSettings.MiniFieldSize);

                    dc.DrawRectangle(brush, null, rectangle);

                });
            }

            bakedMap.Render(dv);
            bakedMap.Freeze();

            // Draw the baked chunk to _visual
            using DrawingContext dc2 = _visual.RenderOpen();
            dc2.DrawImage(bakedMap, MiniChunkBoundries);
        }

    }
    #endregion
}
