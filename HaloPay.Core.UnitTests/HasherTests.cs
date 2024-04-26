using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Payment.HaloPay;

namespace HaloPay.Core.UnitTests;

public class HasherTests
{
    [Fact]
    public void 代收_QRIS()
    {
        var apiKey = "PRAN12179161511U";

        var dict = new Dictionary<string, string?>
        {
            ["merchantId"] = "H0131389964",
            ["merchantOrderNo"] = "db7c8883cc0a462d97139b0ee20f3de6",
            ["amount"] = "1000",
            ["accountName"] = "4ucmqplxb029",
            ["desc"] = "",
            ["payType"] = "QRIS",
            ["mobile"] = "08123765216",
            ["notificationUrl"] = "https://dev-payment.rpgp.cc/halopay/deposit/callback",
        };
        var expected = "BE92F8EAAACF4A8A4B34E3756D20AB5D";

        var sut = new HaloPayHasher(NullLogger<HaloPayHasher>.Instance);

        var actual = sut.ComputeHash(apiKey, dict);

        Assert.Equal(expected, actual);
    }
    [Fact]
    public void 代收_收銀台()
    {
        var apiKey = "PRAN12179161511U";

        var dict = new Dictionary<string, string?>
        {
            ["merchantId"] = "H0131389964",
            ["merchantOrderNo"] = "7f55a29b93ce43bf9a24b6997716bfcd",
            ["amount"] = "1000",
            ["accountName"] = "4ucmqplxb029",
            ["desc"] = "",
            ["payType"] = "",
            ["mobile"] = "08123765216",
            ["notificationUrl"] = "https://dev-payment.rpgp.cc/halopay/deposit/callback",
        };
        var expected = "8D9F8623EECC9AAE6934695F2AB6F004";

        var sut = new HaloPayHasher(NullLogger<HaloPayHasher>.Instance);

        var actual = sut.ComputeHash(apiKey, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 代收訂單查詢()
    {
        var apiKey = "PRAN12179161511U";

        var dict = new Dictionary<string, string?>
        {
            ["merchantId"] = "H0131389964",
            ["merchantOrderNo"] = "db7c8883cc0a462d97139b0ee20f3de6",
        };
        var expected = "BD5BDFCA076E0C458A5D61FC6A263EF9";

        var sut = new HaloPayHasher(NullLogger<HaloPayHasher>.Instance);

        var actual = sut.ComputeHash(apiKey, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 代付()
    {
        var apiKey = "PRAN12179161511U";

        var dict = new Dictionary<string, string?>
        {
            ["merchantId"] = "H0131389964",
            ["desc"] = "",
            ["merchantOrderNo"] = "df74cd3022b84ed0ac5c386573ce7647",
            ["amount"] = "100",
            ["accountNumber"] = "687354546545",
            ["accountName"] = "test",
            ["notificationUrl"] = "https://dev-payment.rpgp.cc/halopay/withdraw/callback",
            ["bankId"] = "TCB",            
        };
        var expected = "C315AE349BF4E7FBBE7AAD22E36475EF";

        var sut = new HaloPayHasher(NullLogger<HaloPayHasher>.Instance);

        var actual = sut.ComputeHash(apiKey, dict);

        Assert.Equal(expected, actual);
    }

}