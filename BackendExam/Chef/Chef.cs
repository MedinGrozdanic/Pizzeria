namespace ExamContext.Chef;
using System.Collections.Immutable;

public class Chef : IChef
{
    private static object queueLock = new();
    private static object warehouseLock = new();
    private static object ovenLock = new();
    private static object deskLock = new();

    public string Name { get; private set; }
    private DeliveryDesk _deliveryDesk { get; }
    private OrderQueue _orderQueue { get; }
    private Oven _oven { get; }
    private Warehouse _warehouse { get; }
    private Cookbook _cookbook { get; }
    public IngredientType _IngredientType { get; }

    public Order? _order { get; set; }

    public Chef(String Name, OrderQueue orderQueue, Warehouse warehouse, Oven oven, DeliveryDesk deliveryDesk, Cookbook cookbook)
    {
        this.Name = Name;
        this._orderQueue = orderQueue;
        this._warehouse = warehouse;
        this._oven = oven;
        this._deliveryDesk = deliveryDesk;
        this._cookbook = cookbook;
    }

    public void Run()
    {

        while (true)
        {

            lock (queueLock)
            {
                if (_orderQueue.Queue.Count > 0)
                {
                    _order = _orderQueue.Queue.Dequeue();
                }
            }
            if (_order == null)
            {
                Thread.Sleep(10);
                continue;
            }
            List<(PizzaType pizzatype, int quantity)> checkOrder;
            checkOrder = _order.Pizzas;

            List<List<Ingredient>> pizzaIngredients = new();
            foreach (var pizza in checkOrder)
            {
                ImmutableList<(IngredientType ingredient, int quantity)> IngredientCheck;
                if (_cookbook.CookbookList.ContainsKey(pizza.pizzatype))
                {
                    IngredientCheck = _cookbook.CookbookList[pizza.pizzatype];
                }
                else
                {
                    continue;
                }
                for (int index = 0; index < pizza.quantity; index++)
                {
                    List<Ingredient> ingredients = new List<Ingredient>();
                    foreach ((IngredientType _IngredietType, int quantity) Ingredient in IngredientCheck)
                    {
                        lock(warehouseLock)
                        {
                            int availableQuantity;
                            try
                            {
                                availableQuantity = Math.Min(Ingredient.quantity, _warehouse.PeekIngredient(Ingredient._IngredietType).Count);
                            }
                            catch(KeyNotFoundException)
                            {
                                continue;
                            }

                            for (int i = 0; i < availableQuantity; i++)
                            {
                                Ingredient? pizzaIngredient;
                                pizzaIngredient = _warehouse.GetIngredient(Ingredient._IngredietType, this);
                                if (pizzaIngredient == null)
                                {
                                    break;
                                }
                                ingredients.Add(pizzaIngredient);
                            }

                        }
                    }

                    pizzaIngredients.Add(ingredients);
                }
            }

            List<Pizza> finishedPizzas = new();
            List<Task<Pizza>> tasks = new();
            foreach (var unfinishedPizza in pizzaIngredients)
            {
                tasks.Add(Task.Run(() => _oven.Bake(unfinishedPizza)));
            }
            Task.WaitAll(tasks.ToArray());

            foreach (var task in tasks)
            {
                finishedPizzas.Add(task.Result);
            }

            _deliveryDesk.FinishedOrders.Add(_order.Id, finishedPizzas);
            _order = null;
        }


    }
}
