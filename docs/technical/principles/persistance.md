# Persistence

A database layer is provided for storing workflow instances. All workflow instances and their associated data are kept in this layer. This layer is called **master data**.

The vNext Platform supports the Dual-Write Pattern. With this support:
* Workflow instance data changes can be provided as a stream using the Event Sourcing approach. (CDC - Change Data Capture) 
* Copies of workflow instances can be hosted in another database. (Replication)

## Master Data

Entity Framework-based database is used to store workflow instances:
- A schema is created for each workflow.
- Each workflow instance is stored in different data tables.
- Each domain works with a different database.
- Each runtime runs only one domain.
- Domains cannot be divided into runtimes.

## Workflow Schemas

For each workflow, a schema is created and a predefined set of tables is created to store data within it.

### Tables

**Instance**: Holds basic information for each workflow instance.

**InstanceData**: Holds the data set contained in the workflow.

**InstanceCorrelation**: References of workflow instances started as SubProcess/SubFlow type. SubProcess/SubFlow may be running on a different domain.

**InstanceTransition**: Holds all transition information related to the workflow instance.

**InstanceTask**: Holds all task execution information related to the workflow instance.

**InstanceAction**: Holds all sub-step execution information related to a task executed in the workflow instance.

**InstanceJobs**: Holds all information related to scheduled tasks executed in the workflow instance.

***Task***
> Task can work for multiple subjects, so there is a Type enum type to know for what purpose it was triggered.
> The reference field is used for the unique number of the record instance of the type that triggered the Task.


![DB Diagram](https://kroki.io/mermaid/svg/eNq9VlFv2jAQfu-vsCJVah8Q75P20BbQGC1jA9rH6UgOcJfYme2worL_vrNJk5CEJJW25YVwvjv7vvv8XVANOGwURBeMnrHQBoSPmr26__ZJEh6wccBmk8y0A-VvQbEJ7iu2USh_MW_6ZcGmy_t7r7J-lyiFwswNGKwsWmui83B2deMbvsP-nYziEA0G_REkof29TfS-PwOtafk638bwCKmGKGZZyI3JVrkwqHYQskGiwHAp8jh8Md9BKdizBWx0XT6F4LLVVZe7PciAr_nJrs9aihV7QAMDMJCZV1KGCIKN9UKB0JxQcWu_L057cScJsdAdt6UvzjQDi-9bNDmMJk39OLq7dtS5uZTzZGXbWsjZkDB1XuxjLDZy_jFd6M_s20xJKk1fn40fyAi46LDRFCLs4PaIShcbnqOfEaW5sWU-ldtke9uhPZ0bkx64yWVIVK1bPxLOHaixpKG9DuconQN0T9zQtUUfiduVmZ1Lz9M2U22kZHSWuVkyeao1eflkVx1u9IgLrrcddeQI_a0M9iXTJ4QAVS2IoH90ge8ElRYAKWXLLa0o7RNQbrGpSm2tuv4L7FyV6aZpBaNJCcZv-DN5I2PRqmPiINbBaydIN35me74HuMqtI5nn4X9ArFTmZ7nqOLXJs00zyaVIH3a1nI6_Loc1at0u000aXZaFJq17iRUNDKr-EcIEmxVrofhmY6XtfJ9msA8lBH9t0KftuLwkMqbDestjXfqsOhx6Pfl6ZrZ_YN4WNPO3PAyYX1jxWrI4pU-jA_u-O86OtsCifKfhJje1BDvCpVHaJ8YmIeH9TNZj4NlBUT6Ekz9K5EtBV4ccjLV4NRpZiny72ekhwK85diHwUN0yos-9tRUcZiQDIc0Wldu-UAJ1dI7huqdwTYwSPmlksTlsLbNvDLqpKVjvbDudPnbfYVkG7_0ZdLJaFw_h_QGEy4Ee)
