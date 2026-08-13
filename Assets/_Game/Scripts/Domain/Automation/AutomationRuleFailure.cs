namespace StockMarket.Domain.Automation
{
    public enum AutomationRuleFailure
    {
        None = 0,
        InvalidCompanyId = 1,
        UnknownCompany = 2,
        InvalidCondition = 3,
        InvalidTriggerPrice = 4,
        InvalidQuantity = 5,
        InvalidCooldown = 6,
        CapacityReached = 7,
        RuleIdExhausted = 8
    }
}
