using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace individual
{
    public partial class AdminForm: Form
    {
        private string posterPath = ""; // Путь к выбранному постеру
        private int currentAdminId;

        public AdminForm(int adminId)
        {
            InitializeComponent();
            this.currentAdminId = adminId;
            LoadUsers();
            LoadBookings();
            this.FormClosed += AdminForm_FormClosed; // Завершение приложения при закрытии
        }

        private void AdminForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit(); // Завершаем приложение при закрытии формы
        }

        private void LoadUsers()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    // Изменяем запрос, чтобы исключить администраторов (role != 1)
                    string query = "SELECT id, name, email, login, role FROM Users WHERE role != 1";
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection))
                    {
                        DataTable usersTable = new DataTable();
                        adapter.Fill(usersTable);
                        dataGridView1.DataSource = usersTable;

                        // Настройка заголовков столбцов
                        dataGridView1.Columns["id"].HeaderText = "ID";
                        dataGridView1.Columns["name"].HeaderText = "Имя";
                        dataGridView1.Columns["email"].HeaderText = "Email";
                        dataGridView1.Columns["login"].HeaderText = "Логин";
                        dataGridView1.Columns["role"].HeaderText = "Роль";
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBookings()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    string query = "SELECT id, user_id, session_id, full_name, movie_title, seat, session_time, card_last4 FROM Bookings";
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection))
                    {
                        DataTable bookingsTable = new DataTable();
                        adapter.Fill(bookingsTable);
                        dataGridView2.DataSource = bookingsTable;

                        // Настройка заголовков столбцов
                        dataGridView2.Columns["id"].HeaderText = "ID брони";
                        dataGridView2.Columns["user_id"].HeaderText = "ID пользователя";
                        dataGridView2.Columns["session_id"].HeaderText = "ID сеанса";
                        dataGridView2.Columns["full_name"].HeaderText = "ФИО";
                        dataGridView2.Columns["movie_title"].HeaderText = "Фильм";
                        dataGridView2.Columns["seat"].HeaderText = "Место";
                        dataGridView2.Columns["session_time"].HeaderText = "Время сеанса";
                        dataGridView2.Columns["card_last4"].HeaderText = "Последние 4 цифры карты";
                        dataGridView2.Columns["session_time"].DefaultCellStyle.Format = "HH:mm";
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Ошибка загрузки бронирований: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик кнопки для выбора постера
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif",
                Title = "Выберите изображение постера"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                posterPath = openFileDialog.FileName; // Сохраняем путь к файлу
                pictureBox1.Image = Image.FromFile(posterPath); // Отображаем постер
            }
        }

        // Обработчик кнопки для добавления фильма
        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(textBox3.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("Введите корректную длительность!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверяем формат времени (например, "2025-03-23 14:30")
            if (!DateTime.TryParse(textBox4.Text, out DateTime sessionDateTime))
            {
                MessageBox.Show("Введите корректное время сеанса (например, '2025-03-23 14:30')!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string savedPosterPath = posterPath;
            if (string.IsNullOrEmpty(posterPath))
            {
                savedPosterPath = Path.Combine(Application.StartupPath, "default_poster.jpg");
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();

                    // 1. Добавляем фильм в таблицу Movies
                    string movieQuery = "INSERT INTO Movies (title, description, duration, poster) VALUES (@title, @description, @duration, @poster); SELECT last_insert_rowid();";
                    int movieId;
                    using (SQLiteCommand command = new SQLiteCommand(movieQuery, connection))
                    {
                        command.Parameters.AddWithValue("@title", textBox1.Text);
                        command.Parameters.AddWithValue("@description", textBox2.Text);
                        command.Parameters.AddWithValue("@duration", duration);
                        command.Parameters.AddWithValue("@poster", savedPosterPath);
                        movieId = Convert.ToInt32(command.ExecuteScalar()); // Получаем ID нового фильма
                    }

                    // 2. Добавляем сеанс в таблицу Sessions
                    string sessionQuery = "INSERT INTO Sessions (movie_id, time_film, hall_number, total_seats, available_seats) " +
                                         "VALUES (@movieId, @time_film, @hallNumber, @totalSeats, @availableSeats)";
                    using (SQLiteCommand command = new SQLiteCommand(sessionQuery, connection))
                    {
                        command.Parameters.AddWithValue("@movieId", movieId);
                        command.Parameters.AddWithValue("@time_film", sessionDateTime);
                        command.Parameters.AddWithValue("@hallNumber", 1); // Укажи номер зала (по умолчанию 1)
                        command.Parameters.AddWithValue("@totalSeats", 25); // Укажи общее количество мест (например, 25)
                        command.Parameters.AddWithValue("@availableSeats", 25); // Все места доступны изначально
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Фильм и сеанс успешно добавлены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                pictureBox1.Image = null;
                posterPath = "";
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Ошибка при добавлении: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик кнопки "Удалить пользователя"
        private void button3_Click(object sender, EventArgs e)
        {
            // Проверяем, выделена ли строка
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для удаления.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получаем данные о пользователе из выделенной строки
            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
            int userId = Convert.ToInt32(selectedRow.Cells["id"].Value);
            string userName = selectedRow.Cells["name"].Value.ToString();

            if (userId == currentAdminId)
            {
                MessageBox.Show("Нельзя удалить текущего администратора!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Показываем сообщение с подтверждением
            DialogResult result = MessageBox.Show(
                $"Вы уверены, что хотите удалить пользователя '{userName}'?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                    {
                        connection.Open();

                        // Сначала удаляем все бронирования пользователя из таблицы Bookings
                        string deleteBookingsQuery = "DELETE FROM Bookings WHERE user_id = @userId";
                        using (SQLiteCommand command = new SQLiteCommand(deleteBookingsQuery, connection))
                        {
                            command.Parameters.AddWithValue("@userId", userId);
                            command.ExecuteNonQuery();
                        }

                        // Удаляем пользователя из таблицы Users
                        string deleteUserQuery = "DELETE FROM Users WHERE id = @userId";
                        using (SQLiteCommand command = new SQLiteCommand(deleteUserQuery, connection))
                        {
                            command.Parameters.AddWithValue("@userId", userId);
                            command.ExecuteNonQuery();
                        }

                        MessageBox.Show("Пользователь успешно удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Обновляем таблицы
                        LoadUsers();
                        LoadBookings(); // Обновляем бронирования, так как они тоже могли измениться
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Ошибка при удалении пользователя: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }
    }
}
