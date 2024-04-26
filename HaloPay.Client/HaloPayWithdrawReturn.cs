namespace Tgs.Payment.HaloPay.Client;

public class HaloPayWithdrawReturn
{
    public decimal AfterExchangeAmount { get; init; }

    public decimal Amount { get; init; }

    public Guid FormId { get; init; }

    public string Status { get; init; } = default!;

    public string Sign { get; init; } = default!;

    public string CompletionDate { get; init; } = default!;

    public string MerchantId { get; init; } = default!;

    public string ThirdPartyOrderNo { get; init; } = default!;
}