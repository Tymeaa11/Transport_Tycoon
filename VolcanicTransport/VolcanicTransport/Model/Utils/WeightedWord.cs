namespace VolcanicTransport.Model.Utils
{
    public readonly struct WeightedWord(string word, float weight)
    {
        public string Word { get;} = word;
        public float Weight { get;} = weight;
        
        public override string ToString()
        => $"{Word}({Weight})";
    }
}
