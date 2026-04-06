using System.Runtime.CompilerServices;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World
{
    public class Field
    {
        #region  Fields
        public FieldType Type { get; private set; } = FieldType.DEEP_LAVA_OCEAN;
        public ISurface? Surface { get; set; }
        #endregion
        
        #region Methods
        public void SetFieldTypeTo(Field f) => Type = f.Type;
        public void SetFieldTypeTo(FieldType ftype) => Type = ftype;
        public void SetFieldHeight(float height)
        {
            var i = 0;
            while (i < GameSettings.MaxFieldTypeHeights.Length &&
                   height > GameSettings.MaxFieldTypeHeights[i])
                i++;

            Type = (FieldType)i;
        }

        public bool IsBuildable() => Type > FieldType.LAVA_OCEAN && Surface is null or Mushroom;
        public bool IsLowerable() => Type > FieldType.BEACH && Surface is null or Mushroom;
        public bool IsHeightenable() => FieldType.BEACH <= Type && Type < FieldType.HIGH_MOUNTAINS && Surface is null or Mushroom;
        public int GetHeightDifference(Field? field) => Type - field?.Type ?? 0;
        #endregion
    }
}
