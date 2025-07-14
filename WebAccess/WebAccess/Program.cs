using System.Net;
using System.Xml.Linq;

// --- XML'den SADECE Port Ayar�n� Okuma ve Gerekirse Dosya Olu�turma Fonksiyonu ---
int GetListenerPort()
{
    int defaultPort = 5000;
    string settingsFilePath = "listener_settings.xml";

    try
    {
        if (!File.Exists(settingsFilePath))
        {
            Console.WriteLine($"Uyar�: '{settingsFilePath}' dosyas� bulunamad�. Varsay�lan port ({defaultPort}) ile yeni bir dosya olu�turuluyor...");

            XDocument newDoc = new XDocument(
                new XElement("Settings",
                    new XElement("Listener",
                        // XML olu�turulurken art�k IP adresi yazmaya gerek yok.
                        new XElement("Port", defaultPort.ToString())
                    )
                )
            );

            newDoc.Save(settingsFilePath);
            Console.WriteLine($"'{settingsFilePath}' dosyas� ba�ar�yla olu�turuldu.");
            return defaultPort;
        }
        else
        {
            XDocument doc = XDocument.Load(settingsFilePath);
            string xmlPortStr = doc.Element("Settings")?.Element("Listener")?.Element("Port")?.Value;

            if (int.TryParse(xmlPortStr, out int portValue) && portValue > 0 && portValue < 65536)
            {
                Console.WriteLine($"Port ayar� '{settingsFilePath}' dosyas�ndan y�klendi: Port = {portValue}");
                return portValue;
            }
            else
            {
                Console.WriteLine($"Uyar�: '{settingsFilePath}' dosyas�ndaki port de�eri ge�ersiz. Varsay�lan port ({defaultPort}) kullan�l�yor.");
                return defaultPort;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Hata: '{settingsFilePath}' dosyas� i�lenirken bir hata olu�tu: {ex.Message}. Varsay�lan port ({defaultPort}) kullan�l�yor.");
        return defaultPort;
    }
}


// --- Ana Uygulama Kodu ---

var builder = WebApplication.CreateBuilder(args);

// Port ayar�n� XML'den oku (veya olu�tur)
var listenerPort = GetListenerPort();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Kestrel sunucusunun ayarlar�
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxResponseBufferSize = null;

    // HATAYI ��ZEN ANAHTAR SATIR:
    // Makinedeki T�M a� aray�zlerini dinle, portu XML'den gelen dinamik de�erle ayarla.
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

// Uygulaman�n hangi adreslerde dinledi�ini konsola yazd�r.
Console.WriteLine($"Uygulama t�m yerel IP adreslerinden {listenerPort} portunu dinliyor.");
Console.WriteLine("Panele girilecek sunucu adresi i�in, bu bilgisayar�n yerel a�daki IP adresini kullan�n (�rn: http://192.168.1.44:{0})", listenerPort);

app.Run();