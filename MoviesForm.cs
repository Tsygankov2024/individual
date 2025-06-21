using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace individual
{
    public partial class MoviesForm : Form
    {
        private int userId;

        public MoviesForm(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadMovies();
            this.FormClosed += Movie_FormClosed;
        }

        private void Movie_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit(); // Завершаем приложение при закрытии формы
        }

        private void LoadMovies()
        {
            flowLayoutPanel1.Controls.Clear();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    string query = "SELECT id, title, description, duration, poster FROM Movies";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int movieId = reader.GetInt32(0);
                                string title = reader.GetString(1);
                                string description = reader.GetString(2);
                                int duration = reader.GetInt32(3);
                                string posterPath = reader.IsDBNull(4) ? null : reader.GetString(4);

                                Panel moviePanel = CreateMoviePanel(movieId, title, description, duration, posterPath);
                                flowLayoutPanel1.Controls.Add(moviePanel);
                            }
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Ошибка загрузки фильмов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CreateMoviePanel(int movieId, string title, string description, int duration, string posterPath)
        {
            Panel panel = new Panel
            {
                Size = new Size(250, 400),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Tag = movieId // Присваиваем ID фильма панели
            };

            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(250, 150),
                Dock = DockStyle.Top,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            try
            {
                if (!string.IsNullOrEmpty(posterPath) && File.Exists(posterPath))
                {
                    pictureBox.Image = Image.FromFile(posterPath);
                }
                else
                {
                    pictureBox.Image = Image.FromFile(Path.Combine(Application.StartupPath, "default_poster.jpg"));
                }
            }
            catch (FileNotFoundException)
            {
                pictureBox.Image = null; // Если постер не найден, оставляем пустым
            }

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            Label lblDescription = new Label
            {
                Text = description.Length > 80 ? description.Substring(0, 80) + "..." : description,
                Font = new Font("Arial", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Top,
                Height = 60
            };

            Label lblDuration = new Label
            {
                Text = $"Длительность: {duration} мин",
                Font = new Font("Arial", 10, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30
            };

            Button btnSelect = new Button
            {
                Text = "Выбрать",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.LightBlue
            };
            btnSelect.Click += (sender, e) => OpenSessionsForm(movieId);

            panel.Controls.Add(btnSelect);
            panel.Controls.Add(lblDuration);
            panel.Controls.Add(lblDescription);
            panel.Controls.Add(lblTitle);
            panel.Controls.Add(pictureBox);

            return panel;
        }

        private void OpenSessionsForm(int movieId)
        {
            int sessionId = GetSessionIdForMovie(movieId);
            if (sessionId == -1)
            {
                MessageBox.Show("Для этого фильма нет доступных сеансов.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string selectedMovieTitle = DatabaseHelper.GetMovieTitle(movieId);
            SessionsForm sessionsForm = new SessionsForm(userId, movieId, sessionId, selectedMovieTitle, false); // isAdmin = false
            sessionsForm.Show();
            this.Hide();
        }

        private int GetSessionIdForMovie(int movieId)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    string query = "SELECT id FROM Sessions WHERE movie_id = @movieId LIMIT 1"; // Берем первый сеанс
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@movieId", movieId);
                        object result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Ошибка получения сеанса: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TicketForm ticketForm = new TicketForm(userId);
            ticketForm.Show();
        }
    }
}