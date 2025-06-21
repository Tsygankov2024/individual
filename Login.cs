using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace individual
{
    public partial class Login : Form
    {
        private string connectionString = DatabaseHelper.GetConnectionString();

        public Login()
        {
            InitializeComponent();
            this.FormClosed += Login_FormClosed;
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit(); // Завершаем приложение при закрытии формы
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            if (string.IsNullOrWhiteSpace(login) || login.Length < 3)
            {
                MessageBox.Show("Логин должен быть не менее 3 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 3)
            {
                MessageBox.Show("Пароль должен быть не менее 3 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId;
            int role = ValidateUser(login, password, out userId); // Теперь получаем userId

            if (role != -1)
            {
                MessageBox.Show("Вход выполнен успешно!");

                if (role == 1)
                {
                    MessageBox.Show("Вы вошли как администратор");
                    AdminForm adminForm = new AdminForm(userId);
                    adminForm.Show();
                }
                else
                {
                    MessageBox.Show("Вы вошли как пользователь");
                    bool isAdmin = (role == 1);
                    MoviesForm moviesForm = new MoviesForm(userId);
                    moviesForm.Show();
                }
                this.Hide();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int ValidateUser(string login, string password, out int userId)
        {
            userId = -1;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT id, role FROM Users WHERE login = @login AND password = @password";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);
                        command.Parameters.AddWithValue("@password", password);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userId = reader.GetInt32(0); // Получаем ID пользователя
                                return reader.GetInt32(1);  // Получаем роль (0 - пользователь, 1 - админ)
                            }
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Ошибка при работе с базой данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return -1; // Ошибка аутентификации
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Sign_Up signUp = new Sign_Up();
            signUp.Show();
            this.Hide();
        }
    }
}
