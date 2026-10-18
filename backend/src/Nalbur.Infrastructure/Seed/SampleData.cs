using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Repositories;

namespace Nalbur.Infrastructure.Seed;

/// <summary>
/// A small, realistic hardware shop with two weeks of history, so the UI has
/// something to show. Runs only when SEED_SAMPLE_DATA=true.
/// Every line goes through the real domain methods, so the rules apply here too.
/// </summary>
public static class SampleData
{
    public static async Task SeedAsync(IProductRepository repository, TimeProvider clock, CancellationToken ct = default)
    {
        if ((await repository.GetAllAsync(ct)).Count > 0)
        {
            return;
        }

        var now = clock.GetUtcNow().UtcDateTime;
        DateTime DaysAgo(double d) => now.AddDays(-d);
        DateTime HoursAgo(double h) => now.AddHours(-h);

        var products = new List<Product>();

        Product Add(string sku, string name, Unit unit, decimal critical, DateTime created)
        {
            var p = Product.Create(sku, name, unit, critical, created);
            products.Add(p);
            return p;
        }

        var matkap = Add("MTK-6-HSS", "Matkap ucu 6 mm HSS", Unit.Adet, 10, DaysAgo(14));
        matkap.Receive(50, "Tedarikçi irsaliyesi #2231", DaysAgo(9));
        matkap.Issue(12, "Usta siparişi", DaysAgo(6));
        matkap.Issue(8, "Perakende satış", DaysAgo(2));
        matkap.Issue(6, "Perakende satış", HoursAgo(3));

        var vida = Add("VD-4X40", "Sunta vidası 4x40 (200'lü)", Unit.Paket, 5, DaysAgo(14));
        vida.Receive(20, "Tedarikçi irsaliyesi #2231", DaysAgo(8));
        vida.Issue(9, "Mobilya atölyesi", DaysAgo(4));
        vida.Issue(4, "Perakende satış", DaysAgo(1));

        var silikon = Add("SLK-280-SF", "Silikon şeffaf 280 ml", Unit.Adet, 6, DaysAgo(14));
        silikon.Receive(24, "Tedarikçi irsaliyesi #2198", DaysAgo(10));
        silikon.Issue(15, "İnşaat şantiyesi", DaysAgo(5));
        silikon.Issue(6, "Perakende satış", HoursAgo(20));

        var dubel = Add("DBL-8", "Plastik dübel 8 mm (100'lü)", Unit.Paket, 4, DaysAgo(14));
        dubel.Receive(10, "Tedarikçi irsaliyesi #2198", DaysAgo(12));
        dubel.Issue(8, "İnşaat şantiyesi", DaysAgo(3));

        var kablo = Add("KBL-NYA-2-5", "NYA kablo 2,5 mm²", Unit.Metre, 50, DaysAgo(14));
        kablo.Receive(300, "Makara (300 m)", DaysAgo(7));
        kablo.Issue(85.5m, "Elektrikçi siparişi", DaysAgo(5));
        kablo.Issue(40, "Perakende satış", DaysAgo(2));

        var tel = Add("TEL-GLV-2", "Galvaniz tel 2 mm", Unit.Kg, 5, DaysAgo(14));
        tel.Receive(25, "Tedarikçi irsaliyesi #2240", DaysAgo(9));
        tel.Issue(7.25m, "Bahçe çiti", DaysAgo(4));
        tel.Issue(3.5m, "Perakende satış", HoursAgo(6));

        var tiner = Add("TNR-SLL", "Selülozik tiner", Unit.Litre, 4, DaysAgo(14));
        tiner.Receive(12, "Tedarikçi irsaliyesi #2240", DaysAgo(6));
        tiner.Issue(7, "Boya ustası", DaysAgo(3));
        tiner.Issue(5, "Perakende satış", HoursAgo(5));

        var zimpara = Add("ZMP-P120", "Zımpara kağıdı P120", Unit.Adet, 20, DaysAgo(14));
        zimpara.Receive(100, "Tedarikçi irsaliyesi #2187", DaysAgo(11));
        zimpara.Issue(35, "Mobilya atölyesi", DaysAgo(6));
        zimpara.Issue(18, "Perakende satış", DaysAgo(2));

        var pense = Add("PNS-180", "Pense 180 mm", Unit.Adet, 3, DaysAgo(14));
        pense.Receive(8, "Tedarikçi irsaliyesi #2150", DaysAgo(13));
        pense.Issue(2, "Perakende satış", DaysAgo(7));

        foreach (var product in products)
        {
            await repository.AddAsync(product, ct);
        }
    }
}
