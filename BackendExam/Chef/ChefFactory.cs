namespace ExamContext.Chef;
public class ChefFactory : IChefFactory
{
    private int chefId = 0;

    private readonly OrderQueue _orderQueue;
    private readonly Warehouse _warehouse;
    private readonly Oven _oven;
    private readonly DeliveryDesk _deliveryDesk;
    private readonly Cookbook _cookbook;

    public ChefFactory(OrderQueue orderQueue, Warehouse warehouse, Oven oven, DeliveryDesk deliveryDesk, Cookbook cookbook)
    {
        this._orderQueue = orderQueue;
        this._warehouse = warehouse;
        this._oven = oven;
        this._deliveryDesk = deliveryDesk;
        this._cookbook = cookbook;
    }

    public IChef CreateChef()
    {
        return new Chef($"Chef_{chefId++}", _orderQueue, _warehouse, _oven, _deliveryDesk, _cookbook);
    }
}
