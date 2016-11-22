using StructureMap;
using RevenuePlanner.DependencyResolution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevenuePlanner.UnitTest
{
    [TestClass]
    public class DependencySetupForTests
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            IContainer container = IoC.Initialize();
        }
    }
}
