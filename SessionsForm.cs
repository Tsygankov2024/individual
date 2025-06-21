using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace individual
{
    public partial class SessionsForm : Form
    {
        private int userId;
        private int movieId;
        private int sessionId;
        private HashSet<string> selectedSeats = new HashSet<string>(); // Коллекция для хранения выбранных мест
        private string movieTitle;
        private string sessionTime;

        public SessionsForm(int userId, int movieId, int sessionId, string movieTitle, bool isAdmin)
        {
            InitializeComponent();
            this.userId = userId;
            this.movieId = movieId;
            this.sessionId = sessionId;
            this.movieTitle = movieTitle;

            label1.Text = movieTitle;

            sessionTime = DatabaseHelper.GetSessionTime(sessionId); // Сохраняем оригинальное значение
            if (!string.IsNullOrEmpty(sessionTime) && sessionTime != "Неизвестно")
            {
                DateTime dateTime = DateTime.Parse(sessionTime);
                label2.Text = dateTime.ToString("HH:mm"); // Форматируем только для отображения
            }
            else
            {
                label2.Text = "Неизвестно";
            }

            LoadSeats();
        }

        private void Sessions_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit(); // Завершаем приложение при закрытии формы
        }

        private void LoadSeats()
        {
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.ColumnCount = 5;

            // Устанавливаем фиксированный размер для строк и столбцов
            for (int i = 0; i < 5; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F)); // 20% для каждой строки
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // 20% для каждого столбца
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    string query = "SELECT seat FROM Bookings WHERE session_id = @sessionId";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sessionId", sessionId);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            HashSet<string> bookedSeats = new HashSet<string>();
                            while (reader.Read())
                            {
                                bookedSeats.Add(reader.GetString(0));
                            }

                            for (int row = 0; row < 5; row++)
                            {
                                for (int col = 0; col < 5; col++)
                                {
                                    string seat = $"{row + 1}-{col + 1}";
                                    Button seatButton = new Button
                                    {
                                        Text = seat,
                                        Dock = DockStyle.Fill,
                                        Tag = seat,
                                        BackColor = bookedSeats.Contains(seat) ? Color.Gray : Color.Green,
                                        Enabled = !bookedSeats.Contains(seat),
                                        Font = new Font("Arial", 8, FontStyle.Bold),
                                        Margin = new Padding(5) // Добавляем отступы для красоты
                                    };

                                    seatButton.Click += tableLayoutPanel1_Click;
                                    tableLayoutPanel1.Controls.Add(seatButton, col, row);
                                }
                            }
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Ошибка загрузки мест: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tableLayoutPanel1_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            string seat = clickedButton.Tag.ToString();

            if (clickedButton.BackColor == Color.Green)
            {
                clickedButton.BackColor = Color.Blue;
                selectedSeats.Add(seat);
            }
            else if (clickedButton.BackColor == Color.Blue)
            {
                clickedButton.BackColor = Color.Green;
                selectedSeats.Remove(seat);
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            // Переходим обратно на форму оплаты, передавая выбранные места
            MoviesForm moviesForm = new MoviesForm(userId);
            moviesForm.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> selectedSeatsList = new List<string>();
            // Перебираем все кнопки мест
            foreach (Control ctrl in tableLayoutPanel1.Controls)
            {
                if (ctrl is Button seatButton && seatButton.BackColor == Color.Blue)
                {
                    selectedSeatsList.Add(seatButton.Tag.ToString());
                }
            }

            // Если выбраны места
            if (selectedSeatsList.Count > 0)
            {
                PaymentForm paymentForm = new PaymentForm(userId, sessionId, movieId, selectedSeatsList, movieTitle, label2.Text, false); // isAdmin = false для пользователя
                paymentForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы одно место.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        private void button2_Click(object sender, EventArgs e)
        {
            MoviesForm moviesForm = new MoviesForm(userId);
            moviesForm.Show();
            this.Hide();
        }
    }
}