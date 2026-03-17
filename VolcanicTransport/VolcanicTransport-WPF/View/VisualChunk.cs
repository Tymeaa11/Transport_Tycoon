using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.View
{
    public class VisualChunk : FrameworkElement
    {
        private DrawingVisual _visual;

        public VisualChunk()
        {
            _visual = new DrawingVisual();
            AddVisualChild(_visual);
            //CacheMode = new BitmapCache();

            DataContextChanged += (s, e) =>
            {
                if (DataContext is Chunk chunkData)
                {
                    PreRender(chunkData);
                }
            };
        }

        // Call this when the underlying World.Chunk data changes
        public void PreRender(Chunk chunkData)
        {
            using (DrawingContext dc = _visual.RenderOpen())
            {
                chunkData.FieldMatrix.ReadEach((x, y, f) =>
                {
                    // Draw the tile based on FieldType
                    Brush brush = FieldBrushProvider.GetBrush(f.Type);
                    dc.DrawRectangle(brush, null, new Rect(x * Field.FieldSize, y * Field.FieldSize, Field.FieldSize, Field.FieldSize));

                    // Render Surface (Roads, Bridges, Mushrooms)
                    if (f.Surface != null)
                    {
                        //RenderSurface(dc, field.Surface, x, y);
                    }
                });
            }
        }

        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index) => _visual;
    }
}
