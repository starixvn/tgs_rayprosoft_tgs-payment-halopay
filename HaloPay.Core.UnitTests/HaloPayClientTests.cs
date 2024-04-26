using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.HaloPay;

namespace HaloPay.Core.UnitTests;
public class HaloPayClientTests
{
    [Fact]
    public async Task 代收_收銀台()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.halopayipi.com");
        var signer = new HaloPayHasher(NullLogger<HaloPayHasher>.Instance);

        var sut = new HaloPayClient(httpClient, signer, NullLogger<HaloPayClient>.Instance);

        var formId = Guid.Parse("7f55a29b93ce43bf9a24b6997716bfcd");
        var merchantId = "H0131389964";
        var apiKey = "PRAN12179161511U";

        _ = mockHttp.Expect(HttpMethod.Post, "https://api.halopayipi.com/halo/indonesia/payIn")
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

        var order = new DepositOrder(
           1000m,
           formId,
           new Uri("https://dev-payment.rpgp.cc/halopay/deposit/callback"),
           "4ucmqplxb029",
           "",
           "08123765216");

        var actual = await sut.DepositAsync(merchantId, apiKey, order);

        Assert.Equal("HA8871783319500664872960022", actual.ThirdPartyOrderNo);
        Assert.Equal(new Uri("https://api.halopayipi.com/cash/?order_code=7f55a29b93ce43bf9a24b6997716bfcd"), actual.RedirectUrl);
    }

    [Fact]
    public async Task 代收訂單查詢_Status1()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.halopayipi.com");
        var signer = new HaloPayHasher(NullLogger<HaloPayHasher>.Instance);

        var sut = new HaloPayClient(httpClient, signer, NullLogger<HaloPayClient>.Instance);

        var formId = Guid.Parse("db7c8883cc0a462d97139b0ee20f3de6");
        var merchantId = "H0131389964";
        var apiKey = "PRAN12179161511U";

        _ = mockHttp.Expect(HttpMethod.Post, "https://api.halopayipi.com/halo/pay/getPayinOrder")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "amount": "1000.00",
                    "code": 0,
                    "payOrderNo": "HA8111783305449167659008762",
                    "merchantId": "H0131389964",
                    "sign": "4C0AFAC8BB84B198B59B9C710D11AC18",
                    "merchantOrderNo": "db7c8883cc0a462d97139b0ee20f3de6",
                    "status": "1"
                }
                """));

        var actual = await sut.QueryDepositFormAsync(merchantId, apiKey, formId);

        Assert.Equal("1", actual);
    }

    [Fact]
    public async Task 代收訂單查詢_Status4()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.halopayipi.com");
        var signer = new HaloPayHasher(NullLogger<HaloPayHasher>.Instance);

        var sut = new HaloPayClient(httpClient, signer, NullLogger<HaloPayClient>.Instance);

        var formId = Guid.Parse("a8c94112fb234a6ba61ec5ca7cf44021");
        var merchantId = "H0131389964";
        var apiKey = "PRAN12179161511U";

        _ = mockHttp.Expect(HttpMethod.Post, "https://api.halopayipi.com/halo/pay/getPayinOrder")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "amount": "500.00",
                    "code": 0,
                    "payOrderNo": "HA2151783326348457549824800",
                    "merchantId": "H0131389964",
                    "sign": "82B8B3DA37D0FA6FE86627A2EC59BAE3",
                    "merchantOrderNo": "a8c94112fb234a6ba61ec5ca7cf44021",
                    "status": "4"
                }
                """));

        var actual = await sut.QueryDepositFormAsync(merchantId, apiKey, formId);

        Assert.Equal("4", actual);
    }

}
