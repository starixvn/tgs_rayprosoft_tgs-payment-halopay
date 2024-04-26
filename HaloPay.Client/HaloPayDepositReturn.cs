namespace Tgs.Payment.HaloPay.Client;
public record HaloPayDepositReturn
{
    public decimal OrderAmount { get; init; }
    public Guid FormId { get; init; }

    public string Status { get; init; } = default!;

    public string Sign { get; init; } = default!;

    public string MerchantId { get; init; } = default!;
    public string ThirdPartyOrderNo { get; init; } = default!;

    public string CompletionDate { get; init; } = default!;
}