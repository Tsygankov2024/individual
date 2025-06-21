using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace individual
{
    public partial class AddMovieForm: Form
    {
        private string posterPath = "";


        public AddMovieForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif",
                Title = "Выберите изображение постера"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                posterPath = openFileDialog.FileName; // Получаем путь к файлу
                pictureBox1.Image = Image.FromFile(posterPath); // Отображаем изображение
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(textBox3.Text) || string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(textBox3.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("Введите корректную длительность!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string savedPosterPath = posterPath;
            if (!string.IsNullOrEmpty(posterPath))
            {
                savedPosterPath = posterPath; // Сохраняем полный путь к файлу
            }
            else
            {
                savedPosterPath = Path.Combine(Application.StartupPath, "default_poster.jpg"); // Относительный путь
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    string query = "INSERT INTO Movies (title, description, duration, poster, time) VALUES (@title, @description, @duration, @poster, @time)";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@title", textBox1.Text);
                        command.Parameters.AddWithValue("@description", textBox2.Text);
                        command.Parameters.AddWithValue("@duration", duration);
                        command.Parameters.AddWithValue("@poster", savedPosterPath);
                        command.Parameters.AddWithValue("@time", textBox4.Text); // Сохраняем время сеанса
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Фильм успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при добавлении фильма: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
