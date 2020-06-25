using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite.Internal;
using Microsoft.EntityFrameworkCore;
using StarChart.Data;
using StarChart.Models;

namespace StarChart.Controllers
{
    [Route("")]
    [ApiController]
    public class CelestialObjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CelestialObjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}"), ActionName("GetById")]
        public IActionResult GetById(int id)
        {
            var firstMatchingCelestialObject = _context
                .CelestialObjects
                .Where(co => co.Id == id);
            
            if (firstMatchingCelestialObject.Count() == 0)
            {
                return NotFound();
            }

            firstMatchingCelestialObject = UpdateCelestialObjectSatellites(firstMatchingCelestialObject);

            return Ok(firstMatchingCelestialObject.First());
        }

        [HttpGet("name")]
        public IActionResult GetByName(string name)
        {
            var allMatchingCelestialObjects = _context
                .CelestialObjects
                .Where(co => co.Name == name);

            if (!allMatchingCelestialObjects.Any())
            {
                return NotFound();
            }

            allMatchingCelestialObjects = UpdateCelestialObjectSatellites(allMatchingCelestialObjects);

            return Ok(allMatchingCelestialObjects);

        }

        [HttpGet]
        public IActionResult GetAll()
        {
            IQueryable<CelestialObject> allCelestialObjects = _context.CelestialObjects;

            allCelestialObjects = UpdateCelestialObjectSatellites(allCelestialObjects);
            
            return Ok(allCelestialObjects);
        }

        private IQueryable<CelestialObject> UpdateCelestialObjectSatellites(IQueryable<CelestialObject> celestialObjects)
        {
            if (celestialObjects == null || !celestialObjects.Any())
            {
                throw new ArgumentNullException("no celestial objects passed in");
            }
            foreach (var celestialObject in celestialObjects)
            {
                celestialObject.Satellites = celestialObjects
                    .Where(co => co.OrbitedObjectId == celestialObject.Id)
                    .ToList();
            }

            return celestialObjects;
        }
    }
}
