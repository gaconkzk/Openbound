using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        /// <summary>
        /// Returns all purchases from authenticated player.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [HttpGet("purchases")]
        [ProducesResponseType(200)]
        public ActionResult GetPlayerPurchases([FromRoute] string playerId)
        {
            return Ok("ok");
        }

    }
}
