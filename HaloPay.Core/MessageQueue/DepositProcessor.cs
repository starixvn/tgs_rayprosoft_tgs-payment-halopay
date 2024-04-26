using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Payment.HaloPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.HaloPay.MessageQueue;

internal class DepositProcessor(
    HaloPayClient haloPayClient,
    IOptions<HaloPayOptions> optionsAccessor,
    ILogger<DepositProcessor> logger)
    : IMessageProcessor<DepositRequest, DepositResponse>
{
    private readonly HaloPayOptions _options = optionsAccessor.Value;

    public async ValueTask<DepositResponse> HandleAsync(
        string subject,
        DepositRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{DepositApplyInfo}", data.ApplyInfo);

        var settings = JsonSerializer.Deserialize<HaloPayDepositSettings>(
            data.ApplyInfo,
            HaloPayUtility.SerializerOptions)!;

        var order = new DepositOrder(
            decimal.Parse(data.Amount),
            settings.FormId,
            new Uri($"{_options.CallbackBaseUrl}/deposit/callback"),
            settings.AccountName,
            settings.PayType,
            settings.Mobile);

        var depositResponse = await haloPayClient
            .DepositAsync(settings.MerchantId, settings.ApiKey, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new HaloPayDepositApplyInfo(
            settings,
            new HaloPayDepositResponse(
                depositResponse.Amount,
                depositResponse.FormId,
                depositResponse.RedirectUrl,
                depositResponse.ThirdPartyOrderNo));

        return new DepositResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, HaloPayUtility.SerializerOptions)
        };
    }
}