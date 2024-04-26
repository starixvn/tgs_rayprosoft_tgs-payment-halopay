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
public class DepositProcessorTests
{
    [Fact]
    public async Task 儲值要求處理()
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
        var formId = Guid.Parse("7f55a29b93ce43bf9a24b6997716bfcd");
        var merchantId = "H0131389964";
        var apiKey = "PRAN12179161511U";

        var sut = new DepositProcessor(
            fakePayClient,
            new OptionsWrapper<HaloPayOptions>(options),
            NullLogger<DepositProcessor>.Instance);

        var applySettings = new HaloPayDepositSettings(
            merchantId,
            apiKey,
            formId,
            "4ucmqplxb029",
            "",
            "08123765216");

        var request = new DepositRequest
        {
            Amount = "500",
            ApplyInfo = JsonSerializer.Serialize(applySettings, HaloPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://api.halopayipi.com/halo/indonesia/payIn")
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "amount": "1000",
                    "code": 0,
                    "payOrderNo": "HA8871783319500664872960022",
                    "merchantId": "H0131389964",
                    "sign": "889CD941E5A947CCFA17087B62969560",
                    "payUrl": "https://api.halopayipi.com/cash/?order_code=7f55a29b93ce43bf9a24b6997716bfcd",
                    "message": "Success",
                    "merchantOrderNo": "7f55a29b93ce43bf9a24b6997716bfcd"
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "tgs.payment.halopay.deposit",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<HaloPayDepositApplyInfo>(actual.Result, HaloPayUtility.SerializerOptions);

        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
        Assert.Equal(
            new HaloPayDepositResponse(
                1000m,
                applySettings.FormId,
                new Uri("https://api.halopayipi.com/cash/?order_code=7f55a29b93ce43bf9a24b6997716bfcd"),
                "HA8871783319500664872960022"),
            applyInfo.Response);
    }
}