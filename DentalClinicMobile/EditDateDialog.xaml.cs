using System;
using Microsoft.Maui.Controls;

namespace DentalClinicMobile
{
    public partial class EditDateDialog : ContentPage
    {
        public DateTime SelectedDate { get; private set; }
        public bool IsConfirmed { get; private set; }

        public EditDateDialog(string title, DateTime? currentDate = null)
        {
            InitializeComponent();

            TitleLabel.Text = title;

            DatePickerControl.MinimumDate = new DateTime(1925, 1, 1);
            DatePickerControl.MaximumDate = DateTime.Today;

            if (currentDate.HasValue)
            {
                DatePickerControl.Date = currentDate.Value;
            }
            else
            {
                DatePickerControl.Date = DateTime.Today;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            SelectedDate = DatePickerControl.Date;
            IsConfirmed = true;
            await Navigation.PopModalAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            IsConfirmed = false;
            await Navigation.PopModalAsync();
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            IsConfirmed = false;
            await Navigation.PopModalAsync();
        }
    }
}