namespace DentalClinicMobile
{
    public partial class DiagnosisDialog : ContentPage
    {
        public string Diagnosis { get; private set; }
        public string Treatment { get; private set; }
        public bool IsConfirmed { get; private set; }

        public DiagnosisDialog()
        {
            InitializeComponent();
            DiagnosisEditor.Focus();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DiagnosisEditor.Text))
            {
                await DisplayAlert("Ошибка", "Поле диагноза обязательно для заполнения", "OK");
                return;
            }

            Diagnosis = DiagnosisEditor.Text.Trim();
            Treatment = TreatmentEditor.Text?.Trim() ?? "";
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