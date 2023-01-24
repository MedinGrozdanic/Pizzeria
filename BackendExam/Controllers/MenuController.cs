using BackendExam.DTO;
using ExamContext;
using ExamContext.TestData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BackendExam.Controllers
{
    [Route("[controller]")]
    [ApiController]
 
    public class MenuController : ControllerBase
    {

        [HttpGet]
        public ICollection<MenuItemDTO> GetMenu([FromServices] IMenuTestData menuTestData)
        {

            var menu = menuTestData.Data;
            
            var ListMenu = new List<MenuItemDTO>();

            foreach (MenuItem item in menu)
            {

                ListMenu.Add(new MenuItemDTO
                {
                    Name = item.Name.Name,
                    Price = item.Price,
                });

            }
            return ListMenu;

           


           

      


        }


    }
}
