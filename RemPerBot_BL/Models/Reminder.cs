namespace MySuperUniversalBot_BL.Models
{
    public class Reminder
    {
        /// <summary>
        /// The reminder ID for the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Chat ID of the user who saves/receives reminders.
        /// </summary>
        public long ChatId { get; set; }

        /// <summary>
        /// Reminder subject.
        /// </summary>
        public string? Topic { get; set; }

        /// <summary>
        /// Reminder date.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Creates a new reminder.
        /// </summary>
        /// <param name="сhatId">Chat ID of the user who saves/receives reminders.</param>
        /// <param name="topic">Reminder subject.</param>
        /// <param name="dateTime">Reminder date.</param>
        /// <exception cref="ArgumentException"></exception>
        public Reminder(long сhatId, string topic, DateTime dateTime)
        {
            #region check for null

            if (сhatId <= 0)
                throw new ArgumentException($"\"{nameof(сhatId)}\" cannot be empty or null", nameof(сhatId));
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException($"\"{nameof(topic)}\" cannot be empty or contain only a space.", nameof(topic));
            if (dateTime < DateTime.MinValue)
                throw new ArgumentException($"\"{nameof(dateTime)}\" cannot be empty or default value.", nameof(dateTime));

            #endregion

            ChatId = сhatId;
            Topic = topic;
            DateTime = dateTime;
        }

        
        public Reminder()
        {

        }
    }
}
