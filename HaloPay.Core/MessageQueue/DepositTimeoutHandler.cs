using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.Payment.HaloPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.HaloPay.MessageQueue;

internal class DepositTimeoutHandler(
    IPaymentServiceClient paymentServiceClient,
    IPaymentClient paymentClient,
    HaloPayClient haloPayClient,
    ILogger<DepositTimeoutHandler> logger)
    : IMessageHandler<ScheduleRequest>
{
    public async ValueTask HandleAsync(
        string subject,
        ScheduleRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Deposit Schedule Handler Receive request Form Id:{FormId}, "
                + "Schedule Time:{ScheduleTime:yyyy/MM/dd HH:mm:ss}",
            data.FormId.ToGuid(),
            data.ScheduleTime.ToDateTime());

        var formId = data.FormId.ToGuid();

        try
        {
            var depositForm = await paymentServiceClient
                .GetDepositFormAsync(formId, cancellationToken)
                .ConfigureAwait(false);

            if (depositForm is null)
            {
                logger.LogWarning("Deposit Form {FormId} not found.", data.FormId.ToGuid());
                return;
            }

            var brandId = depositForm.BrandId;

            logger.LogInformation(
                "GetDepositFormAsync Form Id:{DepositFormId}, Brand Id:{BrandId}, Status:{DepositFormStatus}",
                depositForm.Id,
                brandId,
                depositForm.Status);

            if (depositForm.Status >= PaymentFlowState.Reject)
                return;

            var applyInfo = JsonSerializer.Deserialize<HaloPayDepositApplyInfo>(depositForm.ApplyInfo!, HaloPayUtility.SerializerOptions)!;

            var result = await haloPayClient.QueryDepositFormAsync(
                applyInfo.Settings.MerchantId,
                applyInfo.Settings.ApiKey,
                formId,
                cancellationToken).ConfigureAwait(false);

            if (result is "2")
                await paymentClient
                    .RejectDepositAsync(formId, brandId, cancellationToken)
                    .ConfigureAwait(false);
            else if (result is "1")
                await paymentClient.ScheduleAsync(
                    "tgs.payment.halopay.deposit.schedule.reject",
                    formId,
                    DateTime.UtcNow.AddMinutes(10D),
                    cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Process deposit timeout error!");

            await paymentClient.ScheduleAsync(
                "tgs.payment.halopay.deposit.schedule.reject",
                formId,
                DateTime.UtcNow.AddMinutes(10D),
                cancellationToken).ConfigureAwait(false);
        }
    }
}