-- ClickHouse initialization script for workflow analytics
-- This script creates the necessary database and tables for workflow data

-- Create database if it doesn't exist
CREATE DATABASE IF NOT EXISTS workflow_analytics;

-- Use the workflow_analytics database
USE workflow_analytics;

-- Create instances table
CREATE TABLE IF NOT EXISTS instances (
    Id UUID,
    Key Nullable(String),
    Flow String,
    CurrentState Nullable(String),
    Status String,
    CreatedAt DateTime64(3),
    ModifiedAt Nullable(DateTime64(3)),
    CompletedAt Nullable(DateTime64(3)),
    DurationSeconds Nullable(Float64),
    Tags String DEFAULT '[]',
    IsTransient UInt8 DEFAULT 0,
    Operation String,
    TransferTimestamp DateTime64(3) DEFAULT now64(3)
) ENGINE = MergeTree()
ORDER BY (CreatedAt, Id)
PARTITION BY toYYYYMM(CreatedAt)
SETTINGS index_granularity = 8192;

-- Create instance_transitions table
CREATE TABLE IF NOT EXISTS instance_transitions (
    Id UUID,
    InstanceId UUID,
    TransitionId String,
    FromState String,
    ToState Nullable(String),
    StartedAt DateTime64(3),
    FinishedAt Nullable(DateTime64(3)),
    DurationSeconds Nullable(Float64),
    Body String DEFAULT '{}',
    Header String DEFAULT '{}',
    Operation String,
    TransferTimestamp DateTime64(3) DEFAULT now64(3)
) ENGINE = MergeTree()
ORDER BY (StartedAt, Id)
PARTITION BY toYYYYMM(StartedAt)
SETTINGS index_granularity = 8192;

-- Create instance_tasks table
CREATE TABLE IF NOT EXISTS instance_tasks (
    Id UUID,
    TransitionId UUID,
    TaskId String,
    Status String,
    StartedAt DateTime64(3),
    FinishedAt Nullable(DateTime64(3)),
    DurationSeconds Nullable(Float64),
    FaultedTaskId Nullable(UUID),
    Request String DEFAULT '{}',
    Response String DEFAULT '{}',
    Operation String,
    TransferTimestamp DateTime64(3) DEFAULT now64(3)
) ENGINE = MergeTree()
ORDER BY (StartedAt, Id)
PARTITION BY toYYYYMM(StartedAt)
SETTINGS index_granularity = 8192;

-- Create materialized views for common analytics queries

-- Instance status summary view
CREATE MATERIALIZED VIEW IF NOT EXISTS instance_status_summary
ENGINE = SummingMergeTree()
ORDER BY (Flow, Status, toDate(CreatedAt))
AS SELECT
    Flow,
    Status,
    toDate(CreatedAt) as Date,
    count() as Count,
    avg(DurationSeconds) as AvgDurationSeconds,
    max(DurationSeconds) as MaxDurationSeconds,
    min(DurationSeconds) as MinDurationSeconds
FROM instances
GROUP BY Flow, Status, toDate(CreatedAt);

-- Transition performance view
CREATE MATERIALIZED VIEW IF NOT EXISTS transition_performance
ENGINE = SummingMergeTree()
ORDER BY (TransitionId, FromState, toDate(StartedAt))
AS SELECT
    TransitionId,
    FromState,
    toDate(StartedAt) as Date,
    count() as Count,
    avg(DurationSeconds) as AvgDurationSeconds,
    max(DurationSeconds) as MaxDurationSeconds,
    min(DurationSeconds) as MinDurationSeconds
FROM instance_transitions
WHERE DurationSeconds IS NOT NULL
GROUP BY TransitionId, FromState, toDate(StartedAt);

-- Task performance view
CREATE MATERIALIZED VIEW IF NOT EXISTS task_performance
ENGINE = SummingMergeTree()
ORDER BY (TaskId, Status, toDate(StartedAt))
AS SELECT
    TaskId,
    Status,
    toDate(StartedAt) as Date,
    count() as Count,
    avg(DurationSeconds) as AvgDurationSeconds,
    max(DurationSeconds) as MaxDurationSeconds,
    min(DurationSeconds) as MinDurationSeconds
FROM instance_tasks
WHERE DurationSeconds IS NOT NULL
GROUP BY TaskId, Status, toDate(StartedAt);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_instances_flow ON instances (Flow) TYPE bloom_filter GRANULARITY 1;
CREATE INDEX IF NOT EXISTS idx_instances_status ON instances (Status) TYPE bloom_filter GRANULARITY 1;
CREATE INDEX IF NOT EXISTS idx_transitions_instance_id ON instance_transitions (InstanceId) TYPE bloom_filter GRANULARITY 1;
CREATE INDEX IF NOT EXISTS idx_tasks_transition_id ON instance_tasks (TransitionId) TYPE bloom_filter GRANULARITY 1;

