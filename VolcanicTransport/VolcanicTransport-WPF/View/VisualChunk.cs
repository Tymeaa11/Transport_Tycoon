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
                        ImageSource? image = TextureAtlas.RoadInvalidTexture;

                        if (f.Surface is Mushroom m)
                        {
                            switch (m.GrowthStage)
                            {
                                case MushroomGrowthStage.SPROUT:
                                    image = TextureAtlas.MushroomTexture1;
                                    break;
                                case MushroomGrowthStage.JUVENILE:
                                    image = TextureAtlas.MushroomTexture2;
                                    break;
                                case MushroomGrowthStage.ADULT:
                                    image = TextureAtlas.MushroomTexture3;
                                    break;
                                case MushroomGrowthStage.FULLY_GROWN:
                                    image = TextureAtlas.MushroomTexture4;
                                    break;
                            }
                        }
                        
                        dc.DrawImage(
                            image,
                            new Rect(fieldX, fieldY, Field.FieldSize, Field.FieldSize));
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
