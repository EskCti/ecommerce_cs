#!/usr/bin/env bash
set -euo pipefail

BACKUP_DIR="${BACKUP_DIR:-./backups}"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
DB_NAME="${POSTGRES_DB:-sas_vendas}"
UPLOADS_PATH="${UPLOADS_PATH:-./uploads}"

mkdir -p "$BACKUP_DIR"

echo "==> Backing up database $DB_NAME"
pg_dump -Fc "$DB_NAME" > "$BACKUP_DIR/sas-${TIMESTAMP}.dump"
sha256sum "$BACKUP_DIR/sas-${TIMESTAMP}.dump" | tee "$BACKUP_DIR/sas-${TIMESTAMP}.dump.sha256"

if [ -d "$UPLOADS_PATH" ]; then
  echo "==> Backing up uploads from $UPLOADS_PATH"
  tar -czf "$BACKUP_DIR/uploads-${TIMESTAMP}.tar.gz" -C "$(dirname "$UPLOADS_PATH")" "$(basename "$UPLOADS_PATH")"
  sha256sum "$BACKUP_DIR/uploads-${TIMESTAMP}.tar.gz" | tee "$BACKUP_DIR/uploads-${TIMESTAMP}.tar.gz.sha256"
else
  echo "WARN: uploads path not found: $UPLOADS_PATH"
fi

echo "==> Backup complete in $BACKUP_DIR"
echo "Next: restore test on staging before production cutover."
