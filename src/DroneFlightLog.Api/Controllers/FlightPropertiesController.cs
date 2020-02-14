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
    public class FlightPropertiesController : Controller
    {
        private readonly IDroneFlightLogFactory<DroneFlightLogDbContext> _factory;

        public FlightPropertiesController(IDroneFlightLogFactory<DroneFlightLogDbContext> factory)
        {
            _factory = factory;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<List<FlightProperty>>> GetPropertiesAsync()
        {
            List<FlightProperty> properties = await _factory.Properties.GetPropertiesAsync().ToListAsync();

            if (!properties.Any())
            {
                return NoContent();
            }

            return properties;
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<FlightProperty>> CreatePropertyAsync([FromBody] FlightProperty template)
        {
            FlightProperty property;

            try
            {
                property = await _factory.Properties.AddPropertyAsync(template.Name, template.DataType, template.IsSingleInstance);
                await _factory.Context.SaveChangesAsync();
            }
            catch (PropertyExistsException)
            {
                return BadRequest();
            }

            return property;
        }

        [HttpGet]
        [Route("values/{flightId}")]
        public async Task<ActionResult<List<FlightPropertyValue>>> GetPropertyValuesAsync(int flightId)
        {
            List<FlightPropertyValue> properties = await _factory.Properties.GetPropertyValuesAsync(flightId).ToListAsync();

            if (!properties.Any())
            {
                return NoContent();
            }

            return properties;
        }

        [HttpPost]
        [Route("values")]
        public async Task<ActionResult<FlightPropertyValue>> CreatePropertyValueAsync([FromBody] FlightPropertyValue template)
        {
            FlightPropertyValue value;

            try
            {
                FlightProperty property = await _factory.Properties
                                                        .GetPropertiesAsync()
                                                        .FirstOrDefaultAsync(p => p.Id == template.PropertyId);
                switch (property.DataType)
                {
                    case FlightPropertyDataType.Date:
                        value = await _factory.Properties.AddPropertyValueAsync(template.FlightId, template.PropertyId, template.DateValue);
                        break;
                    case FlightPropertyDataType.Number:
                        value = await _factory.Properties.AddPropertyValueAsync(template.FlightId, template.PropertyId, template.NumberValue);
                        break;
                    case FlightPropertyDataType.String:
                        value = await _factory.Properties.AddPropertyValueAsync(template.FlightId, template.PropertyId, template.StringValue);
                        break;
                    default:
                        return BadRequest();
                }

                await _factory.Context.SaveChangesAsync();
            }
            catch (ValueExistsException)
            {
                return BadRequest();
            }

            return value;
        }
    }
}