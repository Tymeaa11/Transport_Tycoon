namespace VolcanicTransport.Model.Utils
{
    public class NameSet
    {
        public HashSet<string> Usable { get; set; }
        public HashSet<string> AllNames { get; init; }
        public int LoopNumber { get; set; }

        public NameSet(HashSet<string> names)
        {
            AllNames = names;
            Usable = [.. AllNames];
            LoopNumber = 1;
        }
    }
}
