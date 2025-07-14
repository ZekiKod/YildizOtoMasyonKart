using System.Net;
using System.Xml.Linq;

// --- XML'den SADECE Port Ayarýný Okuma ve Gerekirse Dosya Oluþturma Fonksiyonu ---
int GetListenerPort()
{
    int defaultPort = 5000;
    string settingsFilePath = "listener_settings.xml";

    try
    {
        if (!File.Exists(settingsFilePath))
        {
            Console.WriteLine($"Uyarý: '{settingsFilePath}' dosyasý bulunamadý. Varsayýlan port ({defaultPort}) ile yeni bir dosya oluþturuluyor...");

            XDocument newDoc = new XDocument(
                new XElement("Settings",
                    new XElement("Listener",
                        // XML oluþturulurken artýk IP adresi yazmaya gerek yok.
                        new XElement("Port", defaultPort.ToString())
                    )
                )
            );

            newDoc.Save(settingsFilePath);
            Console.WriteLine($"'{settingsFilePath}' dosyasý baþarýyla oluþturuldu.");
            return defaultPort;
        }
        else
        {
            XDocument doc = XDocument.Load(settingsFilePath);
            string xmlPortStr = doc.Element("Settings")?.Element("Listener")?.Element("Port")?.Value;

            if (int.TryParse(xmlPortStr, out int portValue) && portValue > 0 && portValue < 65536)
            {
                Console.WriteLine($"Port ayarý '{settingsFilePath}' dosyasýndan yüklendi: Port = {portValue}");
                return portValue;
            }
            else
            {
                Console.WriteLine($"Uyarý: '{settingsFilePath}' dosyasýndaki port deðeri geçersiz. Varsayýlan port ({defaultPort}) kullanýlýyor.");
                return defaultPort;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Hata: '{settingsFilePath}' dosyasý iþlenirken bir hata oluþtu: {ex.Message}. Varsayýlan port ({defaultPort}) kullanýlýyor.");
        return defaultPort;
    }
}


// --- Ana Uygulama Kodu ---

var builder = WebApplication.CreateBuilder(args);

// Port ayarýný XML'den oku (veya oluþtur)
var listenerPort = GetListenerPort();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Kestrel sunucusunun ayarlarý
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxResponseBufferSize = null;

    // HATAYI ÇÖZEN ANAHTAR SATIR:
    // Makinedeki TÜM að arayüzlerini dinle, portu XML'den gelen dinamik deðerle ayarla.
    options.Listen(IPAddress.Parse("192.168.1.44"), listenerPort);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Uygulamanýn hangi adreslerde dinlediðini konsola yazdýr.
Console.WriteLine($"Uygulama tüm yerel IP adreslerinden {listenerPort} portunu dinliyor.");
Console.WriteLine("Panele girilecek sunucu adresi için, bu bilgisayarýn yerel aðdaki IP adresini kullanýn (örn: http://192.168.1.44:{0})", listenerPort);

app.Run();