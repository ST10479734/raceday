using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using raceday.database; 
using raceday.models;
using static raceday.database.EventRepository;

namespace raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryRepository _categoryRepository;
        private string connectionString;

        public CategoryController()
        {
            // Initialize repository layer
            _categoryRepository = new CategoryRepository();
        }

        // POST: api/category/create
        [HttpPost("create")]
        public IActionResult CreateCategory(
            [FromForm] string categoryName,
            [FromForm] int eventId,
            [FromForm] string distance,
            [FromForm] string eventType)
        {
            // 1. Validate Form Inputs
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return BadRequest(new { message = "Category name cannot be empty." });
            }

            if (eventId <= 0)
            {
                return BadRequest(new { message = "A valid Event ID is required." });
            }

            if (string.IsNullOrWhiteSpace(distance))
            {
                return BadRequest(new { message = "Distance cannot be empty." });
            }

            if (string.IsNullOrWhiteSpace(eventType))
            {
                return BadRequest(new { message = "Event type cannot be empty." });
            }

            // 2. Map form parameters to CategoryModel
            CategoryModel categoryModel = new CategoryModel
            {
                CategoryName = categoryName.Trim(),
                EventId = eventId,
                Distance = distance.Trim(),
                EventType = eventType.Trim()
            };

            // 3. Call database insertion
            string resultMessage = _categoryRepository.InsertCategory(categoryModel);

            // 4. Return HTTP Response
            if (resultMessage.Contains("successfully"))
            {
                return Ok(new { message = resultMessage });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { message = resultMessage });
        }

        // GET: api/category/all
        [HttpGet("all")]
        public IActionResult GetAllCategories()
        {
            var categories = _categoryRepository.GetAllCategories();
            return Ok(categories);
        }

        // GET: api/category/event/5
        [HttpGet("event/{eventId}")]
        public List<CategoryModel> GetCategoriesByEvent(int eventId)
        {
            var categories = new List<CategoryModel>();
            string query = "SELECT category_ID, event_ID, categoryname, distance, eventtype FROM categories WHERE event_ID = @eventId;";

            using (SqlConnection connect = new SqlConnection(connectionString))
            {
                using (SqlCommand run_query = new SqlCommand(query, connect))
                {
                    // Make sure the parameter name matches!
                    run_query.Parameters.AddWithValue("@eventId", eventId);
                    connect.Open();
                    using (SqlDataReader reader = run_query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new CategoryModel
                            {
                                CategoryId = Convert.ToInt32(reader["category_ID"]),
                                EventId = Convert.ToInt32(reader["event_ID"]),
                                CategoryName = reader["categoryname"].ToString(),
                                Distance = reader["distance"].ToString(),
                                EventType = reader["eventtype"].ToString()
                            });
                        }
                    }
                }
            }
            return categories;
        }
    }
}
