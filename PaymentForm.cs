using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace individual
{
    public partial class PaymentForm : Form
    {
        private int userId;
        private int sessionId;
        private int movieId;
        private List<string> selectedSeats;
        private string movieTitle;
        private string sessionTime;
        private bool isAdmin;

        // Конструктор принимает данные о фильме, сеансе и местах
        public PaymentForm(int userId, int sessionId, int movieId, List<string> selectedSeats, string movieTitle, string sessionTime, bool isAdmin)
        {
            InitializeComponent();
            this.userId = userId;
            this.sessionId = sessionId;
            this.movieId = movieId;
            this.selectedSeats = selectedSeats;
            this.movieTitle = movieTitle;
            this.sessionTime = sessionTime;
            this.isAdmin = isAdmin;

            this.FormClosed += PaymentForm_FormClosed;
            DisplayBookingDetails();
        }

        private void PaymentForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void DisplayBookingDetails()
        {
            label10.Text = $"Название фильма: {movieTitle}";
            label11.Text = "Время сеанса: " + DateTime.Parse(sessionTime).ToString("HH:mm");
            label12.Text = "Вы выбрали следующие места: " + string.Join(", ", selectedSeats);
        }

        private string GetCardLast4()
        {
            string cardNumber = textBox1.Text;
            if (cardNumber.Length >= 4)
            {
                return cardNumber.Substring(cardNumber.Length - 4);
            }
            return "Неизвестно";
        }

        // Метод для записи бронирования в базу данных
        private void SaveBookingToDatabase(string seat)
        {
            string cardLast4 = GetCardLast4();
            string fullName = DatabaseHelper.GetUserFullName(userId);

            DateTime sessionDateTime;
            try
            {
                sessionDateTime = DateTime.Parse(sessionTime); // Теперь sessionTime в формате "yyyy-MM-dd HH:mm:ss"
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат времени сеанса. Ожидается формат 'yyyy-MM-dd HH:mm:ss'.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                try
                {
                    connection.Open();

                    string query = "INSERT INTO Bookings (user_id, session_id, seat, card_last4, full_name, movie_title, session_time) " +
                                   "VALUES (@userId, @sessionId, @seat, @cardLast4, @fullName, @movieTitle, @sessionTime)";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@sessionId", sessionId);
                        command.Parameters.AddWithValue("@seat", seat);
                        command.Parameters.AddWithValue("@cardLast4", cardLast4);
                        command.Parameters.AddWithValue("@fullName", fullName);
                        command.Parameters.AddWithValue("@movieTitle", movieTitle);
                        command.Parameters.AddWithValue("@sessionTime", sessionDateTime);
                        command.ExecuteNonQuery();
                    }

                    string updateSeatsQuery = "UPDATE Sessions SET available_seats = available_seats - 1 WHERE id = @sessionId";
                    using (SQLiteCommand command = new SQLiteCommand(updateSeatsQuery, connection))
                    {
                        command.Parameters.AddWithValue("@sessionId", sessionId);
                        command.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Произошла ошибка при бронировании места: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Метод для обработки кнопки "Оплатить"
        private void button1_Click(object sender, EventArgs e)
        {
            // Проверка на заполнение данных карты
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Пожалуйста, введите номер карты.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBox1.Text.Length != 16 || !long.TryParse(textBox1.Text, out _))
            {
                MessageBox.Show("Номер карты должен состоять из 16 цифр.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var seat in selectedSeats)
            {
                SaveBookingToDatabase(seat);
            }

            MessageBox.Show("Оплата прошла успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

            string fullName = DatabaseHelper.GetUserFullName(userId);
            ConfirmationForm confirmationForm = new ConfirmationForm(fullName, movieTitle, sessionTime, selectedSeats, userId, isAdmin);
            confirmationForm.Show();
            this.Hide();
        }


        // Назад
        private void button2_Click(object sender, EventArgs e)
        {
            SessionsForm sessionsForm = new SessionsForm(userId, movieId, sessionId, movieTitle, isAdmin);
            sessionsForm.Show();
            this.Hide();
        }
    }
}
