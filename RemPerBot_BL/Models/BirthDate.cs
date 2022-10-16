using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemBerBot_BL.Models
{
    internal class BirthDate : ModelBase
    {
        /// <summary>
        /// The reminder ID for the database.
        /// </summary>
        public override int Id { get; set; }

        /// <summary>
        /// Chat ID of the user who saves/receives reminders.
        /// </summary>
        public override long ChatId { get; set; }

        /// <summary>
        /// Reminder subject.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Reminder date.
        /// </summary>
        public DateTime DateOfBirthday { get; set; }


        public int Age { get; set; }

        /// <summary>
        /// Creates a new reminder.
        /// </summary>
        /// <param name="сhatId">Chat ID of the user who saves/receives reminders.</param>
        /// <param name="topic">Reminder subject.</param>
        /// <param name="dateTime">Reminder date.</param>
        /// <exception cref="ArgumentException"></exception>

        public BirthDate()
        {

        }
       
        public BirthDate(long сhatId, string name, DateTime dateOfBirthday, int age)
        {
            #region check for null

            if (сhatId <= 0)
                throw new ArgumentNullException($"\"{nameof(сhatId)}\" cannot be empty or null", nameof(сhatId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException($"\"{nameof(name)}\" cannot be empty or contain only a space.", nameof(name));
            if (dateOfBirthday < DateTime.MinValue)
                throw new ArgumentNullException($"\"{nameof(dateOfBirthday)}\" cannot be empty or default value.", nameof(dateOfBirthday));
            if (age <= 0 && age >= 120)
                throw new ArgumentNullException($"\"{nameof(age)}\" cannot be empty or default value.", nameof(age));

            #endregion

            ChatId = сhatId;
            Name = name;
            DateOfBirthday = dateOfBirthday;
            Age = age;
        }

        public override string PrintData()
        {
            return $"Ім'я: {Name}\nДата народження: {DateOfBirthday}\nВік: {Age}";
        }
    }
}

