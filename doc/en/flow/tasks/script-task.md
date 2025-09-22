# Script Task

Script Task is a task type used to execute .NET code in the C# Roslyn runtime. This task type is primarily designed for **output processing** and is used to perform **business logic**, **calculations**, and **data transformation** operations on instance data.

## Main Usage Purpose

The main purpose of Script Task is:
- Applying business logic on instance data (master data)
- Performing calculation operations  
- Preparing data for subsequent states
- Implementing complex business rules with code

## ⚠️ Important Limitations

- **Do not make remote service calls** (HTTP, API, Database, etc.)
- **Do not use operations that create blocking**
- **Write only simple, fast calculation and business logic code**

## Features

- ✅ Dynamic typing
- ✅ LINQ and Collections
- ✅ JSON processing
- ✅ Context data access

## Task Definition

### Basic Structure

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
## Script Structure

Script files must contain a class that implements the IMapping interface:

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Definitions;
using BBT.Workflow.Scripting;

public class CustomScript : IMapping
{
    public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Input processing logic
        return new ScriptResponse();
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        // Output processing logic
        return new ScriptResponse();
    }
}
```

## Usage Examples

### 1. Business Logic - Credit Score Calculation

```csharp
public class CreditScoreCalculator : IMapping
{
    public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return new ScriptResponse();
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        // Get customer information from instance data
        var customerData = context.Instance.Data;
        
        // Business logic: Credit score calculation
        var salary = (decimal)customerData.salary;
        var age = (int)customerData.age;
        var hasCollateral = (bool)customerData.hasCollateral;
        var creditHistory = (int)customerData.creditHistoryMonths;
        
        var creditScore = CalculateCreditScore(salary, age, hasCollateral, creditHistory);
        var riskLevel = DetermineRiskLevel(creditScore);
        
        // Prepare result for next states
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
        
        // Salary factor
        if (salary >= 50000) baseScore += 200;
        else if (salary >= 25000) baseScore += 150;
        else if (salary >= 15000) baseScore += 100;
        
        // Age factor
        if (age >= 25 && age <= 55) baseScore += 100;
        else if (age >= 18 && age <= 65) baseScore += 50;
        
        // Collateral factor
        if (hasCollateral) baseScore += 150;
        
        // Credit history factor
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

### 2. Calculation - Price Optimization

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
        
        // Business logic: Dynamic pricing
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
        return Math.Min(quantityDiscount + tierDiscount, 0.5m); // Max 50% discount
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

### 3. Data Transformation - Address Normalization

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
        
        // Normalize address data
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
        
        // Add normalized data while preserving original data
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
                     .Replace("STREET", "St.")
                     .Replace("AVENUE", "Ave.")
                     .Replace("NEIGHBORHOOD", "Neighborhood")
                     .Replace("APARTMENT", "Apt.");
    }
    
    private string NormalizeCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city)) return "";
        
        var normalized = city.Trim().ToUpperInvariant();
        
        return normalized switch
        {
            "ISTANBUL" or "İSTANBUL" => "ISTANBUL",
            "ANKARA" => "ANKARA",
            "IZMIR" or "İZMİR" => "IZMIR",
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
            "ISTANBUL" => "IST",
            "ANKARA" => "ANK",
            "IZMIR" => "IZM",
            _ => "OTH"
        };
    }
    
    private string CalculateDeliveryZone(string city, string district)
    {
        if (city.ToUpperInvariant() == "ISTANBUL")
        {
            var centralDistricts = new[] { "BEYOGLU", "KADIKOY", "BESIKTAS", "USKUDAR" };
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

### ✅ Do's

- **Use only OutputHandler** - Work output-focused instead of input handler
- **Make fast calculations** - Avoid complex algorithms  
- **Process instance data** - Transformation on master data
- **Prepare data for next states**
- **Implement business rules** - Domain logic
- **Write deterministic code** - Same input, same output

### ❌ Don'ts

- **Don't make HTTP calls** - RestClient, HttpClient usage is prohibited
- **Don't do database operations** - SQL query, connection opening
- **Don't do File I/O operations** - File reading/writing
- **External API calls** - Third-party service integrations
- **Thread.Sleep() usage** - Operations that create blocking
- **Heavy computation** - CPU-intensive long operations

### 🔧 Performance Guide

```csharp
public async Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    // ✅ GOOD - Fast calculation
    var data = context.Instance.Data;
    var result = data.amount * 1.18m; // VAT calculation
    
    // ❌ BAD - Remote service call
    // var response = await httpClient.GetAsync("https://api.example.com");
    
    // ✅ GOOD - Simple business logic
    var discountRate = data.customerType == "VIP" ? 0.2m : 0.1m;
    
    // ❌ BAD - Blocking operation
    // Thread.Sleep(1000);
    var output = new ScriptResponse();
    output.Data = new { calculatedAmount = result, discount = discountRate };
    return output;
}
```

## Error Management

### Try-Catch Usage

```csharp
public async Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    try
    {
        var data = context.Instance.Data;
        
        // Data validation
        if (data?.amount == null)
        {
            return new ScriptResponse();
        }
        
        // Business logic calculation
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

## Standard Response

Script Task directly returns the `ScriptResponse` class.
