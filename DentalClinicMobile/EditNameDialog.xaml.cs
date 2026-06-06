using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace DentalClinicMobile
{
    public partial class EditNameDialog : ContentPage
    {
        private TaskCompletionSource<(string lastName, string firstName, string middleName)> _taskCompletionSource;

        public EditNameDialog(string currentFullName)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(currentFullName))
            {
                var nameParts = currentFullName.Split(' ');
                if (nameParts.Length >= 1) LastNameEntry.Text = nameParts[0];
                if (nameParts.Length >= 2) FirstNameEntry.Text = nameParts[1];
                if (nameParts.Length >= 3) MiddleNameEntry.Text = nameParts[2];
            }

            _taskCompletionSource = new TaskCompletionSource<(string, string, string)>();
            LastNameEntry.Focus();
        }

        public Task<(string lastName, string firstName, string middleName)> ShowDialogAsync()
        {
            return _taskCompletionSource.Task;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastNameEntry.Text) || string.IsNullOrWhiteSpace(FirstNameEntry.Text))
            {
                await DisplayAlert("Ошибка", "Фамилия и имя обязательны для заполнения", "OK");
                return;
            }

            _taskCompletionSource.TrySetResult((
                LastNameEntry.Text?.Trim() ?? "",
                FirstNameEntry.Text?.Trim() ?? "",
                MiddleNameEntry.Text?.Trim() ?? ""
            ));
            await Navigation.PopModalAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            _taskCompletionSource.TrySetResult((null, null, null));
            await Navigation.PopModalAsync();
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            _taskCompletionSource.TrySetResult((null, null, null));
            await Navigation.PopModalAsync();
        }
    }
}