using BackendExam.DTO;
using ExamContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Console;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace BackendExam.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private static object queueLock = new();
        private static object lok = new();

        [HttpPost]
        public IActionResult OrderPizza([FromBody] List<string> pizzas, [FromServices] OrderQueue orderQueue)
        {

            var distList = pizzas.Distinct();
            List<(PizzaType, int)> ordersList = new List<(PizzaType, int)>();
            Dictionary<string, int> orderDict = new Dictionary<string, int>();
            foreach (var item in pizzas)
            {
                if (orderDict.ContainsKey(item))
                {
                    var occurences = orderDict[item];
                    orderDict[item] = occurences + 1;
                }
                else
                {
                    orderDict[item] = 1;

                }
            }

            foreach (KeyValuePair<string, int> item in orderDict)
            {
                ordersList.Add((new PizzaType(item.Key), item.Value));
            }
            var order = new Order(ordersList);

            lock (queueLock)
            {
                orderQueue.Queue.Enqueue(order);

            }
            List<string> pizzaList = distList.ToList();
            if (pizzaList.Count > 0)
            {
                pizzaList.Add(pizzaList[0]);
            }
            else
            {
                return BadRequest();
            }

            if (pizzaList.Contains("Made up pizza"))
            {
                return BadRequest(pizzaList[0]);
            }


            return Created($"/order/{order.Id}", order);

        }



        [HttpGet("/order/{Id}")]
        public IActionResult GetOrder([FromRoute] int Id, [FromServices] DeliveryDesk deliveryDesk)
        {


            lock (lok)
            {
                if (deliveryDesk.FinishedOrders.ContainsKey(Id))
                {
                    List<Pizza> pizzas = deliveryDesk.FinishedOrders[Id];
                    Dictionary<string, object> FinOrder = new Dictionary<string, object>();
                    List<Object> json = new List<Object>();

                    foreach (Pizza item in pizzas)
                    {
                        json.Add(new
                        {
                            id = item.Id,
                            type = item.Name.Name
                        });

                    }

                        FinOrder.Add("status", "done");
                        FinOrder.Add("order", json.ToArray());

                        deliveryDesk.FinishedOrders.Remove(Id);
                        return Ok(FinOrder);
                }
                else
                {
                    return NotFound();
                }
                

            }
        }
    }
}

