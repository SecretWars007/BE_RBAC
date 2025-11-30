using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiResourceService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        // Solo usuarios con permiso "inventory.read" pueden acceder
        [HttpGet]
        [Authorize(Policy = "InventoryRead")]
        public IActionResult Get()
        {
            var items = new[]
            {
                new
                {
                    Id = 1,
                    Name = "Producto A",
                    Stock = 10,
                },
                new
                {
                    Id = 2,
                    Name = "Producto B",
                    Stock = 5,
                },
            };

            return Ok(items);
        }
    }
}
