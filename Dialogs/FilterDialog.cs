using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WikiDataHelpDeskBot.CognitiveModels;

namespace WikiDataHelpDeskBot.Dialogs
{
    public class FilterDialog : ComponentDialog
    {
        private readonly FlightBookingRecognizer _luisRecognizer;
        public FilterDialog(FlightBookingRecognizer luisRecognizer)
            : base(nameof(FilterDialog))
        {
            _luisRecognizer = luisRecognizer;
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GiveBackNumberOfElements,
                LoopStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GiveBackNumberOfElements(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var messageText = "5 db ilyen elem van";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> LoopStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<WikiDataHelpDesk>(stepContext.Context, cancellationToken);
            // Retrieve their selection list, the choice they made, and whether they chose to finish.

            switch (luisResult.TopIntent().intent)
            {
                case WikiDataHelpDesk.Intent.Filter:
                    {
                        var attributeName = (luisResult.Entities.CommonAttributes?.FirstOrDefault() ?? luisResult.Entities.AttributeName)?.FirstOrDefault();
                        var attributeValue = luisResult.Entities.datetime?.FirstOrDefault()?.Expressions.FirstOrDefault()?.Split('T')[0] ?? luisResult.Entities.AttributeValue?.FirstOrDefault();
                        var searchParameters = (SearchParameters)stepContext.Options;
                        if (attributeName != null && attributeValue != null)
                        {
                            if (searchParameters != null)
                                searchParameters.Filters.Add(attributeName, attributeValue);
                        }

                        return await stepContext.ReplaceDialogAsync(nameof(FilterDialog), searchParameters, cancellationToken);
                    }
                case WikiDataHelpDesk.Intent.List:
                    {
                        return await stepContext.EndDialogAsync((SearchParameters)stepContext.Options, cancellationToken);
                    }
                case WikiDataHelpDesk.Intent.Cancel:
                    {
                        var cancelMessage = MessageFactory.Text("Cancelling...", "Cancelling...", InputHints.IgnoringInput);
                        await stepContext.Context.SendActivityAsync(cancelMessage, cancellationToken);
                        return await stepContext.EndDialogAsync(null, cancellationToken);
                    }
                case WikiDataHelpDesk.Intent.None:
                default:
                    {
                        return await stepContext.ReplaceDialogAsync(nameof(FilterDialog), (SearchParameters)stepContext.Options, cancellationToken);
                    }
            }
        }
    }
}
