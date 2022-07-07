using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BakeryApp.Models;

namespace BakeryApp.Controllers
{
    // All of these routes will be at the base URL:     /api/Category
    // That is what "api/[controller]" means below. It uses the name of the controller
    // in this case CategoryController to determine the URL
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        // This is the variable you use to have access to your database
        private readonly DatabaseContext _context;

        // Constructor that recives a reference to your database context
        // and stores it in _context for you to use in your API methods
        public CategoryController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Category
        //
        // Returns a list of all your Categories
        //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            // Uses the database context in `_context` to request all of the Categories, sort
            // them by row id and return them as a JSON array.
            return await _context.Categories.OrderBy(row => row.Id).ToListAsync();
        }

        // GET: api/Category/5
        //
        // Fetches and returns a specific category by finding it by id. The id is specified in the
        // URL. In the sample URL above it is the `5`.  The "{id}" in the [HttpGet("{id}")] is what tells dotnet
        // to grab the id from the URL. It is then made available to us as the `id` argument to the method.
        //
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            // Find the category in the database using `FindAsync` to look it up by id
            var category = await _context.Categories.FindAsync(id);

            // If we didn't find anything, we receive a `null` in return
            if (category == null)
            {
                // Return a `404` response to the client indicating we could not find a category with this id
                return NotFound();
            }

            //  Return the category as a JSON object.
            return category;
        }

        // PUT: api/Category/5
        //
        // Update an individual category with the requested id. The id is specified in the URL
        // In the sample URL above it is the `5`. The "{id} in the [HttpPut("{id}")] is what tells dotnet
        // to grab the id from the URL. It is then made available to us as the `id` argument to the method.
        //
        // In addition the `body` of the request is parsed and then made available to us as a Category
        // variable named category. The controller matches the keys of the JSON object the client
        // supplies to the names of the attributes of our Category POCO class. This represents the
        // new values for the record.
        //
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            // If the ID in the URL does not match the ID in the supplied request body, return a bad request
            if (id != category.Id)
            {
                return BadRequest();
            }

            // Tell the database to consider everything in category to be _updated_ values. When
            // the save happens the database will _replace_ the values in the database with the ones from category
            _context.Entry(category).State = EntityState.Modified;

            try
            {
                // Try to save these changes.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Ooops, looks like there was an error, so check to see if the record we were
                // updating no longer exists.
                if (!CategoryExists(id))
                {
                    // If the record we tried to update was already deleted by someone else,
                    // return a `404` not found
                    return NotFound();
                }
                else
                {
                    // Otherwise throw the error back, which will cause the request to fail
                    // and generate an error to the client.
                    throw;
                }
            }

            // Return a copy of the updated data
            return Ok(category);
        }

        // POST: api/Category
        //
        // Creates a new category in the database.
        //
        // The `body` of the request is parsed and then made available to us as a Category
        // variable named category. The controller matches the keys of the JSON object the client
        // supplies to the names of the attributes of our Category POCO class. This represents the
        // new values for the record.
        //
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            // Indicate to the database context we want to add this new record
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Return a response that indicates the object was created (status code `201`) and some additional
            // headers with details of the newly created object.
            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        // DELETE: api/Category/5
        //
        // Deletes an individual category with the requested id. The id is specified in the URL
        // In the sample URL above it is the `5`. The "{id} in the [HttpDelete("{id}")] is what tells dotnet
        // to grab the id from the URL. It is then made available to us as the `id` argument to the method.
        //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // Find this category by looking for the specific id
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                // There wasn't a category with that id so return a `404` not found
                return NotFound();
            }

            // Tell the database we want to remove this record
            _context.Categories.Remove(category);

            // Tell the database to perform the deletion
            await _context.SaveChangesAsync();

            // Return a copy of the deleted data
            return Ok(category);
        }

        // Private helper method that looks up an existing category by the supplied id
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(category => category.Id == id);
        }
    }
}
