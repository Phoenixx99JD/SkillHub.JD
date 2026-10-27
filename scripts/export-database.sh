#!/usr/bin/env bash
# Exports the SkillHub database (structure + data) to database/skillhub.sql.
# Usage: ./scripts/export-database.sh [host] [port] [user] [password]
#   Docker:  ./scripts/export-database.sh            (uses the skillhub-mysql container)
#   Local:   ./scripts/export-database.sh localhost 3306 root mypassword
set -euo pipefail
cd "$(dirname "$0")/.."
mkdir -p database

if [ $# -eq 0 ]; then
  docker exec skillhub-mysql mysqldump -uroot -proot --routines --triggers --databases skillhub > database/skillhub.sql
else
  mysqldump -h "$1" -P "$2" -u "$3" -p"$4" --routines --triggers --databases skillhub > database/skillhub.sql
fi
echo "Database exported to database/skillhub.sql"
