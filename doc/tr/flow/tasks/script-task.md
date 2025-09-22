# Script Task

Script Task, C# Roslyn çalışma zamanında .NET kodu çalıştırmak için kullanılan görev türüdür. Bu görev türü öncelikle **output işleme** için tasarlanmıştır ve instance data üzerinde **iş mantığı**, **hesaplama** ve **veri dönüşümü** işlemleri gerçekleştirmek için kullanılır.

## Ana Kullanım Amacı

Script Task'ın temel amacı:
- Instance data (master data) üzerinde iş mantığı uygulamak
- Hesaplama işlemleri gerçekleştirmek  
- Veriyi sonraki state'ler için hazırlamak
- Karmaşık iş kurallarını kodla implement etmek

## ⚠️ Önemli Kısıtlamalar

- **Uzak servis çağrıları yapmayın** (HTTP, API, Database vb.)
- **Blokaj yaratacak işlemler kullanmayın**
- **Sadece basit, hızlı hesaplama ve iş mantığı kodları yazın**

## Özellikler

- ✅ Dynamic typing
- ✅ LINQ ve Collections
- ✅ JSON işleme
- ✅ Context data erişimi

## Görev Tanımı

### Temel Yapı

```json
{
  "key": "data-processing-script",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-tasks",
  "flowVersion": "1.0.0",
  "tags": [
    "oauth2",
    "push-notification",
    "mfa",
    "response-check",
    "approval"
  ],
  "attributes": {
    "type": "7",
    "config": {
    }
  }
}

```
## Script Yapısı

Script dosyaları IMapping arayüzünü implement eden bir class içermelidir:

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Definitions;
using BBT.Workflow.Scripting;

public class CustomScript : IMapping
{
    public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Input işleme mantığı
        return new ScriptResponse();
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        // Output işleme mantığı
        return new ScriptResponse();
    }
}
```

## Kullanım Örnekleri

### 1. İş Mantığı - Kredi Skoru Hesaplama

```csharp
public class CreditScoreCalculator : IMapping
{
    public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return new ScriptResponse();
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        // Instance data'dan müşteri bilgilerini al
        var customerData = context.Instance.Data;
        
        // İş mantığı: Kredi skoru hesaplama
        var salary = (decimal)customerData.salary;
        var age = (int)customerData.age;
        var hasCollateral = (bool)customerData.hasCollateral;
        var creditHistory = (int)customerData.creditHistoryMonths;
        
        var creditScore = CalculateCreditScore(salary, age, hasCollateral, creditHistory);
        var riskLevel = DetermineRiskLevel(creditScore);
        
        // Sonucu sonraki state'ler için hazırla
        var result = new
        {
            customerId = customerData.customerId,
            creditScore = creditScore,
            riskLevel = riskLevel,
            calculatedAt = DateTime.UtcNow,
            isEligible = creditScore >= 650
        };

        var output = new ScriptResponse();
        
        output.Data = new {result};
        return output;
    }
    
    private int CalculateCreditScore(decimal salary, int age, bool hasCollateral, int creditHistory)
    {
        var baseScore = 300;
        
        // Maaş faktörü
        if (salary >= 50000) baseScore += 200;
        else if (salary >= 25000) baseScore += 150;
        else if (salary >= 15000) baseScore += 100;
        
        // Yaş faktörü
        if (age >= 25 && age <= 55) baseScore += 100;
        else if (age >= 18 && age <= 65) baseScore += 50;
        
        // Teminat faktörü
        if (hasCollateral) baseScore += 150;
        
        // Kredi geçmişi faktörü
        baseScore += Math.Min(creditHistory * 5, 200);
        
        return Math.Min(baseScore, 850);
    }
    
    private string DetermineRiskLevel(int creditScore)
    {
        if (creditScore >= 750) return "LOW";
        if (creditScore >= 650) return "MEDIUM";
        return "HIGH";
    }
}
```

### 2. Hesaplama - Fiyat Optimizasyonu

```csharp
public class PriceOptimizationScript : IMapping
{
    public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return new ScriptResponse();
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var orderData = context.Instance.Data;
        
        // İş mantığı: Dinamik fiyatlama
        var basePrice = (decimal)orderData.basePrice;
        var quantity = (int)orderData.quantity;
        var customerTier = (string)orderData.customerTier;
        var seasonalFactor = (decimal)orderData.seasonalFactor;
        
        var optimizedPrice = CalculateOptimizedPrice(basePrice, quantity, customerTier, seasonalFactor);
        var discount = CalculateDiscount(quantity, customerTier);
        var finalPrice = optimizedPrice * (1 - discount);
        
        var result = new
        {
            originalPrice = basePrice,
            optimizedPrice = optimizedPrice,
            discountPercentage = discount * 100,
            finalPrice = finalPrice,
            totalAmount = finalPrice * quantity,
            calculationDetails = new
            {
                quantityDiscount = GetQuantityDiscount(quantity),
                tierDiscount = GetTierDiscount(customerTier),
                seasonalAdjustment = seasonalFactor
            }
        };
        
        var output = new ScriptResponse();
        output.Data = new {result};
        return output;
    }
    
    private decimal CalculateOptimizedPrice(decimal basePrice, int quantity, string tier, decimal seasonal)
    {
        return basePrice * seasonal * GetTierMultiplier(tier);
    }
    
    private decimal CalculateDiscount(int quantity, string tier)
    {
        var quantityDiscount = GetQuantityDiscount(quantity);
        var tierDiscount = GetTierDiscount(tier);
        return Math.Min(quantityDiscount + tierDiscount, 0.5m); // Max %50 indirim
    }
    
    private decimal GetQuantityDiscount(int quantity)
    {
        if (quantity >= 100) return 0.2m;
        if (quantity >= 50) return 0.15m;
        if (quantity >= 20) return 0.1m;
        if (quantity >= 10) return 0.05m;
        return 0m;
    }
    
    private decimal GetTierDiscount(string tier)
    {
        return tier switch
        {
            "PLATINUM" => 0.15m,
            "GOLD" => 0.1m,
            "SILVER" => 0.05m,
            _ => 0m
        };
    }
    
    private decimal GetTierMultiplier(string tier)
    {
        return tier switch
        {
            "PLATINUM" => 0.95m,
            "GOLD" => 0.98m,
            _ => 1.0m
        };
    }
}
```

### 3. Veri Dönüşümü - Adres Normalizasyonu

```csharp
public class AddressNormalizationScript : IMapping
{
    public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return new ScriptResponse();
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var customerData = context.Instance.Data;
        
        // Adres verilerini normalize et
        var rawAddress = (string)customerData.address;
        var city = (string)customerData.city;
        var district = (string)customerData.district;
        var postalCode = (string)customerData.postalCode;
        
        var normalizedAddress = new
        {
            formattedAddress = NormalizeAddress(rawAddress),
            standardCity = NormalizeCity(city),
            standardDistrict = NormalizeDistrict(district),
            validatedPostalCode = ValidateAndFormatPostalCode(postalCode),
            regionCode = GetRegionCode(city),
            deliveryZone = CalculateDeliveryZone(city, district)
        };
        
        // Orijinal veriyi koruyarak normalize edilmiş veriyi ekle
        var result = new
        {
            customerId = customerData.customerId,
            originalAddress = new
            {
                address = rawAddress,
                city = city,
                district = district,
                postalCode = postalCode
            },
            normalizedAddress = normalizedAddress,
            isAddressValid = ValidateAddress(normalizedAddress)
        };
        
        var output = new ScriptResponse();
        output.Data = new {result};
        return output;
    }
    
    private string NormalizeAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return "";
        
        return address.Trim()
                     .Replace("  ", " ")
                     .Replace("SOKAK", "Sok.")
                     .Replace("CADDE", "Cad.")
                     .Replace("MAHALLE", "Mah.")
                     .Replace("APARTMAN", "Apt.");
    }
    
    private string NormalizeCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city)) return "";
        
        var normalized = city.Trim().ToUpperInvariant();
        
        return normalized switch
        {
            "İSTANBUL" or "ISTANBUL" => "İSTANBUL",
            "ANKARA" => "ANKARA",
            "İZMİR" or "IZMIR" => "İZMİR",
            _ => normalized
        };
    }
    
    private string NormalizeDistrict(string district)
    {
        return string.IsNullOrWhiteSpace(district) ? "" : district.Trim().ToUpperInvariant();
    }
    
    private string ValidateAndFormatPostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode)) return "";
        
        var cleaned = postalCode.Replace(" ", "").Replace("-", "");
        return cleaned.Length == 5 && cleaned.All(char.IsDigit) ? cleaned : "";
    }
    
    private string GetRegionCode(string city)
    {
        return city.ToUpperInvariant() switch
        {
            "İSTANBUL" => "IST",
            "ANKARA" => "ANK",
            "İZMİR" => "IZM",
            _ => "OTH"
        };
    }
    
    private string CalculateDeliveryZone(string city, string district)
    {
        if (city.ToUpperInvariant() == "İSTANBUL")
        {
            var centralDistricts = new[] { "BEYOĞLU", "KADIKÖY", "BEŞİKTAŞ", "ÜSKÜDAR" };
            return centralDistricts.Contains(district.ToUpperInvariant()) ? "CENTRAL" : "SUBURBAN";
        }
        return "STANDARD";
    }
    
    private bool ValidateAddress(dynamic address)
    {
        return !string.IsNullOrWhiteSpace(address.formattedAddress) &&
               !string.IsNullOrWhiteSpace(address.standardCity) &&
               !string.IsNullOrWhiteSpace(address.validatedPostalCode);
    }
}
```

## Best Practices

### ✅ Yapılması Gerekenler

- **Sadece OutputHandler kullanın** - Input handler yerine output odaklı çalışın
- **Hızlı hesaplamalar yapın** - Karmaşık algoritmalardan kaçının  
- **Instance data'yı işleyin** - Master data üzerinde transformasyon
- **Sonraki state'ler için veri hazırlayın**
- **İş kurallarını implement edin** - Domain logic
- **Deterministik kod yazın** - Aynı input, aynı output

### ❌ Yapılmaması Gerekenler

- **HTTP çağrıları yapmayın** - RestClient, HttpClient kullanımı yasak
- **Database işlemleri yapmayın** - SQL query, connection açma
- **File I/O işlemleri yapmayın** - Dosya okuma/yazma
- **External API çağrıları** - Üçüncü parti servis entegrasyonları
- **Thread.Sleep() kullanımı** - Blokaj yaratan operasyonlar
- **Heavy computation** - CPU yoğun uzun işlemler

### 🔧 Performans Rehberi

```csharp
public async Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    // ✅ İYİ - Hızlı hesaplama
    var data = context.Instance.Data;
    var result = data.amount * 1.18m; // KDV hesaplama
    
    // ❌ KÖTÜ - Uzak servis çağrısı
    // var response = await httpClient.GetAsync("https://api.example.com");
    
    // ✅ İYİ - Basit iş mantığı
    var discountRate = data.customerType == "VIP" ? 0.2m : 0.1m;
    
    // ❌ KÖTÜ - Blokaj yaratan işlem
    // Thread.Sleep(1000);
    var output = new ScriptResponse();
    output.Data = new { calculatedAmount = result, discount = discountRate };
    return output;
}
```

## Hata Yönetimi

### Try-Catch Kullanımı

```csharp
public async Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    try
    {
        var data = context.Instance.Data;
        
        // Veri doğrulama
        if (data?.amount == null)
        {
            return new ScriptResponse();
        }
        
        // İş mantığı hesaplama
        var amount = Convert.ToDecimal(data.amount);
        var tax = amount * 0.18m;
        var total = amount + tax;
        
        var output = new ScriptResponse();
        output.Data = new { 
            originalAmount = amount,
            taxAmount = tax,
            totalAmount = total 
        };
        
        return output;
    }
    catch (ArgumentException ex)
    {
        //Logging
        return new ScriptResponse();
    }
    catch (FormatException ex)
    {
        //Logging
        return new ScriptResponse();
    }
    catch (Exception ex)
    {
         //Logging
        return new ScriptResponse();
    }
}
```

## Standart Yanıt

Script Task, `ScriptResponse`sınıfını doğrudan döner.
