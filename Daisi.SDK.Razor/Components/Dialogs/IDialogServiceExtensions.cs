using MudBlazor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Razor.Components.Dialogs
{
    public static class IDialogServiceExtensions
    {
        extension(IDialogService dialogService) {

            public async Task<bool> ConfirmAsync(string title, string message, Color color = Color.Info)
            {
                var parameters = new DialogParameters<ConfirmDialog>
                {
                    { x => x.ContentText, message },
                    { x => x.Color, color }
                };

                var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

                var response = await dialogService.ShowAsync<ConfirmDialog>(title, parameters, options);
                if (response is null || response.Result is null) return false;
                return !(await response.Result).Canceled;
            }

            public async Task<string?> PromptAsync(string title, string message, Color color = Color.Default, string? firstValue = null)
            {
                var parameters = new DialogParameters<PromptDialog>
                {
                    { x => x.ContentText, message },
                    { x => x.Color, color },
                    { x => x.Input, firstValue },

                };
                var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

                var response = await dialogService.ShowAsync<PromptDialog>(title, parameters, options);
                if (response is null || response.Result is null) return null;
                return (string)(await response.Result).Data;
            }
        }
        
    }
}
