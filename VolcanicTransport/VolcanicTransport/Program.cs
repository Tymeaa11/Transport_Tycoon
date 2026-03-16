using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VolcanicTransport.Model;
using VolcanicTransport.Model.TerrainGeneration;
using VolcanicTransport.Model.World;

namespace VolcanicTransport;

class Program
{
    static void Main(string[] args)
    {
        World.Initialise(40);
        World.Instance.gameWorldGenerator = new GameWorldGenerator(new TerrainHeightGenerator());
        World.Instance.Generate();
        
        var size = World.Instance.SizeInFields * Field.FieldSize;
        using var image = new Image<Rgba32>(size.X, size.Y, Color.DarkSlateGray);
        image.Mutate(ctx => 
        {
            World.Instance.ChunkMatrix.ReadEach((cx, cy, chunk) =>
            {
                chunk.FieldMatrix.ReadEach((fx, fy, field) =>
                {
                    Color c = field.Type switch
                    {
                        FieldType.DEEP_LAVA_OCEAN =>      Color.FromRgb(147, 0, 0),
                        FieldType.LAVA_OCEAN =>           Color.FromRgb(236, 62, 62),  
                        FieldType.BEACH =>                Color.FromRgb(69, 40, 40),
                        FieldType.LOW_LANDS =>            Color.FromRgb(120, 99, 99),
                        FieldType.LOW_MID_TRANSITION =>   Color.FromRgb(120, 137, 115),
                        FieldType.MID_LANDS =>            Color.FromRgb(166, 160, 160),
                        FieldType.MID_HIGH_TRANSITION =>  Color.FromRgb(107, 97, 19),
                        FieldType.HIGH_LANDS =>           Color.FromRgb(71, 73, 14),
                        FieldType.MOUNTAINS =>            Color.FromRgb(32, 47, 40),    // Dark Stone
                        FieldType.HIGH_MOUNTAINS =>       Color.FromRgb(255, 255, 255), // Ash/Snow Peak
                        _ => Color.Magenta
                    };

                    ctx.Fill(c, new Rectangle(
                        cx * Chunk.ChunkSize * Field.FieldSize + fx * Field.FieldSize,
                        cy * Chunk.ChunkSize * Field.FieldSize + fy * Field.FieldSize,
                        Field.FieldSize, Field.FieldSize
                    ));
                });
            });
            // Draw a filled rectangle (Brush)
            //ctx.Fill(Color.Orange, new RectangleF(50, 50, 200, 150));

            // Draw an outlined rectangle (Pen)
            // Note: You specify the thickness in the Pen constructor
            //ctx.Draw(Color.Cyan, 5f, new RectangleF(300, 100, 200, 200));

            // Draw a semi-transparent rectangle
            //ctx.Fill(Color.HotPink.WithAlpha(0.5f), new RectangleF(150, 150, 250, 100));
        });

        image.Save("output.png");
    }
}