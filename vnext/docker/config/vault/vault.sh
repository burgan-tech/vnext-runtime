#!/bin/sh

# Let's wait for the Vault server to start.
sleep 5

# Check your secret endpoint
SECRET_CHECK=$(curl -s -o /dev/null -w "%{http_code}" -X GET 'http://vnext-vault:8200/v1/secret/data/workflow-secret' -H "X-Vault-Token: admin")

# If there is no secret, create it and set the relevant keys.
if [ "$SECRET_CHECK" -ne 200 ]; then
  curl -X POST 'http://vnext-vault:8200/v1/secret/data/workflow-secret' \
  -H "Content-Type: application/json" \
  -H "X-Vault-Token: admin" \
  -d '{
    "data": {
      "ConnectionStrings:Default": "Host=localhost;Port=5432;Database=Aether_WorkflowDb;Username=postgres;Password=postgres;"
    }
  }'
else
  echo "Secret 'workflow-secret' already exists."
fi