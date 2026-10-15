namespace VanderBeer.App;

public partial class MainPage : ContentPage
{
	public MainPage(VanderBeer.App.ViewModels.MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
