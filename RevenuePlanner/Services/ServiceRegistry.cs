using StructureMap.Configuration.DSL;
using StructureMap.Pipeline;
using RevenuePlanner.Services.MarketingBudget;
using RevenuePlanner.Services.Transactions;

namespace RevenuePlanner.Services
{
    public class ServiceRegistry : Registry
    {
        public ServiceRegistry()
        {
            this.For<IMarketingBudget>().LifecycleIs(new SingletonLifecycle()).Use<MarketingBudget.MarketingBudget>();
            this.For<ITransaction>().LifecycleIs(new SingletonLifecycle()).Use<FinancialTransaction>();
        }
    }
}