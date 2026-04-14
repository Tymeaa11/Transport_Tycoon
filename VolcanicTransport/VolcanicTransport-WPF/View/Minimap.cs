using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VolcanicTransport.Model;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.View
{
    public class Minimap : FrameworkElement
    {

        private static readonly int Dpi = 16;
        private static readonly int FieldSize = GameSettings.FieldSize / 4;
        private static readonly int HalfFieldSize = FieldSize / 2;
        private static readonly int ChunkSizeInFields = GameSettings.ChunkSize * FieldSize;

        private readonly DrawingVisual _visual;

        public Minimap()
        {
            _visual = new DrawingVisual();
            AddVisualChild(_visual);

            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            DataContextChanged += (s, e) =>
            {
                if (e.OldValue is Chunk oldChunk)
                {
                    oldChunk.Changed -= OnChunkDataChanged;
                }

                if (e.NewValue is Chunk newChunk)
                {
                    newChunk.Changed += OnChunkDataChanged;
                    Dispatcher.InvokeAsync(() => PreRender(newChunk));
                }
            };
        }

        private void OnChunkDataChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (DataContext is Chunk chunkData)
                {

                    PreRender(chunkData);
                }
            });
        }

        public void PreRender()
        {

            RenderTargetBitmap bakedMap = new(
                ChunkSizeInFields, ChunkSizeInFields, Dpi, Dpi, PixelFormats.Pbgra32
            );

            DrawingVisual dv = new();
            using (DrawingContext dc = dv.RenderOpen())
            {
                chunkData.FieldMatrix.ReadEach((x, y, f) =>
                {
                    // Draw the tile based on FieldType
                    Brush brush = FieldBrushProvider.GetBrush(f.Type);
                    double fieldX = x * FieldSize;
                    double fieldY = y * FieldSize;

                    Rect rectangle = new(fieldX, fieldY, FieldSize, FieldSize);

                    dc.DrawRectangle(brush, null, rectangle);
                });
            }

            bakedMap.Render(dv);
            bakedMap.Freeze();

            // Draw the baked chunk to _visual
            using DrawingContext dc2 = _visual.RenderOpen();
           // dc2.DrawImage(bakedMap, );
        }

        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index) => _visual;
    }
}
