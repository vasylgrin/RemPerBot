using RemBerBot_BL.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySuperUniversalBot_BL.Models
{
    public class NotifyTheUser : ModelBase
    {
        #region Peroperty

        /// <summary>
        /// The reminder ID for the database.
        /// </summary>
        public override int Id { get; set; }
        
        /// <summary>
        /// Chat ID of the user who saves/receives notify.
        /// </summary>
        public override long ChatId { get; set; }

        /// <summary>
        /// The chat ID of the user who will receive notifications about the start of ovulation.
        /// </summary>
        public long ChatIdAdded { get; set; }

        /// <summary>
        /// Username.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ID for Period.
        /// </summary>
        public int? PeriodId { get; set; }

        /// <summary>
        /// Ovulation period for which the user is subscribed.
        /// </summary>
        [ForeignKey("PeriodId")]
        public Period Period { get; set; }

        #endregion

        /// <summary>
        /// Create a new user for ovulation notifications.
        /// </summary>
        /// <param name="chatId">Chat ID of the user who saves/receives notify.</param>
        /// <param name="chatIdAdded">The chat ID of the user who will receive notifications about the start of ovulation.</param>
        /// <param name="name">Username.</param>
        /// <param name="period">Ovulation period for which the user is subscribed.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public NotifyTheUser(long chatId, long chatIdAdded, string name,int periodId)
        {
            #region check for null

            if (chatId <= 0)
                throw new ArgumentException($"\"{nameof(chatId)}\" cannot be empty or null.", nameof(chatId));
            if(chatIdAdded <= 0)
                throw new ArgumentException($"\"{nameof(chatIdAdded)}\" cannot be empty or null.", nameof(chatIdAdded));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"\"{nameof(name)}\" cannot be empty or exsist white space.", nameof(name));
            if (periodId <= 0l)
                throw new ArgumentNullException($"\"{nameof(periodId)}\" cannot be empty or null.", nameof(periodId));

            #endregion
            
            ChatId = chatId;
            ChatIdAdded = chatIdAdded;
            Name = name;
            PeriodId = periodId;
        }

        public NotifyTheUser()
        {

        }

        public override string PrintData()
        {
            return $"Ім'я користувача: {Name}\nChat Id Користувача: {ChatIdAdded}";
        }
    }
}
