using StructureMap.Configuration.DSL;
using StructureMap.Pipeline;
using RevenuePlanner.Services.MarketingBudget;

namespace RevenuePlanner.Services
{
    public class ServiceRegistry : Registry
    {
        public ServiceRegistry()
        {
            this.For<IMarketingBudget>().LifecycleIs(new SingletonLifecycle()).Use<MarketingBudget.MarketingBudget>();
        }
    }
}