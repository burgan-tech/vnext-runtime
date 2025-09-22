
# Persistance

İş akış örneklerinin saklanması için bir veri tabanı katmanı sunulur. Bu katmanda tüm akış örnekleri ve ilişkili verileri tutulur. Bu katman **master data** olarak anılır.

vNext Platform, Dual-Write Pattern (Çift Yazma Deseni) destekler. Bu destek ile:
* Event Sourcing yaklaşımı ile iş akış örneklerinde veri değişimleri bir akış olarak sunulabilir. (CDC - Change Data Capture) 
* İş akış örneklerinin birer kopyaları başka bir veritabanında barındırılabilir. (Replication)

## Master Data

İş akış örneklerini saklamak için Entity Framework tabanlı veritabanı kullanılır:
- Her bir akış için şema yaratılır.
- Her bir akış örneği farklı veri tablolarında saklanır.
- Her bir domain farklı veritabanı ile çalışır.
- Her bir runtime sadece bir domain çalıştırır.
- Domainler runtime'lara bölünemez.

## Akış Şemaları

Her bir akış için bir şema oluşturulup içerisinde verileri saklamak için ön tanımlı bir tablo kümesi yaratılır.

### Tablolar

**Instance**: Her bir iş akış örneğinin temel bilgilerini tutar.

**InstanceData**: İş akışının içerdiği veri kümesini tutar.

**InstanceCorrelation**: SubProcess/SubFlow tipinde başlatılmış iş akış örneklerinin referanslarıdır. SubProcess/SubFlow farklı bir domain üzerinde çalışıyor olabilir.

**InstanceTransition**: İş akış örneği ile ilgili tüm geçiş bilgilerini tutar.

**InstanceTask**: İş akış örneği ile ilgili tüm görev çalışma bilgilerini tutar.

**InstanceAction**: İş akış örneğinde çalıştırılan bir görev ile ilgili tüm alt adım çalışma bilgilerini tutar.

**InstanceJobs**: İş akış örneğinde çalıştırılan zamanlanmış görevler ile ilgili tüm bilgileri tutar.

***Task***
> Task birden fazla konu için çalışabilir, bu yüzden ne amaçla tetiklendiğini bilmek için Type enum tipi bulunmaktadır.
> Task'ı tetikleyen tipin kayıt örneğinin tekil numarası için reference alanı kullanılır.


![DB Diagram](https://kroki.io/mermaid/svg/eNq9VlFv2jAQfu-vsCJVah8Q75P20BbQGC1jA9rH6UgOcJfYme2worL_vrNJk5CEJJW25YVwvjv7vvv8XVANOGwURBeMnrHQBoSPmr26__ZJEh6wccBmk8y0A-VvQbEJ7iu2USh_MW_6ZcGmy_t7r7J-lyiFwswNGKwsWmui83B2deMbvsP-nYziEA0G_REkof29TfS-PwOtafk638bwCKmGKGZZyI3JVrkwqHYQskGiwHAp8jh8Md9BKdizBWx0XT6F4LLVVZe7PciAr_nJrs9aihV7QAMDMJCZV1KGCIKN9UKB0JxQcWu_L057cScJsdAdt6UvzjQDi-9bNDmMJk39OLq7dtS5uZTzZGXbWsjZkDB1XuxjLDZy_jFd6M_s20xJKk1fn40fyAi46LDRFCLs4PaIShcbnqOfEaW5sWU-ldtke9uhPZ0bkx64yWVIVK1bPxLOHaixpKG9DuconQN0T9zQtUUfiduVmZ1Lz9M2U22kZHSWuVkyeao1eflkVx1u9IgLrrcddeQI_a0M9iXTJ4QAVS2IoH90ge8ElRYAKWXLLa0o7RNQbrGpSm2tuv4L7FyV6aZpBaNJCcZv-DN5I2PRqmPiINbBaydIN35me74HuMqtI5nn4X9ArFTmZ7nqOLXJs00zyaVIH3a1nI6_Loc1at0u000aXZaFJq17iRUNDKr-EcIEmxVrofhmY6XtfJ9msA8lBH9t0KftuLwkMqbDestjXfqsOhx6Pfl6ZrZ_YN4WNPO3PAyYX1jxWrI4pU-jA_u-O86OtsCifKfhJje1BDvCpVHaJ8YmIeH9TNZj4NlBUT6Ekz9K5EtBV4ccjLV4NRpZiny72ekhwK85diHwUN0yos-9tRUcZiQDIc0Wldu-UAJ1dI7huqdwTYwSPmlksTlsLbNvDLqpKVjvbDudPnbfYVkG7_0ZdLJaFw_h_QGEy4Ee)