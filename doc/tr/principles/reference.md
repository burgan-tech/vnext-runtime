# Referans Şeması

vNext Platform içinde bulunan iş akışı bileşenleri arasında referans ilişkisi kurulabilir. Bu kavram, veritabanı içinde `Foreign Key` gibi düşünülebilir ve bileşenler arası güvenli bağımlılık yönetimi sağlar.

## Temel Referans Şeması

Tüm bileşenler (workflows, tasks, functions, extensions, schemas, views) arasında referans oluşturmak için aşağıdaki standart şema kullanılır:

```json
{
  "key": "bileşen-benzersiz-anahtarı",
  "domain": "domain-adı", 
  "version": "versiyon-bilgisi",
  "flow": "bileşen-modül-türü"
}
```

### Şema Alanları

- **key**: Bileşenin benzersiz tanımlayıcısı
- **domain**: Bileşenin çalışacağı domain bilgisi (örn: "core", "account", "payment")
- **version**: Bileşenin versiyon bilgisi (örn: "1.0.0", "2.1.3")
- **flow**: Bileşenin hangi modülde yer aldığı (örn: "sys-flows", "sys-tasks", "sys-functions")

## Kullanım Alanları

### 1. Task Referansları

Workflow state'lerinde task'lere referans verirken:

```json
{
  "onEntries": [
    {
      "order": 1,
      "task": {
        "key": "validate-client",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      }
    }
  ]
}
```

### 2. SubFlow Referansları

Bir state'in alt akış çalıştırması durumunda:

```json
{
  "subFlow": {
    "type": "S",
    "process": {
      "key": "password-subflow",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-flows"
    }
  }
}
```

### 3. Function Referansları

Özel fonksiyonlara referans verirken:

```json
{
  "function": {
    "key": "calculate-interest",
    "domain": "banking",
    "version": "2.0.1",
    "flow": "sys-functions"
  }
}
```

## Flow Türleri

- **sys-flows**: Ana iş akışları
- **sys-tasks**: Görev tanımları
- **sys-functions**: Özel fonksiyonlar
- **sys-extensions**: Sistem uzantıları
- **sys-schemas**: Veri şemaları
- **sys-views**: Görünüm tanımları

## Kısa Referans Kullanımı (ref)

Geliştirme aşamasında daha pratik kullanım için **"ref"** kısa kullanımı mevcuttur. Build sürecinde bu referanslar otomatik olarak tam referans formatına dönüştürülür.

### Lokal Tanım Referansı

Proje içindeki dosyalara referans verirken:

```json
{
  "onExecutionTasks": [
    {
      "order": 1,
      "task": {
        "ref": "Tasks/invalidate-cache.json"
      }
    }
  ]
}
```

### Cross Project Referansı

NPM paketi veya external tanımlara referans verirken:

```json
{
  "onExecutionTasks": [
    {
      "order": 1,
      "task": {
        "ref": "@burgan-tech/vnext-core-reference/core/Tasks/invalidate-cache.json"
      }
    }
  ]
}
```

### Build-Time Dönüşümü

Build sürecinde `ref` kullanımları otomatik olarak tam referans formatına çevrilir:

**Geliştirme sırasında:**
```json
{
  "task": {
    "ref": "Tasks/validate-client.json"
  }
}
```

**Build sonrası:**
```json
{
  "task": {
    "key": "validate-client",
    "domain": "core",
    "version": "1.0.0", 
    "flow": "sys-tasks"
  }
}
```

### Ref Kullanım Avantajları

- ✅ **Hızlı geliştirme**: Kısa ve anlaşılır syntax
- ✅ **Otomatik çözümleme**: Build sırasında tam referansa dönüştürülür
- ✅ **IntelliSense desteği**: IDE'de dosya path autocomplete
- ✅ **Lokal/external esneklik**: Hem local hem NPM paketi desteği

## Referans Çözümleme

Runtime sırasında referanslar şu sırayla çözümlenir:

1. Key ve domain kombinasyonu ile bileşen aranır
2. Versiyon stratejisine göre uygun versiyon seçilir
3. Flow türü doğrulanır ve bileşen yüklenir
4. Bağımlılık grafiği oluşturulur

## Hata Yönetimi

Referans çözümlenemediğinde:

- **Missing Component**: Bileşen bulunamadı
- **Version Mismatch**: Versiyon uyumsuzluğu
- **Domain Error**: Domain erişim hatası
- **Circular Reference**: Döngüsel bağımlılık

## En İyi Pratikler

1. **Açık versiyonlama**: Kritik referanslarda tam versiyon kullanın
2. **Domain ayrımı**: İlgili bileşenleri aynı domain'de toplayın
3. **Döngüsel bağımlılık**: Referans zincirlerinde döngü oluşturmayın
4. **Geriye uyumluluk**: Versiyon değişikliklerinde API uyumluluğu koruyun
