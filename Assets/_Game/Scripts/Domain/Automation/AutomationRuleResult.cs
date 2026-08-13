namespace StockMarket.Domain.Automation
{
    public readonly struct AutomationRuleResult
    {
        private AutomationRuleResult(
            bool succeeded,
            AutomationRule rule,
            AutomationRuleFailure failure)
        {
            Succeeded = succeeded;
            Rule = rule;
            Failure = failure;
        }

        public bool Succeeded { get; }
        public AutomationRule Rule { get; }
        public AutomationRuleFailure Failure { get; }

        internal static AutomationRuleResult Success(AutomationRule rule)
        {
            return new AutomationRuleResult(true, rule, AutomationRuleFailure.None);
        }

        internal static AutomationRuleResult Failed(AutomationRuleFailure failure)
        {
            return new AutomationRuleResult(false, null, failure);
        }
    }
}
