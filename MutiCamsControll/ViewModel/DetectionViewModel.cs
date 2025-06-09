using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Drawing;
using MultiCamsControl.message;
using MultiCamsControl.Service;
using MultiCamsControl.Service.Interfaces;

namespace MultiCamsControl.ViewModel
{
    public class DetectionViewModel

    {
        private readonly IInferenceApiService _api;

        public ObservableCollection<RecordResult> Predictions { get; } = new();
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }

        public DetectionViewModel(IInferenceApiService api)
        {
            _api = api;

            WeakReferenceMessenger.Default.Register<DetectionViewModel, ImagesCapturedMessage>(
                recipient: this,
                handler: async (viewModel, message) =>
                {
                    var first = message.Value.FirstOrDefault();
                    if (first != null)
                    {
                        await viewModel.RunInferenceAsync(first, "SO001", new[] { "class1" });
                    }
                });
        }

        public async Task RunInferenceAsync(Bitmap bitmap, string shopOrder, IEnumerable<string> expectedClasses, CancellationToken ct = default)
        {
            Predictions.Clear();
            IsSuccess = false;
            ErrorMessage = string.Empty;

            var input = new YoloImageInput(bitmap, shopOrder, expectedClasses.ToList());
            var result = await _api.DetectAsync(input, ct);

            if (result.IsSuccess && result.Data != null)
            {
                IsSuccess = true;
                foreach (var r in result.Data.Predictions)
                    Predictions.Add(r);
            }
            else
            {
                IsSuccess = false;
                ErrorMessage = result.Message ?? "Unknown error";
            }
        }

        public async Task<YoloResult<YoloLabelResult>> RunInferenceAsync(YoloImageInput input, CancellationToken ct = default)
        {
            Predictions.Clear();
            IsSuccess = false;
            ErrorMessage = string.Empty;

            var result = await _api.DetectAsync(input, ct);

            if (result.IsSuccess && result.Data != null)
            {
                IsSuccess = true;
                foreach (var r in result.Data.Predictions)
                    Predictions.Add(r);
            }
            else
            {
                IsSuccess = false;
                ErrorMessage = result.Message ?? "Unknown error";
            }

            return result;
        }


    }
}
