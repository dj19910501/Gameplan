using StructureMap.Configuration.DSL;
using StructureMap.Pipeline;
using RevenuePlanner.Services.MarketingBudget;
using RevenuePlanner.Services.Transactions;
using RevenuePlanner.Services.PlanPicker;

namespace RevenuePlanner.Services
{
    public class ServiceRegistry : Registry
    {
        public ServiceRegistry()
        {
            this.For<IMarketingBudget>().LifecycleIs(new SingletonLifecycle()).Use<MarketingBudget.MarketingBudget>();
            this.For<ITransaction>().LifecycleIs(new SingletonLifecycle()).Use<FinancialTransaction>();
            this.For<IPlanPicker>().LifecycleIs(new SingletonLifecycle()).Use<PlanPicker.PlanPicker>();
            this.For<ICurrency>().LifecycleIs(new UniquePerRequestLifecycle()).Use<Currency>();
        }
    }
}