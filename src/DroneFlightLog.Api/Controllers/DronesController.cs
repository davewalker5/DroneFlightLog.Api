﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DroneFlightLog.Data.Entities;
using DroneFlightLog.Data.Exceptions;
using DroneFlightLog.Data.Interfaces;
using DroneFlightLog.Data.Sqlite;
using Microsoft.AspNetCore.Mvc;

namespace DroneFlightLog.Api.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Route("[controller]")]
    public class DronesController : Controller
    {
        private readonly IDroneFlightLogFactory<DroneFlightLogDbContext> _factory;

        public DronesController(IDroneFlightLogFactory<DroneFlightLogDbContext> factory)
        {
            _factory = factory;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<List<Drone>>> GetDronesAsync()
        {
            List<Drone> drones = await _factory.Drones.GetDronesAsync(null).ToListAsync();

            if (!drones.Any())
            {
                return NoContent();
            }

            return drones;
        }

        [HttpGet]
        [Route("model/{modelId}")]
        public async Task<ActionResult<List<Drone>>> GetDronesForModelAsync(int modelId)
        {
            List<Drone> drones = await _factory.Drones.GetDronesAsync(modelId).ToListAsync();

            if (!drones.Any())
            {
                return NoContent();
            }

            return drones;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Drone>> GetDroneAsync(int id)
        {
            Drone drone;

            try
            {
                drone = await _factory.Drones.GetDroneAsync(id);
            }
            catch (DroneNotFoundException)
            {
                return NotFound();
            }

            return drone;
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<Drone>> CreateDroneAsync([FromBody] Drone template)
        {
            Drone drone;

            try
            {
                drone = await _factory.Drones.AddDroneAsync(template.Name, template.SerialNumber, template.ModelId);
                await _factory.Context.SaveChangesAsync();
            }
            catch (ModelNotFoundException)
            {
                return BadRequest();
            }
            catch (DroneExistsException)
            {
                return BadRequest();
            }

            return drone;
        }
    }
}
