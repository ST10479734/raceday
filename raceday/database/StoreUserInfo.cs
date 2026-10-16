using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using raceday.models;
namespace raceday.database
{// Start of namespace for database operations
    public class StoreUserInfo
    {
        // Connection string pointing to the local database
        private readonly string _connectionString = @"Data Source=(localdb)\api_guide;Database=raceday;Integrated Security=True;";

        // Method to handle hashing and inserting user record into SQL Server
        public string UserInfo(string fullname, UserRole role, string email, string password)
        {
            string message;

            // Securely hash the plain text password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // SQL command to insert user details into the database table
            string query = @"INSERT INTO users (fullname, role, email, password) 
                             VALUES (@fullname, @role, @email, @password);";

            try
            {
                // Open database connection and execute query securely using parameters
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@fullname", fullname);
                        run_query.Parameters.AddWithValue("@role", role.ToString());
                        run_query.Parameters.AddWithValue("@email", email);
                        run_query.Parameters.AddWithValue("@password", hashedPassword);

                        connect.Open();
                        run_query.ExecuteNonQuery();
                        message = "User info was successfully saved.";
                    }
                }
            }
            catch (Exception error)
            {
                // Catch any database or connection errors
                message = "Error saving user: " + error.Message;
            }

            return message;
        }

        public bool ValidateUser(string email, string password, out string fullname, out string role)
        {
            fullname = string.Empty;
            role = string.Empty;
            string hashedPassword = string.Empty;

            string query = "SELECT fullname, role, password FROM users WHERE email = @email;";

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
                                fullname = reader["fullname"].ToString();
                                role = reader["role"].ToString();
                                hashedPassword = reader["password"].ToString();
                            }
                            else
                            {
                                return false; // User not found
                            }
                        }
                    }
                }

                // Verify the hashed password against the plain text input
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception)
            {
                // Log exception if needed
                return false;
            }
        }
        public string UpdateUser(string email, string fullname, UserRole role, string password)
        {
            string message;

            // Securely hash the new password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // SQL command to update user details based on email
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

        public string CreateEvent(int organizerId, string eventName, string location, DateTime eventDate)
        {
            string message;

            // SQL command to insert event details into the events table
            string query = @"INSERT INTO events (organizer_ID, eventname, location, eventdate) 
                             VALUES (@organizerId, @eventname, @location, @eventdate);";

            try
            {
                // Open database connection and execute query securely using parameters
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@organizerId", organizerId);
                        run_query.Parameters.AddWithValue("@eventname", eventName);
                        run_query.Parameters.AddWithValue("@location", location);
                        run_query.Parameters.AddWithValue("@eventdate", eventDate);

                        connect.Open();
                        run_query.ExecuteNonQuery();
                        message = "Event was successfully created.";
                    }
                }
            }
            catch (Exception error)
            {
                // Catch any database or connection errors
                message = "Error creating event: " + error.Message;
            }

            return message;
        }
        

    }
    public class EventRepository
    {
        // Connection string pointing to the local database matching your project
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
            catch (Exception)
            {
                // Return empty list on failure
            }

            return enrolmentList;
        }

        // Method to insert the event record into SQL Server securely using parameters
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

                        if (rowsAffected > 0)
                        {
                            message = "Event was successfully updated.";
                        }
                        else
                        {
                            message = "Error updating event: Event not found or unauthorized organizer.";
                        }
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error updating event: " + error.Message;
            }

            return message;
        }

        // Method to delete an event
        public string DeleteEvent(int eventId, int organizerId)
        {
            string message;

            // Optional: If you have foreign key constraints in categories referencing events, 
            // you may want to delete related categories first or handle cascade deletion.
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

                        if (rowsAffected > 0)
                        {
                            message = "Event was successfully deleted.";
                        }
                        else
                        {
                            message = "Error deleting event: Event not found or unauthorized organizer.";
                        }
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error deleting event: " + error.Message;
            }

            return message;
        }



        public class CategoryRepository
        {
            private readonly string _connectionString = @"Data Source=(localdb)\api_guide;Database=raceday;Integrated Security=True;";

            // Method to insert a category linked to an event
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
                catch (Exception)
                {
                    // Return empty list on failure
                }

                return categories;
            }

            // Method to get categories filtered by a specific Event ID
            public List<CategoryModel> GetCategoriesByEventId(int eventId)
            {
                var categories = new List<CategoryModel>();
                string query = "SELECT category_ID, categoryname, event_ID, distance, eventtype FROM category WHERE event_ID = @event_ID;";

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
                catch (Exception)
                {
                    // Return empty list on failure
                }

                return categories;
            }

        }


        }
    public class EnrolmentRepository
    {
        private readonly string _connectionString = @"Data Source=(localdb)\api_guide;Database=raceday;Integrated Security=True;";

        public string InsertEnrolment(EnrolmentModel model)
        {
            string message;

            string enrolmentQuery = @"INSERT INTO enrolment (user_ID, category_ID, enrolment_date, racenumber, paymentstatus) 
                                      VALUES (@user_ID, @category_ID, @enrolment_date, @racenumber, @paymentstatus);
                                      SELECT SCOPE_IDENTITY();";

            string paymentQuery = @"INSERT INTO PAYMENT (enrolment_ID, amount, paymentdate, method) 
                                    VALUES (@enrolment_ID, @amount, @paymentdate, @method);";

            try
            {
                using (SqlConnection connect = new SqlConnection(_connectionString))
                {
                    connect.Open();
                    using (SqlTransaction transaction = connect.BeginTransaction())
                    {
                        try
                        {
                            int generatedEnrolmentId;

                            // 1. Insert Enrolment (passing enum as string via .ToString())
                            using (SqlCommand cmdEnrolment = new SqlCommand(enrolmentQuery, connect, transaction))
                            {
                                cmdEnrolment.Parameters.AddWithValue("@user_ID", model.UserId);
                                cmdEnrolment.Parameters.AddWithValue("@category_ID", model.CategoryId);
                                cmdEnrolment.Parameters.AddWithValue("@enrolment_date", model.EnrolmentDate);
                                cmdEnrolment.Parameters.AddWithValue("@racenumber", model.RaceNumber);
                                cmdEnrolment.Parameters.AddWithValue("@paymentstatus", model.PaymentStatus.ToString());

                                generatedEnrolmentId = Convert.ToInt32(cmdEnrolment.ExecuteScalar());
                            }

                            // 2. Insert Payment record (passing enum as string via .ToString())
                            using (SqlCommand cmdPayment = new SqlCommand(paymentQuery, connect, transaction))
                            {
                                cmdPayment.Parameters.AddWithValue("@enrolment_ID", generatedEnrolmentId);
                                cmdPayment.Parameters.AddWithValue("@amount", model.Amount);
                                cmdPayment.Parameters.AddWithValue("@paymentdate", model.PaymentDate);
                                cmdPayment.Parameters.AddWithValue("@method", model.Method.ToString());

                                cmdPayment.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            message = "Participant was successfully enrolled and payment recorded.";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            message = "Error during transaction: " + ex.Message;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                message = "Error connecting to database: " + error.Message;
            }

            return message;
        }
    }

    public class ResultRepository
    {
        private readonly string _connectionString = @"Data Source=(localdb)\api_guide;Database=raceday;Integrated Security=True;";

        // Method for organizers to capture finish time and positions
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

        // Method for participants to view their own results by User ID
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
            catch (Exception)
            {
                // Return empty list on failure
            }

            return resultsList;
        }
    }


}// End of database namespace