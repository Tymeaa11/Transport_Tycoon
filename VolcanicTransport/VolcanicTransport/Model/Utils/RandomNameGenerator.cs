using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.Utils
{
    
    public class RandomNameGenerator
    {
        private static readonly Dictionary<Type, NameSet> names = new();

        public static void Reset()
        {
            names[typeof(CondensatorFactory)] = new NameSet(["Csepeli Vízmûvek", "Vííííííííz Zrt.", "Slötyi", "\"Víz\"gyár", "ELTE Lágymányos Északi épület -4. szint"]);
            names[typeof(ConcreteFactory)] = new NameSet(["Csepeli Betonmûvek", "Eternit Azbesztcementipari Vállalat", "Selypi Cementgyár", "Bitumentõ üzem", "1.12: a szinek világa fejlesztés"]);
            names[typeof(SulfurProducer)] = new NameSet(["Csepeli Kénmûvek", "Tojásromlasztó", "Tudod van az az item Isaacbõl amivel egy hatalmas lézert lõsz, na az", "Kén Kémek Zrt.", "A kéngyûjtõ"]);
            names[typeof(BoneProducer)] = new NameSet(["Csepeli Csontmûvek", "2000 és társa Bt.", "Csonti Farming", "Elefántcsont öböl", "Fehér éjszakák Nyrt."]);
            names[typeof(AshProducer)] = new NameSet(["Csepeli Hamumûvek", "Sötét nappalok Nyrt.", "Hamuzásra kijelölt terület", "Vulkán", "Musztáng Ezredes Emlékgyár"]);
            names[typeof(MushroomProducer)] = new NameSet(["Csepeli Gombamûvek", "****** titkos helye", "a budapesti Móricz Zsigmond körtér jellegzetes épülete", "Vicces csávó", "Spóraföld"]);
            names[typeof(SteamProducer)] = new NameSet(["Csepeli Gõzmûvek", "Csap", "Gõz Gõz Gõz", "Nyomástartó Edény Kft.", "Kérlek fogadd el az EULÁt (a gõzgyár)"]);
            names[typeof(City)] = new NameSet(["Debrecen", "Szeged", "Miskolc", "Pécs", "Gyõr", "Nyíregyháza", "Kecskemét", "Székesfehérvár", "Szombathely", "Érd", "Szolnok", "Tatabánya", "Sopron", "Kaposvár", "Veszprém", "Békéscsaba", "Zalaegerszeg", "Eger", "Dunakeszi", "Nagykanizsa", "Dunaújváros", "Hódmezõvásárhely", "Szigetszentmiklós", "Cegléd", "Vác", "Mosonmagyaróvár", "Baja", "Gödöllõ", "Salgótarján", "Ózd"]);
        }
        public static string NewName(Type placeType, int randomness)
        {
            if (names[placeType].Usable.Count == 0)
            {
                names[placeType].Usable = [.. names[placeType].AllNames];
                names[placeType].LoopNumber += 1;
            }
            string name = names[placeType].Usable.ElementAt(randomness % names[placeType].Usable.Count);
            names[placeType].Usable.Remove(name);
            return name + (names[placeType].LoopNumber > 1 ? " " + names[placeType].LoopNumber.ToString() : "");
        }
    }
}
