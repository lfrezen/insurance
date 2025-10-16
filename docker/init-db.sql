-- Create databases for the microservices
CREATE DATABASE "ProposalServiceDb";
CREATE DATABASE "ContractServiceDb";

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE "ProposalServiceDb" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "ContractServiceDb" TO postgres;
