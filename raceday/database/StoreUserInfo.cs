using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using raceday.models;
namespace raceday.database
{// Start of namespace for database operations
    //
    public class StoreUserInfo
    {
        // Connection string pointing to the local database
        private readonly string _connectionString = @"Data Source=(localdb)\api_guide;Database=raceday;Integrated Security=True;";

        // Method to handle hashing and inserting user record into SQL Server
        public (string Message, int UserId) UserInfo(string fullname, UserRole role, string email, string password)
        {
            string message;
            int newUserId = 0;
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            string query = @"INSERT INTO users (fullname, role, email, password) 
                             VALUES (@fullname, @role, @email, @password);
                             SELECT SCOPE_IDENTITY();";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@fullname", fullname);
                        run_query.Parameters.AddWithValue("@role", role.ToString());
                        run_query.Parameters.AddWithValue("@email", email);
                        run_query.Parameters.AddWithValue("@password", hashedPassword);
                        connect.Open();
                        newUserId = Convert.ToInt32(run_query.ExecuteScalar());
                        message = "User info was successfully saved.";
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error saving user: " + error.Message;
            }
            return (message, newUserId);
        }

        public bool ValidateUser(string email, string password, out int userId, out string fullname, out string role)
        {
            userId = 0;
            fullname = string.Empty;
            role = string.Empty;
            string hashedPassword = string.Empty;
            string query = "SELECT user_ID, fullname, role, password FROM users WHERE email = @email;";

            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@email", email);
                        connect.Open();
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userId = Convert.ToInt32(reader["user_ID"]);
                                fullname = reader["fullname"].ToString();
                                role = reader["role"].ToString();
                                hashedPassword = reader["password"].ToString();
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string UpdateUser(string email, string fullname, UserRole role, string password)
        {
            string message;
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            string query = @"UPDATE users 
                             SET fullname = @fullname, role = @role, password = @password 
                             WHERE email = @email;";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@fullname", fullname);
                        run_query.Parameters.AddWithValue("@role", role.ToString());
                        run_query.Parameters.AddWithValue("@password", hashedPassword);
                        run_query.Parameters.AddWithValue("@email", email);
                        connect.Open();
                        int rowsAffected = run_query.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            message = "User info was successfully updated.";
                        }
                        else
                        {
                            message = "Error saving user: User with this email was not found.";
                        }
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error updating user: " + error.Message;
            }
            return message;
        }
    }

    public class EventRepository
    {
        private readonly string _connectionString = @"Data Source=(localdb)\api_guide;Database=raceday;Integrated Security=True;";

        public List<EnrolmentDetailModel> GetEnrolmentsByOrganizerId(int organizerId)
        {
            var enrolmentList = new List<EnrolmentDetailModel>();
            string query = @"
                SELECT 
                    e.enrolment_ID,
                    e.user_ID,
                    u.fullname AS ParticipantName,
                    u.email AS ParticipantEmail,
                    e.category_ID,
                    c.categoryname,
                    c.distance,
                    c.eventtype,
                    ev.event_ID,
                    ev.eventname,
                    e.enrolment_date,
                    e.racenumber,
                    e.paymentstatus,
                    p.amount,
                    p.paymentdate,
                    p.method
                FROM enrolment e
                JOIN users u ON e.user_ID = u.user_ID
                JOIN category c ON e.category_ID = c.category_ID
                JOIN events ev ON c.event_ID = ev.event_ID
                LEFT JOIN PAYMENT p ON e.enrolment_ID = p.enrolment_ID
                WHERE ev.organizer_ID = @organizer_ID;";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@organizer_ID", organizerId);
                        connect.Open();
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                enrolmentList.Add(new EnrolmentDetailModel
                                {
                                    EnrolmentId = Convert.ToInt32(reader["enrolment_ID"]),
                                    UserId = Convert.ToInt32(reader["user_ID"]),
                                    ParticipantName = reader["ParticipantName"].ToString(),
                                    ParticipantEmail = reader["ParticipantEmail"].ToString(),
                                    CategoryId = Convert.ToInt32(reader["category_ID"]),
                                    CategoryName = reader["categoryname"].ToString(),
                                    Distance = reader["distance"].ToString(),
                                    EventType = reader["eventtype"].ToString(),
                                    EventId = Convert.ToInt32(reader["event_ID"]),
                                    EventName = reader["eventname"].ToString(),
                                    EnrolmentDate = Convert.ToDateTime(reader["enrolment_date"]),
                                    RaceNumber = reader["racenumber"].ToString(),
                                    PaymentStatus = reader["paymentstatus"].ToString(),
                                    Amount = reader["amount"]?.ToString(),
                                    PaymentDate = reader["paymentdate"] != DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : (DateTime?)null,
                                    Method = reader["method"]?.ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return enrolmentList;
        }

        public string InsertEvent(EventModel eventModel)
        {
            string message;
            string query = @"INSERT INTO events (organizer_ID, eventname, location, eventdate) 
                     VALUES (@organizer_ID, @eventname, @location, @eventdate);";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@organizer_ID", eventModel.OrganizerId);
                        run_query.Parameters.AddWithValue("@eventname", eventModel.EventName);
                        run_query.Parameters.AddWithValue("@location", eventModel.Location);
                        run_query.Parameters.AddWithValue("@eventdate", eventModel.EventDate);
                        connect.Open();
                        run_query.ExecuteNonQuery();
                        message = "Event was successfully created.";
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error creating event: " + error.Message;
            }
            return message;
        }

        public string UpdateEvent(EventModel eventModel)
        {
            string message;
            string query = @"UPDATE events 
                             SET eventname = @eventname, location = @location, eventdate = @eventdate 
                             WHERE event_ID = @event_ID AND organizer_ID = @organizer_ID;";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@event_ID", eventModel.EventId);
                        run_query.Parameters.AddWithValue("@organizer_ID", eventModel.OrganizerId);
                        run_query.Parameters.AddWithValue("@eventname", eventModel.EventName);
                        run_query.Parameters.AddWithValue("@location", eventModel.Location);
                        run_query.Parameters.AddWithValue("@eventdate", eventModel.EventDate);
                        connect.Open();
                        int rowsAffected = run_query.ExecuteNonQuery();
                        message = rowsAffected > 0 ? "Event was successfully updated." : "Error updating event: Event not found or unauthorized organizer.";
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error updating event: " + error.Message;
            }
            return message;
        }
        public List<EventModel> GetAllEvents()
        {
            var events = new List<EventModel>();
            string query = "SELECT event_ID, organizer_ID, eventname, location, eventdate FROM events;";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        connect.Open();
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                events.Add(new EventModel
                                {
                                    EventId = Convert.ToInt32(reader["event_ID"]),
                                    OrganizerId = Convert.ToInt32(reader["organizer_ID"]),
                                    EventName = reader["eventname"].ToString(),
                                    Location = reader["location"].ToString(),
                                    EventDate = Convert.ToDateTime(reader["eventdate"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return events;
        }
        public string DeleteEvent(int eventId, int organizerId)
        {
            string message;
            string query = "DELETE FROM events WHERE event_ID = @event_ID AND organizer_ID = @organizer_ID;";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@event_ID", eventId);
                        run_query.Parameters.AddWithValue("@organizer_ID", organizerId);
                        connect.Open();
                        int rowsAffected = run_query.ExecuteNonQuery();
                        message = rowsAffected > 0 ? "Event was successfully deleted." : "Error deleting event: Event not found or unauthorized organizer.";
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error deleting event: " + error.Message;
            }
            return message;
        }
    }

    public class CategoryRepository
    {
        private readonly string _connectionString = @"Data Source=(localdb)\api_guide;Database=raceday;Integrated Security=True;";

        public string InsertCategory(CategoryModel categoryModel)
        {
            string message;
            string query = @"INSERT INTO category (categoryname, event_ID, distance, eventtype) 
                             VALUES (@categoryname, @event_ID, @distance, @eventtype);";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@categoryname", categoryModel.CategoryName);
                        run_query.Parameters.AddWithValue("@event_ID", categoryModel.EventId);
                        run_query.Parameters.AddWithValue("@distance", categoryModel.Distance);
                        run_query.Parameters.AddWithValue("@eventtype", categoryModel.EventType);
                        connect.Open();
                        run_query.ExecuteNonQuery();
                        message = "Category was successfully created.";
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error creating category: " + error.Message;
            }
            return message;
        }

        public List<CategoryModel> GetAllCategories()
        {
            var categories = new List<CategoryModel>();
            string query = "SELECT category_ID, categoryname, event_ID, distance, eventtype FROM category;";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        connect.Open();
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new CategoryModel
                                {
                                    CategoryId = Convert.ToInt32(reader["category_ID"]),
                                    CategoryName = reader["categoryname"].ToString(),
                                    EventId = Convert.ToInt32(reader["event_ID"]),
                                    Distance = reader["distance"].ToString(),
                                    EventType = reader["eventtype"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return categories;
        }

        public List<CategoryModel> GetCategoriesByEventId(int eventId)
        {
            var categories = new List<CategoryModel>();
            string query = "SELECT category_ID, categoryname, event_ID, distance, eventtype WHERE event_ID = @event_ID;";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@event_ID", eventId);
                        connect.Open();
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new CategoryModel
                                {
                                    CategoryId = Convert.ToInt32(reader["category_ID"]),
                                    CategoryName = reader["categoryname"].ToString(),
                                    EventId = Convert.ToInt32(reader["event_ID"]),
                                    Distance = reader["distance"].ToString(),
                                    EventType = reader["eventtype"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return categories;
        }
    }

    public class EnrolmentRepository
    {
        private readonly string _connectionString = @"Data Source=(localdb)\api_guide;Database=raceday;Integrated Security=True;";



        public string InsertEnrolment(EnrolmentModel enrolmentModel)
        {
            string message;
            string query = @"INSERT INTO enrolments (user_ID, category_ID, racenumber, paymentstatus) 
                     VALUES (@user_ID, @category_ID, @racenumber, @paymentstatus);";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@user_ID", enrolmentModel.UserId);
                        run_query.Parameters.AddWithValue("@category_ID", enrolmentModel.CategoryId);
                        run_query.Parameters.AddWithValue("@racenumber", enrolmentModel.RaceNumber);
                        run_query.Parameters.AddWithValue("@paymentstatus", enrolmentModel.PaymentStatus);
                        connect.Open();
                        run_query.ExecuteNonQuery();
                        message = "Enrolled successfully.";
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error enrolling: " + error.Message;
            }
            return message;
        }
    }

    public class EnrolmentRepositorys
    {
        private readonly string _connectionString = @"Data Source=(localdb)\api_guide;Database=raceday;Integrated Security=True;";

        // 1. Insert a new participant enrolment
        public string InsertEnrolment(EnrolmentModel enrolmentModel)
        {
            string message;
            string query = @"INSERT INTO enrolments (user_ID, category_ID, racenumber, paymentstatus) 
                             VALUES (@user_ID, @category_ID, @racenumber, @paymentstatus);";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@user_ID", enrolmentModel.UserId);
                        run_query.Parameters.AddWithValue("@category_ID", enrolmentModel.CategoryId);
                        run_query.Parameters.AddWithValue("@racenumber", enrolmentModel.RaceNumber);
                        run_query.Parameters.AddWithValue("@paymentstatus", enrolmentModel.PaymentStatus);
                        connect.Open();
                        run_query.ExecuteNonQuery();
                        message = "Enrolled successfully.";
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error enrolling: " + error.Message;
            }
            return message;
        }

        // 2. Fetch all enrolments for events created by a specific organizer
        public List<EnrolmentViewModel> GetEnrolmentsByOrganizer(int organizerId)
        {
            var enrolments = new List<EnrolmentViewModel>();
            string query = @"
                SELECT e.enrolment_ID, u.fullname AS ParticipantName, ev.eventname, c.categoryname, e.racenumber, e.paymentstatus
                FROM enrolments e
                INNER JOIN users u ON e.user_ID = u.user_ID
                INNER JOIN categories c ON e.category_ID = c.category_ID
                INNER JOIN events ev ON c.event_ID = ev.event_ID
                WHERE ev.organizer_ID = @organizerId;";

            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@organizerId", organizerId);
                        connect.Open();
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                enrolments.Add(new EnrolmentViewModel
                                {
                                    EnrolmentId = Convert.ToInt32(reader["enrolment_ID"]),
                                    ParticipantName = reader["ParticipantName"].ToString(),
                                    EventName = reader["eventname"].ToString(),
                                    CategoryName = reader["categoryname"].ToString(),
                                    RaceNumber = reader["racenumber"].ToString(),
                                    PaymentStatus = reader["paymentstatus"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception) { }

            return enrolments;
        }
    }


public class ResultRepository
    {
        private readonly string _connectionString = @"Data Source=(localdb)\api_guide;Database=raceday;Integrated Security=True;";

        public string InsertResult(ResultModel model)
        {
            string message;
            string query = @"INSERT INTO results (enrolment_ID, finishtime, overallposition, categoryposition) 
                             VALUES (@enrolment_ID, @finishtime, @overallposition, @categoryposition);";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@enrolment_ID", model.EnrolmentId);
                        run_query.Parameters.AddWithValue("@finishtime", model.FinishTime);
                        run_query.Parameters.AddWithValue("@overallposition", model.OverallPosition);
                        run_query.Parameters.AddWithValue("@categoryposition", model.CategoryPosition);
                        connect.Open();
                        run_query.ExecuteNonQuery();
                        message = "Race result was successfully captured.";
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error capturing result: " + error.Message;
            }
            return message;
        }

        public List<ResultModel> GetResultsByUserId(int userId)
        {
            var resultsList = new List<ResultModel>();
            string query = @"
                SELECT 
                    r.results_ID,
                    r.enrolment_ID,
                    r.finishtime,
                    r.overallposition,
                    r.categoryposition,
                    e.racenumber,
                    u.fullname AS ParticipantName,
                    c.categoryname,
                    ev.eventname
                FROM results r
                JOIN enrolment e ON r.enrolment_ID = e.enrolment_ID
                JOIN users u ON e.user_ID = u.user_ID
                JOIN category c ON e.category_ID = c.category_ID
                JOIN events ev ON c.event_ID = ev.event_ID
                WHERE e.user_ID = @user_ID;";
            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@user_ID", userId);
                        connect.Open();
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                resultsList.Add(new ResultModel
                                {
                                    ResultsId = Convert.ToInt32(reader["results_ID"]),
                                    EnrolmentId = Convert.ToInt32(reader["enrolment_ID"]),
                                    FinishTime = (TimeSpan)reader["finishtime"],
                                    OverallPosition = Convert.ToInt32(reader["overallposition"]),
                                    CategoryPosition = Convert.ToInt32(reader["categoryposition"]),
                                    RaceNumber = reader["racenumber"].ToString(),
                                    ParticipantName = reader["ParticipantName"].ToString(),
                                    CategoryName = reader["categoryname"].ToString(),
                                    EventName = reader["eventname"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return resultsList;
        }
    }
}