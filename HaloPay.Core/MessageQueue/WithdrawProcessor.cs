using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.Clients;
using Tgs.Payment.HaloPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.HaloPay.MessageQueue;

internal class WithdrawProcessor(
    HaloPayClient haloPayClient,
    IOptions<HaloPayOptions> optionsAccessor,
    IPaymentClient paymentClient)
    : IMessageProcessor<WithdrawRequest, WithdrawResponse>
{
    private readonly HaloPayClient _haloPayClient = haloPayClient ?? throw new ArgumentNullException(nameof(haloPayClient));
    private readonly IPaymentClient _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
    private readonly HaloPayOptions _options = optionsAccessor.Value;

    public async ValueTask<WithdrawResponse> HandleAsync(
        string subject,
        WithdrawRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<HaloPayWithdrawSettings>(data.ApplyInfo, HaloPayUtility.SerializerOptions)!;

        var order = new WithdrawOrder(
            settings.FormId,
            decimal.Parse(data.Amount),
            new Uri($"{_options.CallbackBaseUrl}/withdraw/callback"),
            settings.AccountNumber,
            settings.AccountName,
            settings.BankId);

        var withdrawResponse = await _haloPayClient
            .WithdrawAsync(settings.MerchantId, settings.ApiKey, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new HaloPayWithdrawApplyInfo(
            settings,
            new HaloPayWithdrawResponse(withdrawResponse.FormId, withdrawResponse.ThirdPartyOrderNo));

        await _paymentClient
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                settings.FormId,
                withdrawResponse.ThirdPartyOrderNo,
                cancellationToken)
            .ConfigureAwait(false);

        return new WithdrawResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, HaloPayUtility.SerializerOptions)
        };
    }
}