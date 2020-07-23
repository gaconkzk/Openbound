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
    public class AdminController : ControllerBase
    {
        /// <summary>
        /// Returns all purchases from game.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [HttpGet("purchases")]
        [ProducesResponseType(200)]
        public ActionResult GetAllPurchases([FromRoute] DateTime from, [FromRoute] DateTime to)
        {
            return Ok("ok");
        }

        [HttpGet("payments")]
        [ProducesResponseType(201)]
        public ActionResult GetPayments([FromRoute] string playerId)
        {
            return Ok("ok");
        }

        /// <summary>
        /// Insert a new avatar to player.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [HttpPost("purchase")]
        [ProducesResponseType(200)]
        [ProducesResponseType(409)]
        public ActionResult PurchaseAvatar([FromRoute] string playerId, [FromRoute] string avatarId)
        {
            return Ok("ok");
        }


        [HttpDelete("wipe")]
        [ProducesResponseType(201)]
        public ActionResult WipePlayer([FromRoute] string playerId)
        {
            return Ok("ok");
        }
    }
}
