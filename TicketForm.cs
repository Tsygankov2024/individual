using System;
using System.Data;
using System.Drawing;
using System.Data.SQLite;
using System.Windows.Forms;

namespace individual
{
    public partial class TicketForm: Form
    {
        private int userId;

        public TicketForm(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.FormClosed += Ticket_FormClosed;
            LoadTickets();
        }

        private void Ticket_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit(); // Завершаем приложение при закрытии формы
        }

        private void LoadTickets()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    // Добавляем id и session_id в запрос
                    string query = "SELECT id, movie_title, seat, session_time, session_id FROM Bookings WHERE user_id = @userId";
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@userId", userId);
                        DataTable ticketsTable = new DataTable();
                        adapter.Fill(ticketsTable);
                        dataGridView1.DataSource = ticketsTable;

                        if (ticketsTable.Rows.Count == 0)
                        {
                            MessageBox.Show("У вас пока нет купленных билетов.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        // Настройка заголовков столбцов
                        dataGridView1.Columns["id"].HeaderText = "ID брони";
                        dataGridView1.Columns["movie_title"].HeaderText = "Фильм";
                        dataGridView1.Columns["seat"].HeaderText = "Место";
                        dataGridView1.Columns["session_time"].HeaderText = "Время сеанса";
                        dataGridView1.Columns["session_id"].HeaderText = "ID сеанса";
                        dataGridView1.Columns["session_time"].DefaultCellStyle.Format = "HH:mm"; // Форматируем время

                        // Скрываем столбцы id и session_id
                        dataGridView1.Columns["id"].Visible = false;
                        dataGridView1.Columns["session_id"].Visible = false;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Ошибка загрузки билетов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите билет для возврата.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Вы уверены, что хотите вернуть {dataGridView1.SelectedRows.Count} билет(ов)?",
                "Подтверждение возврата",
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

                        foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                        {
                            int bookingId = Convert.ToInt32(row.Cells["id"].Value);
                            int sessionId = Convert.ToInt32(row.Cells["session_id"].Value);

                            DateTime sessionTime = Convert.ToDateTime(row.Cells["session_time"].Value);
                            if (sessionTime < DateTime.Now)
                            {
                                MessageBox.Show($"Нельзя вернуть билет на фильм '{row.Cells["movie_title"].Value}', так как сеанс уже начался.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }

                            string deleteQuery = "DELETE FROM Bookings WHERE id = @bookingId";
                            using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                            {
                                command.Parameters.AddWithValue("@bookingId", bookingId);
                                command.ExecuteNonQuery();
                            }

                            string updateSeatsQuery = "UPDATE Sessions SET available_seats = available_seats + 1 WHERE id = @sessionId";
                            using (SQLiteCommand command = new SQLiteCommand(updateSeatsQuery, connection))
                            {
                                command.Parameters.AddWithValue("@sessionId", sessionId);
                                command.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Билеты успешно возвращены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadTickets();
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Ошибка при возврате билетов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
