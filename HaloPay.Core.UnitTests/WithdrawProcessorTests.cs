using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.Clients;
using Tgs.Payment.HaloPay;
using Tgs.Payment.HaloPay.Client;
using Tgs.Payment.HaloPay.MessageQueue;
using Tgs.Payment.Protos;

namespace HaloPay.Core.UnitTests;
public class WithdrawProcessorTests
{
    [Fact]
    public async Task 代付()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.halopayipi.com");

        var fakePayClient = new HaloPayClient(
            httpClient,
            new HaloPayHasher(NullLogger<HaloPayHasher>.Instance),
            NullLogger<HaloPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var options = new HaloPayOptions
        {
            CallbackBaseUrl = new Uri("https://dev-payment.rpgp.cc/halopay"),
            PaymentApiUrl = new Uri("https://api.halopayipi.com"),
            PaymentServiceBaseUrl = new Uri("http://localhost:5541")
        };

        var sut = new WithdrawProcessor(
            fakePayClient,
            new OptionsWrapper<HaloPayOptions>(options),
            fakePaymentClient);

        var formId = Guid.Parse("322bf6c7b2de4f2196e450650c57ed39");
        var merchantId = "H0131389964";
        var apiKey = "PRAN12179161511U";

        var applySettings = new HaloPayWithdrawSettings(
            merchantId,
            apiKey,
            formId,
            "687354546545",
            "test",
            "1");

        var request = new WithdrawRequest
        {
            Amount = "100",
            ApplyInfo = JsonSerializer.Serialize(applySettings, HaloPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://api.halopayipi.com/halo/indonesia/payOut")
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "amount": "100",
                    "code": 0,
                    "payOrderNo": "1711783399233540857856117",
                    "merchantId": "H0131389964",
                    "sign": "45CCCCF458174FD197203C7BD5EAB00C",
                    "message": "Success",
                    "merchantOrderNo": "322bf6c7b2de4f2196e450650c57ed39"
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "tgs.payment.halopay.withdraw",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<HaloPayWithdrawApplyInfo>(actual.Result, HaloPayUtility.SerializerOptions);

        // Assert
        _ = fakePaymentClient.Received(1)
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                Arg.Is(applySettings.FormId),
                Arg.Is("1711783399233540857856117"));
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
    }
}
