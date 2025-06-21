using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace individual
{
    public partial class ConfirmationForm: Form
    {
        private string userFullName;
        private string movieTitle;
        private string sessionTime;
        private List<string> selectedSeats;
        private int userId;
        private bool isAdmin;


        public ConfirmationForm(string userFullName, string movieTitle, string sessionTime, List<string> selectedSeats, int userId, bool isAdmin)
        {
            InitializeComponent();

            this.userFullName = userFullName;
            this.movieTitle = movieTitle;
            this.sessionTime = sessionTime;
            this.selectedSeats = selectedSeats;
            this.userId = userId;
            this.isAdmin = isAdmin;

            DisplayBookingDetails();

        }

        // Метод для отображения информации о бронировании
        private void DisplayBookingDetails()
        {
            label2.Text = $"Фильм: {movieTitle}";
            label3.Text = $"Время сеанса: {sessionTime}";
            label4.Text = $"Вы выбрали следующие места: {string.Join(", ", selectedSeats)}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MoviesForm moviesForm = new MoviesForm(userId);
            moviesForm.Show();
            this.Hide();

        }
    }
}
