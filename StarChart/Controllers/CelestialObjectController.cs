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
                .Where(co => co.Id == id)
                .ToList();

            if (!firstMatchingCelestialObject.Any())
            {
                return NotFound();
            }

            firstMatchingCelestialObject = UpdateCelestialObjectSatellites(firstMatchingCelestialObject);

            return Ok(firstMatchingCelestialObject.First());
        }

        [HttpGet("{name}")]
        public IActionResult GetByName(string name)
        {
            var allMatchingCelestialObjects = _context
                .CelestialObjects
                .Where(co => co.Name == name)
                .ToList();

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
            var allCelestialObjects = _context.CelestialObjects.ToList();

            allCelestialObjects = UpdateCelestialObjectSatellites(allCelestialObjects);

            return Ok(allCelestialObjects);
        }

        private List<CelestialObject> UpdateCelestialObjectSatellites(List<CelestialObject> celestialObjects)
        {
            if (celestialObjects == null || !celestialObjects.Any())
            {
                throw new ArgumentNullException("no celestial objects passed in");
            }

            foreach (var celestialObject in celestialObjects)
            {
                celestialObject.Satellites = _context
                    .CelestialObjects
                    .Where(co => co.OrbitedObjectId == celestialObject.Id)
                    .ToList();
            }

            return celestialObjects;
        }

        [HttpPost]
        public IActionResult Create([FromBody] CelestialObject celestialObject)
        {
            _context.CelestialObjects.Add(celestialObject);
            var newCelestialObjectId = _context.SaveChanges();
            return CreatedAtRoute(routeName: "GetById", new {Id = newCelestialObjectId});
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, CelestialObject celestialObject)
        {
            var matchingCelestialObject = _context
                .CelestialObjects
                .First(co => co.Id == id);

            if (matchingCelestialObject == null)
            {
                return NotFound();
            }

            matchingCelestialObject.Name = celestialObject.Name;
            matchingCelestialObject.OrbitalPeriod = celestialObject.OrbitalPeriod;
            matchingCelestialObject.OrbitedObjectId = celestialObject.OrbitedObjectId;

            _context.CelestialObjects.Update(matchingCelestialObject);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPatch("{id}/{name}")]
        public IActionResult RenameObject(int id, string name)
        {
            var matchingCelestialObject = _context
                .CelestialObjects
                .First(co => co.Id == id);

            if (matchingCelestialObject == null)
            {
                return NotFound();
            }

            matchingCelestialObject.Name = name;
            _context.CelestialObjects.Update(matchingCelestialObject);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var matchingCelestialObjects =_context
                .CelestialObjects
                .Where(co => co.Id == id || co.OrbitedObjectId == id);
            
            if (!matchingCelestialObjects.Any())
            {
                return NotFound();
            }

            _context.CelestialObjects.RemoveRange(matchingCelestialObjects);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
