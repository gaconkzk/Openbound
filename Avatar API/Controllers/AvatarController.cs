using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_API.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class AvatarController : ControllerBase
    {

        /// <summary>
        /// Returns all avatars from game.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(200)]
        public ActionResult GetAllAvatars()
        {
            return Ok("ok");
        }

        /// <summary>
        /// Returns all avatars from player.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [HttpGet("inventory")]
        [ProducesResponseType(200)]
        public ActionResult GetInventory()
        {
            return Ok("ok");
        }

        /// <summary>
        /// Returns updated avatar metadata.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [HttpGet("metadata")]
        [ProducesResponseType(200)]
        public ActionResult GetMetadata()
        {
            return Ok("ok");
        }

        /// <summary>
        /// Returns if avatar api is online.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [HttpGet("status")]
        [ProducesResponseType(200)]
        public ActionResult GetStatus()
        {
            return Ok("ok");
        }
    }
}
