using System;
using System.Net.Http;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace ComputerVisionClient
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		public async Task Handle_Clicked(object sender, EventArgs e)
		{
			await CrossMedia.Current.Initialize();

			var pickOrTake = await DisplayActionSheet("Where do you want to pick the image from?", "Cancel", null, new[] { "Album", "Camera" });

			MediaFile file = null;

			switch (pickOrTake)
			{
				case "Camera":
					if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
					{
						await DisplayAlert("No Camera", ":( No camera available.", "OK");
						return;
					}

					file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
					{
						SaveToAlbum = false,
						PhotoSize = PhotoSize.Medium
					});
					break;

				case "Album":
					file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
					{
						PhotoSize = PhotoSize.Medium
					});
					break;

				default:
					return;
			}

			if (file == null)
				return;

			previewImage.Source = ImageSource.FromFile(file.Path);
			selectImageButton.IsVisible = false;
			loadingIndicator.IsVisible = true;

			using (var httpClient = new HttpClient())
			{
				var request = new StreamContent(file.GetStreamWithImageRotatedForExternalStorage());

				var httpResult = await httpClient.PostAsync("https://dnc-testfunction.azurewebsites.net/api/HttpTrigger", request);

				var descriptionResult = await httpResult.Content.ReadAsStringAsync();

				await DisplayAlert("Result", $"I see {descriptionResult}", "Thank you!");
			}

			selectImageButton.IsVisible = true;
			loadingIndicator.IsVisible = false;
		}
	}
}