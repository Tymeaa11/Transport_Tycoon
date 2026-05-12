using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport_Tests.Utils
{
    [TestClass]
    public class RandomNameGeneratorTests
    {
        [TestMethod]
        public void NameSet_AllNames_SetterWorks()
        {
            var ns = new NameSet(["A", "B"])
            {
                AllNames = ["C", "D"]
            };
            Assert.IsTrue(ns.AllNames.Contains("C"));
        }

        [TestMethod]
        public void RandomNameGenerator_Exhaustion_ResetsUsable()
        {
            RandomNameGenerator.Reset();
            for (int i = 0; i < 6; i++)
            {
                RandomNameGenerator.NewName(typeof(SulfurProducer), i);
            }
        }
    }

}
