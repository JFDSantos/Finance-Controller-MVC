#!/usr/bin/env bash
# Source me: source scripts/dev-env.sh
export DOTNET_ROOT="${DOTNET_ROOT:-/usr/lib/dotnet}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export PATH="$PATH:$HOME/.dotnet/tools"
if ! command -v dotnet-ef >/dev/null 2>&1; then
  dotnet tool install -g dotnet-ef --version 9.0.10 >/dev/null 2>&1
fi
if ! pg_isready -q 2>/dev/null; then
  service postgresql start >/dev/null 2>&1
  for _ in $(seq 1 20); do pg_isready -q && break; sleep 1; done
fi
# dev-only credentials for the local test database
su postgres -c "psql -qc \"ALTER USER postgres PASSWORD 'postgres'\"" >/dev/null 2>&1
if ! su postgres -c "psql -tAc \"SELECT 1 FROM pg_database WHERE datname='finance_test'\"" 2>/dev/null | grep -q 1; then
  su postgres -c "createdb finance_test" >/dev/null 2>&1
fi
export TEST_DB_CONNECTION="Host=localhost;Database=finance_test;Username=postgres;Password=postgres"
