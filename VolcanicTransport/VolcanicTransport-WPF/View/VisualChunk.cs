using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.View
{
    public class VisualChunk : FrameworkElement
    {
        private readonly DrawingVisual _visual;

        public VisualChunk()
        {
            _visual = new DrawingVisual();
            AddVisualChild(_visual);
            //CacheMode = new BitmapCache();

            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            DataContextChanged += (s, e) =>
            {
                if (DataContext is Chunk chunkData)
                {
                    PreRender(chunkData);
                }
            };
        }

        public void PreRender(Chunk chunkData)
        {
            int size = Chunk.ChunkSize * Field.FieldSize;

            RenderTargetBitmap bakedMap = new(size, size, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual dv = new();
            using (DrawingContext dc = dv.RenderOpen())
            {
                chunkData.FieldMatrix.ReadEach((x, y, f) =>
                {
                    // Draw the tile based on FieldType
                    Brush brush = FieldBrushProvider.GetBrush(f.Type);
                    double fieldX = x * Field.FieldSize;
                    double fieldY = y * Field.FieldSize;

                    dc.DrawRectangle(brush, null, new Rect(fieldX, fieldY, Field.FieldSize, Field.FieldSize));

                    // Render Surface (Roads, Bridges, Mushrooms)
                    if (f.Surface != null)
                    {
                        ImageSource image = TextureAtlas.RoadTextures[RoadType.INVALID].ImageSource;
                        int rotation = 0;


                        if (f.Surface is Mushroom m)
                        {
                            image = TextureAtlas.MushroomTextures[(int)m.GrowthStage];
                        }
                        else if (f.Surface is Road r)
                        {
                            var data = TextureAtlas.RoadTextures[r.RoadType];

                            image = data.ImageSource;
                            rotation = data.AngleDegrees;
                        }

                        double centerX = fieldX + Field.FieldSize * 0.5;
                        double centerY = fieldY + Field.FieldSize * 0.5;

                        dc.PushTransform(new RotateTransform(rotation, centerX, centerY));

                        dc.DrawImage(
                                image,
                                new Rect(fieldX, fieldY, Field.FieldSize, Field.FieldSize));

                        dc.Pop();
                    }
                });
            }

            bakedMap.Render(dv);
            bakedMap.Freeze();

            using DrawingContext dc2 = _visual.RenderOpen();
            dc2.DrawImage(bakedMap, new Rect(0, 0, size, size));

        }

        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index) => _visual;
    }
}
