using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.Payment.HaloPay.ApplicationServices;
using Tgs.Payment.HaloPay.Client;
using Tgs.Payment.HaloPay.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.HaloPay.Controllers;

[Route("[controller]")]
[ApiController]
public class DepositController(ILogger<DepositController> logger) : ControllerBase
{
    private readonly ILogger<DepositController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Consumes("application/json")]
    [Produces(MediaTypeNames.Text.Plain)]
    [HttpPost("callback")]
    public async Task<string> DepositCallbackAsync(
       [FromServices] IGetService<GetDepositFormQuery, DepositForm?> getService,
       [FromServices] IPaymentServiceClient paymentServiceClient,
       [FromServices] IPaymentClient paymentClient,
       [FromServices] HaloPayHasher hasher,
       [FromBody] CallbackDepositViewModel source)    
    {
        _logger.LogInformation("{source}", source);

        var formId = Guid.Parse(source.MerchantOrderNo);

        var depositForm = await getService
            .GetAsync(new(formId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (depositForm == null)
        {
            _logger.LogWarning("Can't found deposit form({id}).", formId);
            return "fail";
        }

        var apiKey = depositForm.ApplyInfo!.Settings.ApiKey;        

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(source))!;

        var sign = source.Sign;
        var hash = hasher.ComputeHash(
            apiKey,
            dict.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value.ToString())));

        _logger.LogInformation("Compute hash: {hash}, sign: {sign}", hash, sign);

        if (sign != hash)
        {
            _logger.LogWarning("Sign value invalid. input: {sign}, compute: {hash}", sign, hash);
            return "fail";
        }

        var paymentReturn = new HaloPayDepositReturn()
        {
            OrderAmount = decimal.Parse(source.Amount),
            FormId = formId,
            Status = source.Status,
            Sign = source.Sign,
            MerchantId = source.MerchantId,
            ThirdPartyOrderNo = source.PayOrderNo,
            CompletionDate = source.CompletionDate,
        };

        var fsm = PaymentStateMachineFactory.CreateDepositStateMachine(depositForm.Status);

        switch (paymentReturn.Status)
        {
            case "0"://成功
                if (fsm.CanFire(PaymentTrigger.PaymentDepositCompleted))
                {
                    await fsm.FireAsync(PaymentTrigger.PaymentDepositCompleted).ConfigureAwait(false);

                    await paymentServiceClient.SubmitDepositFormStatusAsync(
                        depositForm.Id,
                        paymentReturn: JsonSerializer.Serialize(paymentReturn, HaloPayUtility.SerializerOptions),
                        cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

                    await paymentClient.ApproveDepositAsync(
                        depositForm.Id,
                        depositForm.BrandId,
                        paymentReturn.OrderAmount,
                        HttpContext.RequestAborted).ConfigureAwait(false);
                }

                break;

            case "2"://失敗
                if (fsm.CanFire(PaymentTrigger.Reject))
                {
                    await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

                    await paymentServiceClient.SubmitDepositFormStatusAsync(
                        depositForm.Id,
                        paymentReturn: JsonSerializer.Serialize(
                            paymentReturn,
                            HaloPayUtility.SerializerOptions),
                        cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

                    await paymentClient.RejectDepositAsync(
                        depositForm.Id,
                        depositForm.BrandId,
                        HttpContext.RequestAborted).ConfigureAwait(false);
                }

                break;

            default:
                break;
        }

        return "SUCCESS";
    }
}