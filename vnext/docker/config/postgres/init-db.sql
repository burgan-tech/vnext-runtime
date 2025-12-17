-- PostgreSQL initialization script for VNext Runtime
-- This script runs automatically when the postgres container starts for the first time

-- Create the main database for VNext
CREATE DATABASE "vNext_WorkflowDb";

-- Grant all privileges to postgres user
GRANT ALL PRIVILEGES ON DATABASE "vNext_WorkflowDb" TO postgres;

-- Connect to the new database and set up extensions if needed
\c "vNext_WorkflowDb"

-- Enable useful extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Log success
DO $$
BEGIN
    RAISE NOTICE 'Database vNext_WorkflowDb created successfully!';
END $$;

