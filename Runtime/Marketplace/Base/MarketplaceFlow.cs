public class MarketplaceFlow
{
    private MarketplaceOperation operation;

    public MarketplaceFlowState State { get; private set; } = MarketplaceFlowState.Idle;

    public bool TryBegin(MarketplaceFlowState state, MarketplaceOperation newOperation)
    {
        if (State != MarketplaceFlowState.Idle)
            return false;

        State = state;
        operation = newOperation;
        return true;
    }

    public MarketplaceOperation Finish(MarketplaceFlowState state, string sku)
    {
        if (State != state || operation == null)
            return null;

        if (!string.IsNullOrEmpty(sku) && sku != operation.sku)
            return null;

        MarketplaceOperation finished = operation;
        operation = null;
        State = MarketplaceFlowState.Idle;
        return finished;
    }
}
