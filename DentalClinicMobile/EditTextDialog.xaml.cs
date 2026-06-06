using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace DentalClinicMobile
{
    public partial class EditTextDialog : ContentPage
    {
        private TaskCompletionSource<string> _taskCompletionSource;

        public EditTextDialog(string title, string currentText, string placeholder = "")
        {
            InitializeComponent();
            TitleLabel.Text = title;
            InputEntry.Text = currentText;
            InputEntry.Placeholder = placeholder;

            _taskCompletionSource = new TaskCompletionSource<string>();

            InputEntry.Focus();
            InputEntry.CursorPosition = 0;
            InputEntry.SelectionLength = InputEntry.Text?.Length ?? 0;
        }

        public Task<string> ShowDialogAsync()
        {
            return _taskCompletionSource.Task;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputEntry.Text))
            {
                await DisplayAlert("Ошибка", "Поле не может быть пустым", "OK");
                return;
            }
            _taskCompletionSource.TrySetResult(InputEntry.Text.Trim());
            await Navigation.PopModalAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            _taskCompletionSource.TrySetResult(null);
            await Navigation.PopModalAsync();
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            _taskCompletionSource.TrySetResult(null);
            await Navigation.PopModalAsync();
        }
    }
}